////////////////////////////////////////////////////////////////
// MonoBrowser Core                                          
////////////////////////////////////////////////////////////////
// Author: Yury Romanov
//
// https://monobrowser.org
//
// https://github.com/romanov/monobrowser
//
// (c) 2024                        
////////////////////////////////////////////////////////////////

namespace Romanov.MonoBrowserCore

open System
open System.Diagnostics
open System.IO
open System.Net.Http
open System.Threading
open Apos.Input
open BasicRectangle
open Camera
open FontStashSharp
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open BasicData
open RenderElementMethods

type BrowserComponent(game, window:Rectangle) as x =
    inherit DrawableGameComponent(game)

    let mutable _fontSystem = Unchecked.defaultof<FontSystem>
    let mutable _fontSystemBold = Unchecked.defaultof<FontSystem>

    let mutable camera: Camera2D = Unchecked.defaultof<Camera2D>
    let debug = AnyCondition(KeyboardCondition(Keys.OemTilde))
    let refreshBtn = AnyCondition(KeyboardCondition(Keys.F5))

    // EVENTS
    
    let browserReadyEvent = Event<EventHandler<_>, string>()
    
    let clickLinkEvent = Event<EventHandler<_>, string>()
    
    let pageLoadedEvent = Event<EventHandler<_>, string>()
    
    let mutable mouseRect = Rectangle.Empty
    
    let borderRect = BorderRectangle()
    let filledRect = BorderBackground()

    let mutable loadingThread = Unchecked.defaultof<Thread>

    //     graphics.PreparingDeviceSettings.Add(fun e ->
    //         e.GraphicsDeviceInformation.PresentationParameters.PresentationInterval <- PresentInterval.One)

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable isScrollbarVisible = false
    let mutable isDebugAllowed = false
    let mutable isActive = false
    let mutable isScrollingEnabled = true
    let mutable isNavbarEnabled = false
    
    let mutable navbarText = "https://google.com"

    // pixels of scroll target movement per wheel notch
    let scrollStep = 40f

    // last loaded payload, used by F5 to reload the current page
    let mutable lastPayload : BrowserUrl option = None

    // set when a new page loads (off-thread) so Update can reset scroll on the game thread
    let mutable pendingScrollReset = false

    member x.EnableScrollbar with set (value) = isScrollbarVisible <- value
    
    member x.EnableNavbar with set (value) = isNavbarEnabled <- value
    
    member x.EnableDebug with set (value) = isDebugAllowed <- value
    
    /// enable scrolling
    member x.AllowScroll with set (value) = isScrollingEnabled <- value
     
     
    member x.IsActive with get() = isActive
    
    /// Allow images for remote documents. Warning: the app will download and resize images for Texture2D
    member x.DisableImages with set value = Global.AllowImages <- not(value)
    
    [<CLIEvent>]
    member x.OnReady = 
        browserReadyEvent.Publish
        
    [<CLIEvent>]
    member x.OnLinkClicked = 
        clickLinkEvent.Publish
        
        
    /// fires when remote link, file or local content loaded to the page    
    [<CLIEvent>]
    member x.OnContentLoaded = 
        pageLoadedEvent.Publish
    
    override x.Initialize() =
       
        //x.IsFixedTimeStep <- true //Force the game to update at fixed time intervals
        //x.TargetElapsedTime <- TimeSpan.FromMilliseconds(1.0 / 60.0)

        spriteBatch <- new SpriteBatch(x.GraphicsDevice)
        camera <- Camera2D(x.GraphicsDevice.Viewport, x.GraphicsDevice)
        
        Global.Window <- window
        Global.WindowWidth <- window.Width
        Global.WindowHeight <- window.Height
        
        Global.MaxRenderWidth <- window.Width - ( Global.WindowPadding.Y * 2)

        spriteBatch.GraphicsDevice.ScissorRectangle <- window

        browserReadyEvent.Trigger(null, "done")

        base.Initialize()

        ()

    override x.LoadContent() =

        x.SetupDefaultFonts()
        
        //let textureStream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("MyProject.Resources.myimage.png");
        //let texture = Texture2D.FromStream(game.GraphicsDevice, textureStream)
        
        let folder = AppDomain.CurrentDomain.BaseDirectory
        
        _fontSystem <- new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(Path.Combine(folder, "Content", "Fonts", "regular.ttf")))
        
        _fontSystemBold <- new FontSystem();
        _fontSystemBold.AddFont(File.ReadAllBytes(Path.Combine(folder, "Content", "Fonts", "bold.ttf")))

        // default fonts        
        Global.Fonts.Add("default", _fontSystem.GetFont(20f))
        Global.Fonts.Add("header1", _fontSystemBold.GetFont(38f))
        Global.Fonts.Add("header2", _fontSystemBold.GetFont(26f))
        
        // helpers
        borderRect.LoadContent(game.GraphicsDevice)
        filledRect.LoadContent(game.GraphicsDevice)
        
        InputHelper.Setup(game)
        
        ImageLoader.Setup(game.GraphicsDevice)
        
        ()
        
    member private x.SetupDefaultFonts() =

         let path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Fonts")

         // CreateDirectory is a no-op when the folder already exists
         Directory.CreateDirectory(path) |> ignore

         let fonts = [|
             "https://github.com/googlefonts/opensans/raw/refs/heads/main/fonts/ttf/OpenSans-Regular.ttf"
             "https://github.com/googlefonts/opensans/raw/refs/heads/main/fonts/ttf/OpenSans-Bold.ttf"
             "https://github.com/googlefonts/opensans/raw/refs/heads/main/fonts/ttf/OpenSans-Light.ttf"
         |]

         let localFileFor (item:string) =
             let filename = (Path.GetFileName(Uri(item).AbsolutePath).Split("-")[1]).ToLowerInvariant()
             Path.Combine(path, filename)

         let missing = fonts |> Array.filter (fun item -> not (File.Exists(localFileFor item)))

         // Only the internet can supply missing fonts; fail clearly if it is unavailable
         if missing.Length > 0 && not (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) then
             raise (Exception("Font files are missing. Create a 'Content/Fonts' folder with 'regular.ttf' and 'bold.ttf', or connect to the internet so they can be downloaded."))

         for item in missing do
             let localFile = localFileFor item

             use client = new HttpClient()
             use s = client.GetStreamAsync(item).Result
             use fs = new FileStream(localFile, FileMode.OpenOrCreate)
             s.CopyToAsync(fs).Wait()

         ()
        
    
   
    
   
        
    member private x.LoadPage(payload:BrowserUrl, forceRefresh:bool) =

        Global.Page.Clear()

        let data = match payload with
                    | FromRemote url -> Book.GetPage(url, false, false, forceRefresh)
                    | FromLocal localFile -> Book.GetPage(localFile, true, false, forceRefresh)
                    | FromString text -> Book.GetFromString(text)

        for item in data.Children do
            Global.Page.Add(item)

        //Builder.showTree (None) (page1) (0)

        lastPayload <- Some payload
        pendingScrollReset <- true
        isActive <- true
        
        let info = match payload with
                    | FromRemote url -> "Link"
                    | FromLocal localFile -> "Local file"
                    | FromString text -> "Content"
        
        pageLoadedEvent.Trigger(null, info)
        
        ()

    /// Close window and stop listening to events
    member x.Close() =
        isActive <- false
        Global.Page.Clear()
        loadingThread <- null
        
    
    member x.FromString(text:string) =
        loadingThread <- Thread(ParameterizedThreadStart(fun _ -> x.LoadPage(BrowserUrl.FromString(text), false)))
        loadingThread.Start()

    /// Load remote file into the window
    member x.Navigate(url:string) =
        loadingThread <- Thread(ParameterizedThreadStart(fun _ -> x.LoadPage(BrowserUrl.FromRemote(url), false)))
        loadingThread.Start()
        

    /// Load local file
    member x.LoadFile(path:string) =
        loadingThread <- Thread(ParameterizedThreadStart(fun _ -> x.LoadPage(BrowserUrl.FromLocal(path), false)))
        loadingThread.Start()


    override x.Update(gameTime) =

        // skip all input/scroll work while there is no active page
        if isActive then

            InputHelper.UpdateSetup()

            // F5 reloads the current page, bypassing the cache
            if refreshBtn.Pressed() && isDebugAllowed then
                match lastPayload with
                | Some payload ->
                    loadingThread <- Thread(ParameterizedThreadStart(fun _ -> x.LoadPage(payload, true)))
                    loadingThread.Start()
                | None -> ()

            if debug.Pressed() && isDebugAllowed then
                Global.IsDebug <- not(Global.IsDebug)

            // a newly loaded page starts at the top
            if pendingScrollReset then
                camera.SnapScroll(0f)
                pendingScrollReset <- false

            // wheel sets a scroll target; the camera eases toward it below
            if isScrollingEnabled && MouseCondition.Scrolled() then
                let sc = MouseCondition.ScrollDelta
                let maxScroll =
                    max 0f (float32 (Global.ContentHeight + Global.WindowPadding.Y - Global.WindowHeight))

                if sc > 0 then camera.ScrollBy(-scrollStep, maxScroll)      // wheel up -> toward top
                elif sc < 0 then camera.ScrollBy(scrollStep, maxScroll)     // wheel down -> toward bottom

            // smoothly approach the scroll target every frame
            camera.UpdateScroll(float32 gameTime.ElapsedGameTime.TotalSeconds)

            let invert = InputHelper.NewMouse.Position + Point(0, int camera.Position.Y)

            mouseRect <- Rectangle(invert, Point(5,5))

            //InputHelper.TextEvents.ForEach(fun x-> printfn $"{x.Character}")

            InputHelper.UpdateCleanup()

        ()

    override x.Draw(gameTime) =
        
        // let fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
        // Console.WriteLine(fps)
        //x.GraphicsDevice.Clear (Color(214, 214, 214))  //#999999
     
        if x.IsActive then do
            
            let clippingRectangle = spriteBatch.GraphicsDevice.ScissorRectangle;
 
            spriteBatch.GraphicsDevice.ScissorRectangle <- window
            
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, new RasterizerState(ScissorTestEnable = true))
            
            filledRect.Draw(spriteBatch, window, Color.White)
            
            spriteBatch.End()
            
            
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, new RasterizerState(ScissorTestEnable = true), null, camera.ViewMatrix)
          
            for item in Global.Page do
                x.DrawElement(item, spriteBatch, borderRect, filledRect, Global.Fonts["default"])
                
            if Global.IsDebug then do
                for item in Global.DebugData do
                    spriteBatch.DrawString(Global.Fonts["default"], item.Text, item.Outline.Location.ToVector2(), Color.Red) |> ignore    
                
            
            //filledRect.Draw(spriteBatch, mouseRect, Color.Red)    
                
            spriteBatch.End()
            
            
            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, new RasterizerState(ScissorTestEnable = true))
            
            if isNavbarEnabled then do
                x.DrawNavbar(spriteBatch)
            
            if isScrollbarVisible then do
                x.DrawScrollbar(spriteBatch)
            
            spriteBatch.End()
            
            spriteBatch.GraphicsDevice.ScissorRectangle <- clippingRectangle
        
        else do
            
            ()
        ()
        
    member private x.DrawNavbar(spriteBatch:SpriteBatch) =
        filledRect.Draw(spriteBatch, Rectangle(10, 0, window.Width - 20, 40), Color.LightGray)
        borderRect.Draw(spriteBatch, Rectangle(10, 0, window.Width - 20, 40), Color.Black, 1)
        spriteBatch.DrawString(Global.Fonts["default"], navbarText, Vector2(15f,15f), Color.Black) |> ignore
        ()
        
        
    member private x.DrawScrollbar(spriteBatch:SpriteBatch) =
         
         if Global.ContentHeight > 0 then do
            // TODO scroller pos fix
            let steps = Math.Round((float Global.ContentHeight - float window.Height - float window.Y) / float 40) + 1.0
            let thick = (float window.Height / steps)
            let scroller_y = (float thick) * Math.Round(float camera.Position.Y / float 40)
            filledRect.Draw(spriteBatch, Rectangle(window.Width + 2, window.Top + 3, 8, window.Height - 5), ColorHelper.FromHex("#f5f5f5"))
            filledRect.Draw(spriteBatch, Rectangle(window.Width + 2, int scroller_y, 8, int thick), Color.Gray)
         
        
        
        
    member x.DrawElement(element:RenderElement, spriteBatch:SpriteBatch, border:BorderRectangle, backRect:BorderBackground, font1:DynamicSpriteFont) =
        
        if MouseCondition.Released(MouseButton.LeftButton) && element.IsOverMe(mouseRect) then do
                
                match element.Payload with
                        | LinkNode (_, url) -> clickLinkEvent.Trigger(null, url)
                        | _ -> (
                            printfn $"{element.Tag} = {element.Outline}"
                            )                
        
   

        
        match element.Payload with
          
             | TextNode(text, color, font)  -> (
                 spriteBatch.DrawString(Global.Fonts[font], text, element.Outline.Location.ToVector2(), color) |> ignore
                )
             
              | LinkNode(text, _)  -> (
                 spriteBatch.DrawString(Global.Fonts["default"], text, element.Outline.Location.ToVector2(), Color.Blue) |> ignore
                )
             
             | BODY when Global.IsDebug -> backRect.Draw(spriteBatch, element.Outline, Color.LightYellow)
             | IMG(url) -> spriteBatch.Draw(ImageLoader.GetImage(url), element.Outline, Color.White)
             | BLOCKQUOTE _ -> backRect.Draw(spriteBatch, element.Outline, Color.Beige)
             | CODE _  -> backRect.Draw(spriteBatch, element.Outline, Color(245, 247, 249))
             | _ -> ()
             
        match (element.Display, Global.IsDebug) with
            | Block, true when not(element.Tag = "BODY") -> border.Draw(spriteBatch, element.Outline, Color.Pink, 1)
            | Inline, true -> border.Draw(spriteBatch, element.Outline, Color.Blue, 1)
            | Anon, true -> border.Draw(spriteBatch, element.Outline, Color.Red, 1)
            | _ -> ()
             
      
        for child in element.Children do
            x.DrawElement(child, spriteBatch, border, backRect, font1)
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

    let clickLinkEvent = Event<EventHandler<_>,string>()
    
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
    
    member x.EnableScrollbar with set (value) = isScrollbarVisible <- value
    
    member x.EnableNavbar with set (value) = isNavbarEnabled <- value
    
    member x.EnableDebug with set (value) = isDebugAllowed <- value
    
    /// enable scrolling
    member x.AllowScroll with set (value) = isScrollingEnabled <- value
     
     
    member x.IsActive with get() = isActive
    
    /// Allow images for remote documents. Warning: the app will download and resize images for Texture2D
    member x.DisableImages with set value = Global.AllowImages <- not(value)
    
    [<CLIEvent>]
    member x.OnLinkClicked = 
        clickLinkEvent.Publish
    
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

        base.Initialize()

        ()

    override x.LoadContent() =

        x.SetupDefaultFonts()
        
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
         
         if not(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()) then do
             raise (Exception("Please, create 'Content/Fonts' folder and put files 'regular.ttf' and 'bold.ttf'"))
         
         let path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Fonts")
         
         if not(Directory.Exists(path)) then do
            Directory.CreateDirectory(path) |> ignore
         
         // check fonts
                
               
            let fonts = [|
                "https://monobrowser.org/fonts/regular.ttf"
                "https://monobrowser.org/fonts/bold.ttf"
            |]
            
            
            for item in fonts do
            
                let uri = Uri(item)
                let filename = Path.GetFileName(uri.AbsolutePath)
                
                let localFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Fonts", filename)
                
                if not(File.Exists(localFile)) then do
                
                    use client = new HttpClient()
                    use s = client.GetStreamAsync(item).Result
                    use fs = new FileStream(localFile, FileMode.OpenOrCreate)
                    s.CopyToAsync(fs).Wait()
            
         ()
        
    
   
    
   
        
    member private x.LoadPage(url:string, isLocal:bool) =
        Global.Page.Clear()
        
        let data = Book.GetPage(url, isLocal, false)
                
        for item in data.Children do
            Global.Page.Add(item)
            
        //Builder.showTree (None) (page1) (0)
        
        isActive <- true
        ()

    /// Close window and stop listening to events
    member x.Close() =
        isActive <- false
        Global.Page.Clear()
        loadingThread <- null
        
    
    /// Load remote file into the window
    member x.Navigate(url:string) =
    
        loadingThread <- Thread(ParameterizedThreadStart(fun _ -> x.LoadPage(url, false)))
        loadingThread.Start()
        

    /// Load local file
    member x.LoadFile(path:string) =
        loadingThread <- Thread(ParameterizedThreadStart(fun _ -> x.LoadPage(path, true)))
        loadingThread.Start()
    
    
    override x.Update(gameTime) =

        if not(isActive) then do
            ()
        
        // TODO: Add your update logic here
        InputHelper.UpdateSetup()

        if refreshBtn.Pressed() && isDebugAllowed then
            ()
        
        if debug.Pressed() && isDebugAllowed then
            Global.IsDebug <- not(Global.IsDebug)
          
        if MouseCondition.Scrolled() then
            do
                let sc = MouseCondition.ScrollDelta

                if sc > 100 then
                    camera.MoveCamera(Vector2(0f, 40f))
                else
                    if (int camera.Position.Y + Global.WindowHeight) <= Global.ContentHeight + Global.WindowPadding.Y then
                        camera.MoveCamera(Vector2(0f, -40f))


        let invert = InputHelper.NewMouse.Position + Point(0, int camera.Position.Y);
        
        mouseRect <- Rectangle(invert, Point(5,5))
        
        InputHelper.TextEvents.ForEach(fun x-> printfn $"{x.Character}")
        
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
            
            // TODO scroller fix
            let steps = Math.Round((float Global.ContentHeight - float window.Height - float window.Y) / float 40) + 1.0
            let thick = (float window.Height / steps)
            let scroller_y = (float thick) * Math.Round(float camera.Position.Y / float 40)
            
            filledRect.Draw(spriteBatch, Rectangle(window.Width - 2, window.Top + 3, 8, window.Height - 5), ColorHelper.FromHex("#f5f5f5"))
         
            filledRect.Draw(spriteBatch, Rectangle(window.Width - 2, int scroller_y, 8, int thick), Color.Gray)
         
        
        
        
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
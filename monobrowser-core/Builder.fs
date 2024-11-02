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

module Builder

open System
open System.Collections.Generic
open System.Text
open AngleSharp.Dom
open AngleSharp.Html.Dom
open BasicData
open Microsoft.Xna.Framework
open Creation
open RenderElementMethods



let mutable top = 0

let imagesList = List<string>()

let rec showTree (parent: RenderElement option) (element: RenderElement) (lvl: int) =

    let sb = StringBuilder()
    let mutable left = 0

    for i in 0..lvl do
        left <- 10 + (lvl * 10)
        sb.Append("X") |> ignore

    let debug: DebugTreeItem =
        { Outline = Rectangle(left, top, 100, 20)
          Text = element.Tag }

    if not (element.Tag = "Text") then
        do
            Global.DebugData.Add(debug)
            top <- top + 20


    let parentTag =
        match parent with
        | Some(root) -> root.Tag
        | _ -> ""

    element.Children |> Array.iter (fun x -> showTree (Some(element)) x (lvl + 1))

let mutable lastItem = 0

let createChildNodesWithPrepend (nodes: INodeList, maxWidth: int, font: string, childMode: ChildMode) =

    let appendBlocks = ResizeArray<RenderElement>()

    let blocks = ResizeArray<TextData>()

    let mutable isFirst = true
    
    for item in nodes do

        let textContent =
            match childMode, isFirst with
            | Number, _ when not(item.NodeName = "CODE") -> (
                                                                     let text = lastItem.ToString() + ". " + item.TextContent
                                                                     lastItem <- lastItem + 1
                                                                     text)
            | Bullet, true -> ("• " + item.TextContent)
            | Code, _ -> item.TextContent
            | _ -> item.TextContent

        isFirst <- false
        
        if item.NodeName = "IMG" && Global.AllowImages then
            do

                let image = item :?> IHtmlImageElement

                ImageLoader.PreloadImage(image.Source, Global.MaxRenderWidth)

                let size = ImageLoader.GetImageSize(image.Source)

                let imageEl =
                    { Tag = "IMG"
                      Outline = Rectangle(0, 0, size.Width, size.Height)
                      Children = Array.empty
                      Payload = IMG(image.Source)
                      Display = DisplayMode.Block
                      Padding = BoxPad.Zero
                      Margin =
                        { Top = 20
                          Left = 0
                          Right = 0
                          Bottom = 20 }
                      IsClickable = false }

                appendBlocks.Add(imageEl)

                ()


        if item.NodeName = "#text" then
            do

                let i1 =
                    { Text = textContent
                      TextType = TextType.Default }

                blocks.Add(i1)
        
        if item.NodeName = "BR" then do
            blocks.Add({ Text = textContent; TextType = TextType.Newline })
                
                
        if item.NodeName = "EM" then
            do
                blocks.Add({ Text = textContent; TextType = TextType.Accent })

        if item.NodeName = "CODE" then
            do

                let icode =
                    { Text = item.TextContent
                      TextType = TextType.Code }


                blocks.Add(icode)


        if item.NodeName = "A" then
            do

                let link = item :?> IHtmlAnchorElement

                let ilink =
                    { Text = textContent
                      TextType = TextType.Link(link.Href) }

                blocks.Add(ilink)



        if item.NodeName = "STRONG" then
            do

                let i2 =
                    { Text = textContent
                      TextType = TextType.Strong }

                blocks.Add(i2)

    let code =
        match childMode with
        | ChildMode.Code -> true
        | _ -> false

    
    let proccess = Queue<TextData>(blocks)
    let chunck = ResizeArray<TextData>()
    
    while proccess.Count > 0 do
        let item = proccess.Dequeue()
        
        if item.TextType = TextType.Newline then do
            appendBlocks.AddRange(CreateTextBlock(chunck.ToArray(), font, maxWidth, code))
            chunck.Clear()
        else
            chunck.Add(item)
            
    appendBlocks.AddRange(CreateTextBlock(chunck.ToArray(), font, maxWidth, code))
    appendBlocks.ToArray()


let createChildNodes (nodes: INodeList, maxWidth: int, font: string) =
    createChildNodesWithPrepend (nodes, maxWidth, font, ChildMode.Empty)


let rec CreateElement (inputElement: IElement, font: string[]) : RenderElement =
                                
    let children =
        inputElement.Children
        |> Seq.map (fun x -> CreateElement(x, font))
        |> Seq.toArray
    
    let element: RenderElement =
        
        match inputElement.NodeName with


        | "#text" ->
            { Tag = inputElement.NodeName + " " + RandomHelp.CreateString(5)
              Outline = Rectangle(Global.WindowPadding.X, 0, Global.MaxRenderWidth, 0)
              Children = children
              Payload = TextNode(inputElement.TextContent, Color.White, "default")
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Margin = BoxPad.Zero
              IsClickable = false }


        | "BODY" ->
            { Tag = inputElement.NodeName + " " + RandomHelp.CreateString(5)
              Outline = Rectangle(Global.WindowPadding.X, 0, Global.MaxRenderWidth, 0)
              Children = children
              Payload = BODY
              Display = DisplayMode.Block
              Padding =
                { Top = 0
                  Left = 0
                  Right = 0
                  Bottom = 0 }
              Margin = {
                  Top = 0 
                  Left = 0
                  Right = 0
                  Bottom = 0 
              }
              IsClickable = false }

        // | "IMG"  -> (
        //
        //             let image = inputElement :?> IHtmlImageElement
        //
        //             imagesList.Add(image.Source)
        //
        //             {
        //             Tag = inputElement.NodeName + " " + RandomHelp.CreateString(5)
        //             Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
        //             Children = children
        //             Payload = IMG(image.Source)
        //             Display = DisplayMode.Block
        //             Padding = BoxPad.Zero
        //             Margin =  { Top = 0; Left = 0; Right = 0; Bottom = 0 }
        //             IsClickable = false
        //          }
        //
        //             )


        | "H1" ->
            { Tag = inputElement.NodeName + " " + RandomHelp.CreateString(5)
              Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
              Children = children
              Payload = Header(inputElement.ChildNodes, "header1")
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Margin =
                { Top = 0
                  Left = 0
                  Right = 0
                  Bottom = 10 }
              IsClickable = false }



        | "H2"
        | "H3" ->
            { Tag = inputElement.NodeName + " " + RandomHelp.CreateString(5)
              Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
              Children = children
              Payload = Header(inputElement.ChildNodes, "header2")
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Margin =
                { Top = 20
                  Left = 0
                  Right = 0
                  Bottom = 10 }
              IsClickable = false }


        | "BLOCKQUOTE" ->
            { Tag = inputElement.NodeName
              Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
              Payload = BLOCKQUOTE(inputElement.ChildNodes)
              Display = DisplayMode.Block
              Padding =
                { Top = 30
                  Left = 25
                  Right = 0
                  Bottom = 30 }
              Children = children
              Margin =
                { Top = 10
                  Bottom = 10
                  Left = 0
                  Right = 0 }
              IsClickable = false }

        | "CODE" ->
            { Tag = inputElement.NodeName
              Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
              Payload = CODE(inputElement.ChildNodes)
              Display = DisplayMode.Block
              Children = children
              Padding =
                { Top = 30
                  Left = 25
                  Right = 0
                  Bottom = 30 }
              Margin =
                { Top = 10
                  Bottom = 10
                  Left = 0
                  Right = 0 }
              IsClickable = false }

        | "UL" ->
            
            { Tag = inputElement.NodeName
              Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
              Payload = UL(RandomHelp.CreateString(10))
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Children = children
              Margin = BoxPad.Zero
              IsClickable = false }
            
        | "OL" ->
            
            let start = (inputElement :?> IHtmlOrderedListElement).Start

            { Tag = inputElement.NodeName
              Outline = Rectangle(0, 0, Global.MaxRenderWidth, 0)
              Payload = OL(start)
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Children = children
              Margin = BoxPad.Zero
              IsClickable = false }

        | "LI" ->
            { Tag = inputElement.NodeName
              Outline = Rectangle.Empty
              Payload = LI(inputElement.ChildNodes)
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Children = children
              Margin = BoxPad.Zero
              IsClickable = false }

        | "P" ->
            { Tag = inputElement.NodeName + " " + RandomHelp.CreateString(5)
              Outline = Rectangle.Empty
              Children = children
              Payload = Paragraph(inputElement.ChildNodes)
              Display = DisplayMode.Block
              Padding = BoxPad.Zero
              Margin =
                { Left = 0
                  Top = 0
                  Right = 0
                  Bottom = 0 }
              IsClickable = false }

        | _ ->
            { Tag = "None"
              Outline = Rectangle.Empty
              Children = Array.empty
              Payload = NotSet
              Display = DisplayMode.Anon
              Padding = BoxPad.Zero
              Margin =
                { Left = 0
                  Top = 0
                  Right = 0
                  Bottom = 0 }
              IsClickable = false }


    { element with Children = children }
                            



let rec AddTextNodes (rootElement: RenderElement option) (element: RenderElement) : RenderElement =

    //let newChildren =
        //element.Children |> Array.map (fun x -> AddTextNodes (Some(element)) x)

    let maxWidth =
        match rootElement with
        | Some(parent) ->
            
            parent.Outline.Width
            - parent.Margin.Left
            - parent.Margin.Right
            - parent.Padding.Left
            - parent.Padding.Right
            - element.Margin.Left
            - element.Margin.Right
            - element.Padding.Left
            - element.Padding.Right
            
        | _ -> element.Outline.Width
               - element.Margin.Left
               - element.Margin.Right
               - element.Padding.Left
               - element.Padding.Right

    let listMode = match rootElement with
                    | Some(parent) ->
                        match parent.Payload with
                                | OL _ -> ChildMode.Number
                                | UL _ -> ChildMode.Bullet
                                | _ -> ChildMode.Empty
                    | None -> ChildMode.Empty
    
    
    let refreshedChildren =
        match element.Payload with
        | Header(childNodes, font) -> createChildNodes (childNodes, maxWidth, font)
        | Paragraph(childNodes) -> createChildNodes (childNodes, maxWidth, "default")
        | LI(childNodes) -> createChildNodesWithPrepend (childNodes, maxWidth, "default", listMode)
        | CODE(childNodes) -> createChildNodesWithPrepend (childNodes, maxWidth, "default", ChildMode.Code)
        | OL start -> (
                        lastItem <- start
                        element.Children |> Array.map (fun x -> AddTextNodes (Some(element)) x))
        | _ -> element.Children |> Array.map (fun x -> AddTextNodes (Some(element)) x)
        
    { element with Children = refreshedChildren }



let rec AddSize (element: RenderElement) : RenderElement =

    let newChildren = element.Children |> Array.map AddSize

    let childrenHeight =
        newChildren
        |> Array.filter (fun x -> not (x.Display = DisplayMode.Inline))
        |> Array.map (_.Outline)
        |> Array.sumBy (_.Height)

    // let childrenPaddingTop =
    //     newChildren
    //     |> Seq.filter (fun x -> not (x.Display = DisplayMode.Inline))
    //     |> Seq.map (_.Padding)
    //     |> Seq.sumBy (fun x -> x.Left + x.Right)

    let basicSize =
        match element.Display with
        | _ ->
            element.Outline.Size
            - Point(element.Margin.Left, 0)
            - Point(element.Margin.Right, 0)
            + Point(0, childrenHeight)
            + Point(0, element.Padding.Top + element.Padding.Bottom) // <- PADDING

    let newSize = Rectangle(element.Outline.Location, basicSize)

    if element.Payload = BODY then
        do
            Global.ContentHeight <- newSize.Height
            printfn $"max page height is {Global.ContentHeight}"

    match element.Payload with
    | TextNode _ -> element
    | _ ->
        { element with
            Outline = newSize
            Children = newChildren }

// Calculte position of each block + margin + padding
let rec RefreshPosition (element: RenderElement) (rootElement: RenderElement option) =

    let newPosition =
        match (element.Display, rootElement) with
        | DisplayMode.Block, None -> element.Outline.Location + Global.Window.Location + Global.WindowPadding
        | DisplayMode.Block, Some(parent)
        | DisplayMode.Anon, Some(parent) ->
            (
             let prevLocation = parent.PrevBlockPosition element
             Point(parent.Outline.X + parent.Padding.Left + parent.Margin.Left, prevLocation.Y)
            )
    
        | DisplayMode.Inline, Some(parent) ->
            (
                
             // inline blocks are always in anon blocks   
             let paddingLeft = parent.Outline.X + parent.Padding.Left + parent.Margin.Left + element.Outline.X
             Point(paddingLeft, parent.Outline.Y)

            )
        | _, _ -> Point.Zero

    //printfn $"{element.Tag}: position {newPosition}"

    let newOutline = Rectangle(newPosition, element.Outline.Size)

    element.Outline <- newOutline

    element.Children |> Array.iter (fun x -> RefreshPosition (x) (Some(element)))



/// create top and bottom margin via anonblocks
let rec AddMarginNodes (inputElement: RenderElement) : RenderElement =

    let refreshedChildren = inputElement.Children |> Array.map AddMarginNodes

    let topMargin =
        { Tag = "margin-top"
          Outline = Rectangle(0, 0, inputElement.Outline.Width, inputElement.Margin.Top)
          Children = Array.empty
          Payload = Margin
          Display = DisplayMode.Anon
          Padding = BoxPad.Zero
          Margin = BoxPad.Zero
          IsClickable = false }

    let botMargin =
        { Tag = "margin-bot"
          Outline = Rectangle(0, 0, inputElement.Outline.Width, inputElement.Margin.Bottom)
          Children = Array.empty
          Payload = Margin
          Display = DisplayMode.Anon
          Padding = BoxPad.Zero
          Margin = BoxPad.Zero
          IsClickable = false }

    let holderElement =
        { Tag = "margin-holder"
          Outline = Rectangle(0, 0, inputElement.Outline.Width, 0)
          Children =
            [| topMargin
               { inputElement with
                   Children = refreshedChildren }
               botMargin |]
          Payload = Margin
          Display = DisplayMode.Anon
          Padding = BoxPad.Zero
          Margin = BoxPad.Zero
          IsClickable = false }

    match (inputElement.Margin, inputElement.Payload) with
    | margin, _ when not(margin.Top = 0) || not(margin.Bottom = 0) -> holderElement
    | _ -> { inputElement with Children = refreshedChildren }



let rec AddPadding (inputElement: RenderElement) : RenderElement =

    let children = inputElement.Children |> Seq.map AddPadding |> Seq.toArray

    let topPadding =
        { Tag = "padding"
          Outline = Rectangle(0, 0, inputElement.Outline.Width, inputElement.Padding.Top)
          Children = Array.empty
          Payload = Padding
          Display = DisplayMode.Anon
          Padding = BoxPad.Zero
          Margin = BoxPad.Zero
          IsClickable = false }

    let bottomPadding =
        { Tag = "padding"
          Outline = Rectangle(0, 0, inputElement.Outline.Width, inputElement.Padding.Bottom)
          Children = Array.empty
          Payload = Padding
          Display = DisplayMode.Anon
          Padding = BoxPad.Zero
          Margin = BoxPad.Zero
          IsClickable = false }

    match inputElement.Padding with
    | pad when pad.Top > 0 && pad.Bottom = 0 ->
        { inputElement with
            Children = Array.append [| topPadding |] children }
    | pad when pad.Bottom > 0 && pad.Top = 0 ->
        { inputElement with
            Children = Array.append children [| bottomPadding |] }
    | pad when pad.Top > 0 && pad.Bottom > 0 ->
        (

         
         let newchilds = children |> ResizeArray
         newchilds.Insert(0, topPadding)
         newchilds.Add(bottomPadding)

         { inputElement with Children = newchilds.ToArray() })
    | _ ->
        { inputElement with Children = children }

//let childrenWithPadding = Array.append [| topPadding |] children

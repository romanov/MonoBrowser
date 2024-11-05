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

module BasicData

open System
open AngleSharp.Dom
open Microsoft.Xna.Framework

type Settings = {
    TextColor:string
    LinkColor:string
    BackgroundColor:string
    CodeColor:string
    CodeBackground:string
    BlockquoteBackground:string
}

type BrowserUrl =
    | FromRemote of string
    | FromLocal of string
    | FromString of string

type DebugTreeItem = {
    Outline:Rectangle
    Text:string
}

type TextType =
    | Empty
    | Default
    | Accent
    | Link of url:string
    | Strong
    | Header
    | Newline
    | Code
    | ActionLink of method:string

type TextData = {
    Text:string
    TextType:TextType
}

type Word = {
    Text:string
    TextType:TextType
    Height:int
    Width:int
}

type BoxPad = {
    Top:int
    Right:int
    Bottom:int
    Left:int
} with

    static member Zero =
        { Top = 0; Left  = 0; Right = 0; Bottom = 0 }

type Styles = {
    BackgroundColor:string
}

type ChildMode =
    | Empty
    | Bullet
    | Number
    | Code

type DisplayMode =
     | Anon
     | Inline
     | Block

type SimpleNode = {
    Tag:string
    Display:DisplayMode
    Children:SimpleNode[]
}

type HtmlElement =
    | NotSet
    | TextNode of text:string * color:Color * font:string
    | LinkNode of text:string * url:string
    | ActionLinkNode of text:string * methodName:string
    | Strong of text:string
    | Header of nodes:INodeList * font:string
    | Paragraph of nodes:INodeList
    | BODY
    | BLOCKQUOTE of nodes:INodeList
    | CODE of nodes:INodeList
    | UL of guid:string
    | OL of start:int
    | LI of nodes:INodeList 
    | Div
    | IMG of path:string
    | Html
    | Padding
    | Margin
    | Line
    | HR
    
type RenderElement = {
    Tag:string
    mutable Outline:Rectangle
    Children: RenderElement[]
    Payload:HtmlElement
    Display:DisplayMode
    Padding:BoxPad
    Margin:BoxPad
    IsClickable:bool
}
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

module HtmlData

type PositionMode =
    | Static
    | Relative
    | Absolute
    | Sticky

type DisplayMode =
    | Block
    | Inline
    | Flexbox of int
    | Grid

type ColorString =
    | Word of string
    | Hex of string

type Style =
    | Color of string
    | PixelWidth of int
    | PixelHeight of int
    | PercentageHeight of int * maxSize:int
    | PercentageWidth of int * maxSize:int
    | BackgroundHexColor of string
    | BackgroundWordColor of string
    | PositionTop of int
    | PositionLeft of int
    | Display of DisplayMode
    | Outline of int * string * ColorString
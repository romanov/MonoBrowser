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

module Global

open System.Collections.Concurrent
open System.Collections.Generic
open BasicData
open FontStashSharp
open Microsoft.Xna.Framework


let mutable Window = Rectangle.Empty

let mutable WindowPadding = Point(10,20)

let mutable IsDebug = false

let mutable AllowImages = true

// ---- Theme (light defaults) ----
// Page background painted behind the document. Supports alpha (e.g. transparent or
// semi-transparent black). Read at draw time, so it can change live.
let mutable BackgroundColor = Color.White
// Text colors. Baked into elements at parse time, so set the theme before Navigate.
let mutable TextColor = Color.Black
let mutable StrongColor = Color.Brown
let mutable LinkColor = Color.Blue
let mutable CodeColor = Color.Gray
// Backgrounds for code / blockquote blocks. Read at draw time.
let mutable CodeBackground = Color(245, 247, 249)
let mutable BlockquoteBackground = Color.Beige

// ---- Font sizes in px. Glyphs are rasterized once on load, so set these BEFORE Initialize. ----
let mutable FontSize = 20f
let mutable Header1Size = 38f
let mutable Header2Size = 26f

// Extra scrollable space (px) past the end of the content, so the last line can be
// scrolled clear of the bottom edge. Read at scroll time.
let mutable ScrollPaddingBottom = 0

let mutable ContentHeight = 0
let mutable MaxRenderWidth = 0

let mutable WindowWidth = 0
let mutable WindowHeight = 0

// Scrollbar settings
let mutable ScrollbarWidth = 5
let mutable ScrollbarColor = ColorHelper.FromHex("#f5f5f5")
let mutable ScrollbarTrackColor = Color.Gray

let Fonts = Dictionary<string, DynamicSpriteFont>()
let Page = ConcurrentBag<RenderElement>()
let DebugData = ConcurrentBag<DebugTreeItem>()
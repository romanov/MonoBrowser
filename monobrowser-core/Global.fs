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

let mutable ContentHeight = 0
let mutable MaxRenderWidth = 0

let mutable WindowWidth = 0
let mutable WindowHeight = 0


let Fonts = Dictionary<string, DynamicSpriteFont>()
let Page = ConcurrentBag<RenderElement>()

let DebugData = ConcurrentBag<DebugTreeItem>()


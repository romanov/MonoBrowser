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

module RenderElementMethods

open BasicData
open System.Linq
open Microsoft.Xna.Framework

// --- element construction ---------------------------------------------------
// Every RenderElement gets a stable unique Id. Rather than repeat the full
// record literal (and a random Tag) at each call site, build from `defaults`
// with copy-and-update and stamp a fresh Id via `mkElement`.

let private idLock = obj()
let private idCounter = ref 0

let private nextId () =
    lock idLock (fun () ->
        idCounter.Value <- idCounter.Value + 1
        idCounter.Value)

/// A neutral RenderElement template; override only the fields that differ,
/// then pipe through `mkElement` to receive a unique Id.
let defaults =
    { Id = 0
      Tag = ""
      Outline = Rectangle.Empty
      Children = Array.empty
      Payload = NotSet
      Display = DisplayMode.Block
      Padding = BoxPad.Zero
      Margin = BoxPad.Zero
      IsClickable = false }

/// Stamp a fresh unique Id onto an element template.
let mkElement (template: RenderElement) = { template with Id = nextId() }

type RenderElement with

    static member WrapBlock(children) =
        mkElement { defaults with Tag = "wrapper"; Children = children; Payload = HR }

    static member CreateLine() =
        mkElement { defaults with Tag = "HR"; Payload = HR }
    
    
    member this.IsOverMe(mouse:Rectangle) =
        if this.IsClickable then
            mouse.Intersects(this.Outline)
        else
            if Global.IsDebug then
                mouse.Intersects(this.Outline)
            else
            false
    
    member this.LocationWithOffset =
        this.Outline.Location
        
    member this.PrevBlockPosition (element:RenderElement) =
        
        let index = this.Children
                    |> Array.findIndex (fun x -> x.Id = element.Id)
        
        let item = this.Children
                   |> Array.tryItem (index - 1)
                   
        match item with
             | Some(element) -> Point(this.Outline.Location.X + this.Padding.Left, (element.Outline.Location.Y + element.Outline.Height))
             | None -> this.Outline.Location + Point(this.Padding.Left, this.Padding.Top)
             
             
    
    
    member this.LastAnon1 =
        
        let anons = this.Children.Count(fun x -> x.Display = DisplayMode.Anon)
        match anons with
            | 0 | 1 -> Point(0, 0)
            | _ -> (
                
                  let lastItem = this.Children.LastOrDefault(fun x -> x.Display = DisplayMode.Anon) |> DataValidation.ofNull
                  match lastItem with
                        | None -> Point.Zero
                        | Some(x) -> x.Outline.Size
                
                )
        
      
        
        
        
   
            
            
  
        
        
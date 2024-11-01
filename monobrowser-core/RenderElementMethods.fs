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

type RenderElement with

    static member WrapBlock(children) =
        {
            Tag = "wrapper"
            Outline = Rectangle.Empty
            Children = children
            Payload = HR
            Display = DisplayMode.Block
            Padding = BoxPad.Zero
            Margin = BoxPad.Zero
            IsClickable = false 
        }
        
    
    static member CreateLine() =
        
        {
            Tag = "HR"
            Outline = Rectangle.Empty
            Children = Array.empty
            Payload = HR
            Display = DisplayMode.Block
            Padding = BoxPad.Zero
            Margin = BoxPad.Zero
            IsClickable = false 
        }
    
    
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
                    |> Array.findIndex (fun x -> x = element)
        
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
        
      
        
        
        
   
            
            
  
        
        
module ColorHelper

open System
open System.Globalization
open Microsoft.Xna.Framework

let CalculatePercentage(screenSize:int, percentage:int) =
    let final = (decimal)screenSize * ((decimal)percentage / 100m)
    Convert.ToInt32(final)

let FromHex (color: string) =
    let hex = color.Replace("#", String.Empty)
    let h = NumberStyles.HexNumber

    let r = Int32.Parse(hex.Substring(0, 2), h)
    let g = Int32.Parse(hex.Substring(2, 2), h)
    let b = Int32.Parse(hex.Substring(4, 2), h)
    let mutable a = 255

    if hex.Length = 8 then
        a <- Int32.Parse(hex.Substring(6, 2), h)

    Color(r, g, b, a)

module RandomHelp

open System
open System.Linq

let CreateString(length:int) =
    let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
    let data = new string(Enumerable.Repeat(chars, length).Select(fun s -> s[System.Random.Shared.Next(s.Length)]).ToArray())
    data

let CreateNewsFileName(ext:string) =
    let date = DateTime.Now.ToString("dd_MM_yyyy")
    let name = "news_" + CreateString(15)
    let fileName = $"{date}_{name}{ext}"
    fileName
    
let CreateNumber(length:int) =
     let chars = "123456789"
     let data = new string(Enumerable.Repeat(chars, length).Select(fun s -> s[Random.Shared.Next(s.Length)]).ToArray())
     Convert.ToInt32(data)
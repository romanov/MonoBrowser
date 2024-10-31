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

module Book

open System
open System.Collections.Concurrent
open System.IO
open System.Net.Http
open System.Runtime.InteropServices
open AngleSharp.Html.Parser
open BasicData
open Markdig



let private pages = ConcurrentDictionary<string, RenderElement>()

let private convertMarkdownToRender(text:string, isHtml:bool) =
  
    let markdownToHtml =
        if isHtml then text
        else Markdown.ToHtml(text)
    
    let doc = HtmlParser().ParseDocument(markdownToHtml)
    
    let nodes =
            Builder.CreateElement (doc.Body, [| "default"; "header1"; "header2" |])
            |> Builder.AddMargin
            |> Builder.AddTextNodes None
            |> Builder.AddSize

    // TODO functional chain
    Builder.RefreshPosition nodes None
    nodes


let AddPage(url:string, isHtml:bool) =
    
    let name = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
    
    let text = match url with
                    | path when path.Contains("content://") -> (
                        
                        let path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", path.Replace("content://",""))
                        File.ReadAllTextAsync(path).Result
                        
                        )
                    | _ -> (
                            use client = new HttpClient()
                            client.GetStringAsync(url).Result
                        )

    let data = convertMarkdownToRender(text, isHtml)
    pages.TryAdd(name, data) |> ignore
    data
    


        
        
let GetLocalPage(file:string) =
    let path = Path.Combine(file)
    let file = File.ReadAllTextAsync(path).Result
    convertMarkdownToRender(file, true)
    
    
    // C# support    
let GetPage(url:string, isLocal:bool, [<Optional; DefaultParameterValue(false)>]isHtml: bool) =
    
    let name = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
    
    if isLocal then
        GetLocalPage(url)
    else 
        match pages.ContainsKey(name) with
            | true -> pages[name]
            | false -> AddPage(url, isHtml)
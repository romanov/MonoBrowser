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

module Creation

open System
open System.Collections.Generic
open System.Diagnostics
open BasicData
open Microsoft.Xna.Framework
open System.Linq

let private CreateTextNode(words:Word seq, gap:int, font:string) =
    
    let list = ResizeArray<RenderElement>()
    
    let mutable wordX = 0
    let mutable prevType = TextType.Empty
    
    for word in words do
    
        let color = match word.TextType with
                        | TextType.Strong -> Color.Brown
                        | TextType.Link _ | TextType.ActionLink _ -> Color.Blue
                        | TextType.Code -> Color.Gray
                        | _ -> Color.Black
     
        
    
            
        let clearFix = match word.Text with
                                | "," | ";" | ":" -> -5
                                | _ -> 0
                                
        let textBlock = match word.TextType with
                            
                            | TextType.Link url -> (
                                
                                  let linkBlock : RenderElement = {
                                            Tag = "Text"
                                            Payload = LinkNode(word.Text, url)
                                            Outline = Rectangle(wordX, 0, word.Width, word.Height)
                                            Children = Array.empty
                                            Display = DisplayMode.Inline
                                            Padding = BoxPad.Zero
                                            Margin =  BoxPad.Zero
                                            IsClickable = true
                                        }
                                  
                                  
                                  linkBlock
                                  
                                )
                            
                            | _ -> (
                                
                                  let textBlock : RenderElement = {
                                            Tag = "Text"
                                            Payload = TextNode(word.Text, color, font)
                                            Outline = Rectangle(wordX + clearFix, 0, word.Width, word.Height)
                                            Children = Array.empty
                                            Display = DisplayMode.Inline
                                            Padding = BoxPad.Zero
                                            Margin =  BoxPad.Zero
                                            IsClickable = false 
                                        }
                                
                                  textBlock
                                
                                )
        
                
      
        let hasWhitespace = word.Text.All(fun x -> Char.IsWhiteSpace(x))
        if not(hasWhitespace) then do
            wordX <- wordX + word.Width + gap
            list.Add(textBlock)
        
        
        prevType <- word.TextType
        
    list.ToArray()


let private putThisWordInCurrentLine(word:Word, wordGap:int, lineWidth:int, maxWidth:int) =
        lineWidth + (word.Width + wordGap) < maxWidth


let private createLine(words:Word[], line:int, gap:int, font:string) =
    
    let elementWidth = words |> Array.sumBy (_.Width)
    let elementHeight = words |> Array.maxBy (_.Height)
      
      // todo fix line width
    let anonBlock : RenderElement = {
        Tag = $"Line {line}" + " " + RandomHelp.CreateString(5)
        Payload = Line
        Outline = Rectangle(0, 0, elementWidth + (words.Length - 1) * gap, elementHeight.Height)
        Children = CreateTextNode(words, gap, font)
        Display = DisplayMode.Anon
        Padding = BoxPad.Zero
        Margin =  BoxPad.Zero
        IsClickable = false 
    }
    
    anonBlock

let private CreateTextNodes(words:Word seq, maxWidth:int, wordGap:int, font:string) =
    
    let textLines = ResizeArray<RenderElement>()
        
    let mutable lineNumber = 0
    let mutable lineWidth = 0
    
    let wordsLine = Dictionary<int, ResizeArray<Word>>()
    
    let pendingWords = Queue<Word>(words)
    
    while pendingWords.Count > 0 do
        
        let word = pendingWords.Dequeue()
        
        if putThisWordInCurrentLine(word, wordGap, lineWidth, maxWidth) then
            
            wordsLine.TryAdd(lineNumber, ResizeArray<Word>(1)) |> ignore
            wordsLine[lineNumber].Add(word)
        
        else
            
            lineWidth <- 0
            lineNumber <- lineNumber + 1
            
            wordsLine.TryAdd(lineNumber, ResizeArray<Word>(1)) |> ignore
            wordsLine[lineNumber].Add(word)
           
        lineWidth <- lineWidth + (word.Width + wordGap)           
            
         
        
        
    for item in wordsLine do
       
        let line = createLine(item.Value.ToArray(), item.Key, wordGap, font)
        textLines.Add(line)

        
    textLines.ToArray()

// each block converts into line
let private ProccessTextBlock(words:TextData[], font:string, maxWidth:int) =
    
    let gap = Global.Fonts[font].MeasureString(" ")
    
    let wordList = ResizeArray<Word>()
    
    for word in words do
        let wordDimensions = Global.Fonts[font].MeasureString(word.Text)
        wordList.Add( { Text = word.Text; Width = int wordDimensions.X; Height = int wordDimensions.Y; TextType = word.TextType } )
        
    let textNodes = CreateTextNodes(wordList, int maxWidth, int gap.X, font)
    
    textNodes


// TODO avoid hacky way for tabs
let private splitLine(line:string) =
    if line.StartsWith("    ") then
        let split = line.Split(null)
        split |> Array.append [|"‎ ‎ "|]
    else
        line.Split(null)




// TODO simplify logic, convert to functional    
let CreateTextBlock(inputBlocks:TextData[], font:string, maxWidth:int, isCode:bool) =
    
    let elements = ResizeArray<RenderElement>()
    
    let data = ResizeArray<TextData>()
    
    for item in inputBlocks do
        
        // chunks
        
        
        let lines = item.Text.Split([|"\n";|], StringSplitOptions.None)
        
        for line in lines do
                    
                    if isCode then do
                        let words = splitLine(line) |> Array.map (fun x -> { Text = x; TextType = item.TextType })
                        let codeblock = ProccessTextBlock(words, font, maxWidth)
                        elements.AddRange(codeblock)                
                    else
                        let words = splitLine(line) |> Array.map (fun x -> { Text = x; TextType = item.TextType })
                        data.AddRange(words)
                     
    if not(isCode) then
        let block = ProccessTextBlock(data.ToArray(), font, maxWidth)
        elements.AddRange(block)      
    
    elements.ToArray()
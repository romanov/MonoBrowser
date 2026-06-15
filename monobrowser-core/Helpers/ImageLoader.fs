module ImageLoader

open System
open System.Collections.Generic
open System.IO
open System.Net.Http
open Microsoft.Xna.Framework.Graphics
open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing

let private _images = Dictionary<string, Texture2D>()
let mutable private _graphics = Unchecked.defaultof<GraphicsDevice>
let mutable private _basePath = ""

let Setup(graphics:GraphicsDevice) =
    _graphics <- graphics

let private downloadImage(url:string) =
    use httpClient = new HttpClient()
    
    let uri = Uri(url)
    
    // Get the file extension
    let uriWithoutQuery = uri.GetLeftPart(UriPartial.Path)
    let fileExtension = Path.GetExtension(uriWithoutQuery)

    // Create file path and ensure directory exists
    //let path = Path.Combine(directoryPath, $"{fileName}{fileExtension}")
    //Directory.CreateDirectory(directoryPath) |> ignore
    
    let path = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString(); 

    let imageBytes = httpClient.GetByteArrayAsync(uri).Result
    File.WriteAllBytesAsync(path, imageBytes).Wait()
    path

let GetImageSize(url:string) =
      let imageName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
      let texture = _images[imageName]
      texture.Bounds

let GetImage(url:string) =
       let imageName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
       _images[imageName]

// added base path to get the directory of the markdown file so images in markdown can be loaded from the same directory as the markdown file
let SetBasePath(path:string) =
    _basePath <- path

// check if the url is a remote url (http or https)
let private isRemoteUrl (url:string) = 
    url.StartsWith("http://") || url.StartsWith("https://")

// resolve the local path using the base path + url
let private resolveLocalPath (url:string) =
    if url.StartsWith("file://") then Uri(url).LocalPath
    elif Path.IsPathRooted(url) then url
    elif _basePath <> "" then Path.Combine(_basePath, url)
    else url

let PreloadImage(url:string, maxWidth:int) =
       
       let imageName = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))
             
       if not(_images.ContainsKey(imageName)) then
           
           // check if the image's url is remote (needs to download) or local
           let fileName = 
                if isRemoteUrl url then downloadImage(url)
                else resolveLocalPath url
           
           use image = Image.Load(fileName)
           
           let resize = ResizeOptions(Mode = ResizeMode.Max, Size = Size(maxWidth, 0))

           // resize image if it larger than the browser window
           if image.Width > maxWidth then do           
               image.Mutate(fun x -> x.Resize(resize).BackgroundColor(Rgba32.ParseHex("#FFFFFF")) |> ignore)
           
           let encoder = PngEncoder(BitDepth = PngBitDepth.Bit16, ColorType = PngColorType.RgbWithAlpha)
           
           let imageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "MBC_Docs")
           
           Directory.CreateDirectory(imageDir) |> ignore
           
           let path = Path.Combine(imageDir, imageName + ".png")
           image.Save(path, encoder)
           _images.Add(imageName, Texture2D.FromFile(_graphics, path))
           


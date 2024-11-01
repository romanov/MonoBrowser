# MonoBrowser
Render markdown files inside your projects and games!
  
![image](https://github.com/user-attachments/assets/0393dcbb-82c1-4f7e-8893-3bfb2c0efacd)

## Documenation

Install via Nuget: `dotnet add package MonoBrowser --version 0.0.1-alpha`

1. Add component to your game or project

```
var browserWindow = new Rectangle(10, 10, _graphics.PreferredBackBufferWidth - 20, _graphics.PreferredBackBufferHeight - 20);

var browser = new BrowserComponent(this, browserWindow)
{
    EnableDebug = true,
    EnableScrollbar = true,
    AllowScroll = true,
    DisableImages = false
};

browser.OnLinkClicked += (_, url) =>
{
    // load new page or invoke method inside your game
    Console.WriteLine($"User clicked on: {url}");
};

Components.Add(browser);
```

2. Open remote markdown document with `browser.Navigate("https://yoursite/readme.md")`
3. Close window with `browser.Close()` 

## About

![image](https://github.com/user-attachments/assets/04c8e9f5-439d-40cc-be41-a128e697b129)

* MonoBrowser - a browser for markdown files (in development) based on MonoBrowser Core.
* MonoBrowser Core - a lightweight bare minimum browser component & markdown rendering engine written in F# for [MonoGame](https://monogame.net), and adaptable to desktop, mobile, and embedded C#/F# applications.

## Support
F#
C#

## Version
This is alpha version, some markdown tags are missing!

## Roadmap
1. Font dowloading
2. Styles
3. Smooth scrolling

## Licenses
[ImageSharp](https://github.com/SixLabors/ImageSharp) - allows convert images to PNG and load it to the game

[AngleSharp](https://github.com/AngleSharp/AngleSharp) - HTML parser

[Markdig](https://github.com/xoofx/markdig) - markdown parser

[FontStashSharp](https://github.com/FontStashSharp/FontStashSharp) - fonts and text rendering

[Apos Input](https://github.com/Apostolique/Apos.Input) - events

[Inter font](https://openfontlicense.org/) - default font

![logo_mono](https://github.com/user-attachments/assets/078bc0a3-a624-4b64-8be9-58ea13162d2b)


# MonoBrowser Core
MonoBrowser Core is a markdown rendering engine written in F# for [MonoGame](https://monogame.net), and adaptable to desktop, mobile, and embedded C#/F# applications.

![image](https://github.com/user-attachments/assets/7aad1f49-cf83-423c-8445-78c8d53f7001)


## Documenation

Install via Nuget: `dotnet add package MonoBrowser --version 0.0.1-alpha`

1. Add component to your game or project

```
var browserWindow = new Rectangle(10, 10, _graphics.PreferredBackBufferWidth - 20, _graphics.PreferredBackBufferHeight - 20);

var browser = new OleaComponent(this, browserWindow)
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

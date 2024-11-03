# MonoBrowser Core
Render local and remote markdown files inside your projects and games!
  
![app1](https://github.com/user-attachments/assets/73d60b78-f2e6-4e70-b791-807830391b67)


## Quick start

Install via [Nuget](https://www.nuget.org/packages/MonoBrowser/): `dotnet add package MonoBrowser`

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

## Documentation

You can load markdown content directly.
```
browser.FromString("""
                           # Game manual
                           
                           My game is **awesome** and here you can 
                           [read more](https://mygame.test) about it.
                           
                           ## How to play
                           
                           - Run my game
                           - Click on a play button
                           - Enjoy!
                           
                           """);
```

## About

![image](https://github.com/user-attachments/assets/0f5f9894-dede-4478-a267-9b6d796ba6dd)

* MonoBrowser - a browser for markdown files (in development) based on MonoBrowser Core.
* MonoBrowser Core - a lightweight bare minimum browser component & markdown rendering engine written in F# for [MonoGame](https://monogame.net), and adaptable to desktop, mobile, and embedded C#/F# applications.

## Support
Works in your C# or F# MonoGame projects.
Library tested on Windows 11, and Mac OS Sequoia 15.0


## Roadmap
1. Multiple windows.
2. Basic styles.
3. Smooth scrolling.

## Licenses
[ImageSharp](https://github.com/SixLabors/ImageSharp) - allows convert images to PNG and load it to the game

[AngleSharp](https://github.com/AngleSharp/AngleSharp) - HTML parser

[Markdig](https://github.com/xoofx/markdig) - markdown parser

[FontStashSharp](https://github.com/FontStashSharp/FontStashSharp) - fonts and text rendering

[Apos Input](https://github.com/Apostolique/Apos.Input) - events

[Inter font](https://openfontlicense.org/) - default font

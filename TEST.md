**MonoGame** is a [free](free_software "wikilink") and [open
source](Open-source_software "wikilink")
[C#](C_Sharp_(programming_language) "wikilink") framework used by game
developers to make games for multiple
[platforms](Computing_platform "wikilink") and other systems. It is also
used to make [Windows](Microsoft_Windows "wikilink") and [Windows
Phone](Windows_Phone "wikilink") games run on other systems. It supports
[iOS](iOS "wikilink"), [iPadOS](iPadOS "wikilink"),
[Android](Android_(operating_system) "wikilink"),
[macOS](macOS "wikilink"), [Linux](Linux "wikilink"), [PlayStation
4](PlayStation_4 "wikilink"), [PlayStation 5](PlayStation_5 "wikilink"),
[PlayStation Vita](PlayStation_Vita "wikilink"), [Xbox
One](Xbox_One "wikilink"), [Xbox Series X/S](Xbox_Series_X/S "wikilink")
and [Nintendo Switch](Nintendo_Switch "wikilink").[^1][^2] It implements
the [Microsoft XNA](Microsoft_XNA "wikilink") 4 [application programming
interface](application_programming_interface "wikilink") (API).[^3] It
has been used for several games, including
*[Bastion](Bastion_(video_game) "wikilink"),
[Celeste](Celeste_(video_game) "wikilink"),*
*[Fez](Fez_(video_game) "wikilink")* and *[Stardew
Valley](Stardew_Valley "wikilink").*

### test
* one
* two
* three

#### test 3
1. TEST 1
2. TEST 2
3. TEST 3

## History

MonoGame is a derivative of XNA Touch (September 2009) started by Jose
Antonio Farias[^4] and Silver Sprite by Bill
Reiss.`{{Citation needed|date=June 2013}}`{=mediawiki} The first
official release of MonoGame was version 2.0 with a downloadable version
0.7 that was available from [CodePlex](CodePlex "wikilink"). These early
versions only supported 2D
[sprite](Sprite_(computer_graphics) "wikilink")-based games. The last
official 2D-only version was released as 2.5.1 in June 2012.

Since mid-2013, the framework has begun to be extended beyond XNA4 with
the addition of new features like RenderTarget3D,[^5] support for
multiple GameWindows,[^6] and a new cross-platform command line content
building tool.[^7]

## Architecture

MonoGame attempts to fully implement the XNA 4 API.[^8] It accomplishes
this across Microsoft platforms using SharpDX and DirectX.[^9] When
targeting non-Microsoft platforms, platform specific capabilities are
utilized by way of the [OpenTK](OpenTK "wikilink") library. When
targeting OS X, iOS, and/or Android, the [Xamarin](Xamarin "wikilink")
platform runtime is necessary. This runtime provides a tuned OpenTK
implementation that allows the MonoGame team to focus on the core
graphics tuning of the platform.

The graphics capabilities of MonoGame come from either OpenGL, OpenGL
ES, or DirectX. Since MonoGame version 3, OpenGL 2 has been the focus
for capabilities. The earlier releases of MonoGame (2.5) used OpenGL 1.x
for graphics rendering. Utilizing OpenGL 2 allowed for MonoGame to
support shaders to make more advanced rendering capabilities in the
platform.

Content management and distribution continues to follow the XNA 4
ContentManager model. The MonoGame team has created a new content
building capability that can integrate with Microsoft Visual Studio to
deliver the same content building capabilities to Windows 8 Desktop that
Windows 7 users had used in Microsoft XNA.

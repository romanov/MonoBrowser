# MonoBrowser — Refactoring Analysis

A render engine for MonoGame that parses Markdown/HTML (via Markdig + AngleSharp) into a tree of `RenderElement` and draws it with FontStashSharp. The core pipeline is:

```
CreateElement → AddMarginNodes → AddTextNodes → AddSize → RefreshPosition → Draw
```

The design (immutable tree, successive rewrite passes) is sound. The problems are concentrated in global mutable state, massive record duplication, dead code, and a few real bugs. Items are ordered by impact.

---

## 1. Critical bugs

### 1.1 `SetupDefaultFonts` logic is inverted (`BrowserComponent.fs:141`)
```fsharp
if not (NetworkInterface.GetIsNetworkAvailable()) then
    raise (Exception("Please, create 'Content/Fonts' folder and put files..."))
...
if not (Directory.Exists(path)) then
    Directory.CreateDirectory(path) |> ignore   // creates EMPTY dir, downloads nothing
else
    // download fonts only if the folder ALREADY exists
```
Two inversions: the exception fires on *no network* but its message is about *missing font files*; and fonts are only downloaded when the folder already exists, so on a clean first run the folder is created empty and `LoadContent` then throws on `File.ReadAllBytes(".../regular.ttf")`. The download branch should run when files are missing, regardless of folder existence.

### 1.2 `Update` does not early-return when inactive (`BrowserComponent.fs:228`)
```fsharp
if not(isActive) then do ()   // no-op; execution continues
```
Input handling, scrolling, and mouse math all run even when the component is inactive. Should be `if not isActive then () else (...)` or a guard clause.

### 1.3 Ordered-list counter is a shared module global (`Builder.fs:57`)
`let mutable lastItem = 0` is mutated by `createChildNodesWithPrepend` and reset in the `OL` branch of `AddTextNodes`. Two `<ol>`s on a page, or two pages loading on the loader thread, corrupt each other's numbering. Same risk for `let mutable top` (used by `showTree`). List numbering state should be threaded through the recursion, not held in a module variable.

### 1.4 F5 refresh and page cache never invalidate (`BrowserComponent.fs:236`, `Book.fs:26`)
```fsharp
if refreshBtn.Pressed() && isDebugAllowed then ()   // does nothing
```
The `pages` cache is keyed by base64(url) and never evicted, so even a working refresh would return stale content.

---

## 2. Global mutable state

`Global.fs` holds the entire engine state as module-level mutables and shared collections (`Window`, `MaxRenderWidth`, `ContentHeight`, `WindowWidth/Height`, `Fonts`, `Page`, `DebugData`). Every pass reads and writes these directly.

Consequences: you cannot run two `BrowserComponent` instances (they share `Global.Page`), layout is not unit-testable without spinning up graphics, and `AddSize` mutates `Global.ContentHeight` as a side effect mid-traversal. `RefreshPosition` also mutates `element.Outline` in place while every other pass is purely functional — inconsistent and surprising.

Recommendation: introduce a `RenderContext` record (window, padding, max width, fonts, debug flag) passed explicitly into the pipeline, and have each pass *return* its result (content height, positioned tree) rather than writing globals. This is the single highest-leverage change for testability and multi-instance support.

---

## 3. Massive duplication in `CreateElement` (`Builder.fs:185`)

The tag `match` repeats a near-identical `RenderElement` record literal ~12 times, differing only in `Payload`, `Margin`, and `Padding`. It ends with `{ element with Children = children }`, which re-assigns the same `children` already set in every branch — redundant.

Extract a smart constructor:
```fsharp
let private mkElement tag payload display padding margin children =
    { Tag = tag; Outline = Rectangle(0,0,Global.MaxRenderWidth,0)
      Children = children; Payload = payload; Display = display
      Padding = padding; Margin = margin; IsClickable = false }
```
Then each branch is one line. This also removes the scattered `RandomHelp.CreateString(5)` tag suffixes — random strings are being used as element identity, which is fragile: `PrevBlockPosition` finds an element via structural equality (`Array.findIndex (fun x -> x = element)`), an O(n) comparison that breaks on duplicate records. Give elements a real `Id` (incrementing int / GUID) instead of random tags.

The same duplication pattern recurs in the synthetic-node builders (`AddMarginNodes`, `AddPadding`, `WrapBlock`, `CreateLine`) — all hand-roll the full record. Route them through `mkElement` too.

---

## 4. Dead code (safe to delete)

Confirmed unreferenced across the codebase:

- **`HtmlData.fs`** — the entire module (`PositionMode`, a second `DisplayMode`, `ColorString`, `Style`) is never used. Note it defines a *second* `DisplayMode` that collides conceptually with the active one in `BasicData` (`Anon | Inline | Block`). Delete the file and its `<Compile>` entry.
- **`Basic.fs`** — `Settings`, `Styles`, `SimpleNode` types are unused.
- **`Builder.fs`** — `imagesList` (only referenced in commented code), `showTree`/`top` (only called recursively + a commented call site), the commented `IMG` branch.
- **`RenderElementMethods.fs`** — `LastAnon1` is never called; large trailing blank region.
- **`RandomHelp.fs`** — `CreateNewsFileName`, `CreateNumber` unused.
- **`Camera.fs`** — `DeprojectScreenPosition`, `ProjectScreenPosition`, `ZoomCamera` unused; commented `ViewMatrix`.
- Stray `printfn` debug calls in hot paths: `AddSize` prints page height on every layout (`Builder.fs:462`); `DrawElement` prints on every non-link click.

---

## 5. IO and concurrency

### 5.1 `HttpClient` created per-call with `use`
`Book.AddPage`, `ImageLoader.downloadImage`, and `SetupDefaultFonts` each `use client = new HttpClient()`. This is the classic socket-exhaustion antipattern. Use a single shared static `HttpClient` (or `IHttpClientFactory`).

### 5.2 Sync-over-async everywhere
`.Result` / `.Wait()` on `GetStringAsync`, `GetByteArrayAsync`, `WriteAllBytesAsync`, `CopyToAsync`. Risks thread-pool starvation/deadlock. Since loading already runs on a dedicated `Thread`, either make the path genuinely async or keep blocking but at minimum stop blocking on the UI-adjacent paths.

### 5.3 Network IO during layout
`createChildNodesWithPrepend` (`Builder.fs:81`) calls `ImageLoader.PreloadImage` — which downloads and re-encodes a PNG to disk — *inside the tree-building pass*. Layout should operate on already-resolved image dimensions; move image fetching to a distinct resolve phase before layout.

---

## 6. Naming, config, consistency

- **Typos in identifiers**: `proccess`, `chunck`, `ProccessTextBlock`, `newchilds`. Rename.
- **Inconsistent casing**: public `CreateElement`/`CreateTextBlock` vs `createChildNodes`/`createLine`. Pick one convention for module-level functions.
- **Magic numbers**: paddings (30/25), margins (20/10), scroll step (40), font sizes (20/38/26), `WindowPadding (10,20)`, scrollbar widths are inlined throughout `Builder.fs` and `BrowserComponent.fs`. The unused `Settings` type in `Basic.fs` is the natural home — wire up a single theme/config record and read from it.
- **Misplaced helper**: `ColorHelper.CalculatePercentage` has nothing to do with color; move to a layout/math helper.
- **Non-exhaustive position logic**: `RefreshPosition` (`Builder.fs:472`) silently sends `Inline,None` / `Anon,None` to `Point.Zero`; make the intent explicit.

---

## Suggested order of work

1. Fix the four bugs in §1 (small, high value, low risk).
2. Delete dead code in §4 (shrinks surface area before deeper changes).
3. Extract `mkElement` and give elements real IDs (§3).
4. Share `HttpClient`, separate image-resolve from layout (§5).
5. Introduce `RenderContext` to retire `Global` mutables (§2) — largest change, do last.
6. Sweep naming/config/magic numbers (§6).

Steps 1–4 are mostly mechanical and independently shippable. Step 5 is the architectural one and benefits from the cleanup before it.

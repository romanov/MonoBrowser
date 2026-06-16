# MonoBrowser — Refactoring Plan

A render engine for MonoGame that parses Markdown/HTML (Markdig + AngleSharp) into a tree of `RenderElement` and draws it with FontStashSharp. Pipeline:

```
CreateElement → AddMarginNodes → AddTextNodes → AddSize → RefreshPosition → Draw
```

The immutable-tree / successive-rewrite-pass design is sound. Since the previous analysis, the four critical bugs (font setup, inactive `Update`, OL counter, F5/cache) are **fixed**, `mkElement` + stable integer `Id` are **implemented**, and a live theme/font-size config lives in `Global.fs`. What remains is dead code, IO/concurrency hygiene, the global-state architecture, and naming. Ordered by impact-to-risk.

---

## 1. Dead code (delete — low risk, shrinks surface before deeper work)

All confirmed unreferenced (only self-references or commented call sites):

| Item | Location |
|---|---|
| Entire `HtmlData` module (`PositionMode`, `ColorString`, second `DisplayMode`, `Style`, `Word`) | `HtmlData.fs` + `<Compile>` line 34 of the fsproj |
| `Settings`, `Styles`, `SimpleNode` types | `Basic.fs:19,71,86` |
| `showTree` + `top` | `Builder.fs:27,31-55` |
| `imagesList` + commented `IMG` branch | `Builder.fs:29,208-225` |
| `LastAnon1`, `WrapBlock`, `CreateLine`, `LocationWithOffset` | `RenderElementMethods.fs:50,53,66,84` |
| `ZoomCamera`, `DeprojectScreenPosition`, `ProjectScreenPosition` | `Camera.fs:66,69,72` |
| `CalculatePercentage` (also misplaced — not color logic) | `Helpers/ColorHelper.fs:7` |
| Stray `printfn` on every layout (`max page height is …`) | `Builder.fs:397` |
| Stray `printfn` on every non-link click | `BrowserComponent.fs:425` |

Note: `Basic.fs` `Settings` was previously floated as the home for a theme config, but that config already exists as mutables in `Global.fs` — so `Settings` is genuinely dead, not "to be wired up." Delete it.

---

## 2. IO & concurrency

### 2.1 `HttpClient` created per call (socket exhaustion)
`use client = new HttpClient()` appears in three places: `Book.fs:61`, `BrowserComponent.SetupDefaultFonts` (`BrowserComponent.fs:241`), `ImageLoader.downloadImage` (`ImageLoader.fs:21`). Replace with a single shared `static` `HttpClient` (or `IHttpClientFactory`).

### 2.2 Sync-over-async everywhere
`.Result` / `.Wait()` on `GetStringAsync`, `GetStreamAsync`, `GetByteArrayAsync`, `WriteAllBytesAsync`, `CopyToAsync`, `ReadAllTextAsync` (Book, ImageLoader, BrowserComponent). Page loading already runs on a dedicated `Thread`, so the blocking is contained — but make the chain genuinely `async`/`task` or document the deliberate block. Don't leave it ambiguous.

### 2.3 Network IO inside the layout pass
`createChildNodesWithPrepend` (`Builder.fs:89`) calls `ImageLoader.PreloadImage`, which downloads and re-encodes a PNG to disk — *during tree building*. Layout should run on already-resolved image dimensions. Split a `resolveImages` phase before `AddTextNodes`/`AddSize`, and have layout read cached sizes only.

---

## 3. Global mutable state (largest architectural item — do after 1–2)

`Global.fs` holds the whole engine state as module-level mutables and shared collections: `Window`, `MaxRenderWidth`, `ContentHeight`, `WindowWidth/Height`, `WindowPadding`, theme colors, font sizes, `Fonts`, `Page` (`ConcurrentBag`), `DebugData`.

Consequences:
- **No two `BrowserComponent` instances** — they share `Global.Page`/`Global.ContentHeight`. (The README roadmap explicitly wants "multiple windows.")
- **`AddSize` writes `Global.ContentHeight`** as a side effect mid-traversal (`Builder.fs:396`) — every other pass is pure.
- **`RefreshPosition` mutates `element.Outline` in place** (`Builder.fs:433`; `Outline` is the one `mutable` field on `RenderElement`) while the rest of the pipeline returns new trees. Inconsistent and surprising.
- **Not unit-testable** without a live `GraphicsDevice`.

Recommendation: introduce a `RenderContext` record (window, padding, max width, fonts, theme, debug flag) threaded explicitly into each pass; have passes **return** results (content height, positioned tree) instead of writing globals. Make `Outline` non-mutable and have `RefreshPosition` return a new tree. This unblocks multi-window and testing. Largest change — sequence it last, after the cleanup makes the surface smaller.

---

## 4. Identity & payload cleanup

Elements now carry a stable `Id` (`RenderElementMethods.mkElement`), and `PrevBlockPosition` correctly compares by `Id` (`RenderElementMethods.fs:72`). Two leftovers:

- `UL(RandomHelp.CreateString(10))` (`Builder.fs:271`) — the guid payload is never read now that `Id` exists. Change `UL of guid:string` → a fieldless `UL` and drop the random string. Then audit whether `RandomHelp.CreateString` (and the `Nanoid` package ref) is still needed anywhere.
- `DrawScrollbar` (`BrowserComponent.fs:409-411`) hard-codes `40` (the scroll step) and `10` (the right-edge inset) as literals instead of reading `scrollStep` / a named inset constant. Wire to the existing fields so scroll math stays consistent.

---

## 5. Naming & consistency

- **Typos in identifiers**: `proccess`, `chunck` (`Builder.fs:161-173`), `newchilds` (`Builder.fs:504-508`), `ProccessTextBlock` (`TextCreator.fs:134,178,185`). Rename.
- **Inconsistent module-function casing**: public `CreateElement` / `CreateTextBlock` vs `createChildNodes` / `createChildNodesWithPrepend`. Pick one convention.
- **Non-exhaustive position logic**: `RefreshPosition` (`Builder.fs:427`) routes `Inline,None` / `Anon,None` to `Point.Zero` via a catch-all — make the intent explicit or assert it can't happen.
- **Remaining magic numbers**: block paddings `30/25` (`Builder.fs:254,263`), header margins `20/10`, paragraph `5`. Lower priority than the theme work already done, but fold into the `RenderContext` theme in §3 when you get there.

---

## Suggested order of work

1. **Delete dead code (§1)** — mechanical, independently shippable, shrinks everything else.
2. **Share `HttpClient` + separate image resolve from layout (§2)** — correctness/perf, contained changes.
3. **Identity/payload + scrollbar constants (§4)** — small, removes the last `RandomHelp` dependency.
4. **Naming sweep (§5)** — cheap once the dead code is gone.
5. **`RenderContext` to retire `Global` mutables + make `Outline` immutable (§3)** — the architectural change; benefits from all the above landing first, and unblocks the multi-window roadmap item.

Steps 1–4 are low-risk and shippable in any order. Step 5 is the one worth a design pass.

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

module Camera

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type Camera2D(viewport: Viewport, graph:GraphicsDevice) =

    [<DefaultValue>] val mutable Position:Vector2

    /// vertical position the camera eases toward (set by scrolling)
    [<DefaultValue>] val mutable ScrollTarget:float32

    member val Rotation:float32 = 0f
    member val Zoom:float32 = 1f with get,set
    member val Origin:Vector2 = Vector2((float32)viewport.Width / 2f, (float32)viewport.Height / 2f)
    member val Graphics = graph with get

    //member this.ViewMatrix =
    //    Matrix.CreateOrthographicOffCenter(0f, (float32)this.Graphics.PresentationParameters.BackBufferWidth, (float32)this.Graphics.PresentationParameters.BackBufferHeight, 0f, 0f, 1f)
    //        * Matrix.CreateTranslation(Vector3(-this.Position, 0.0f))

    member this.ViewMatrix =
        Matrix.CreateTranslation(Vector3(-this.Position, 0.0f)) *
        Matrix.CreateTranslation(new Vector3(-this.Origin, 0.0f)) *
        Matrix.CreateRotationZ(this.Rotation) *
        Matrix.CreateScale(this.Zoom, this.Zoom, 1f) *
        Matrix.CreateTranslation(new Vector3(this.Origin, 0.0f))

    member this.MoveCamera(pos:Vector2) =
        let newPosition = this.Position - Vector2(pos.X, pos.Y)
        if newPosition.Y >= 0f then
            this.Position <- newPosition

    /// Nudge the scroll target by `delta`, clamped to [0, maxScroll].
    member this.ScrollBy(delta:float32, maxScroll:float32) =
        this.ScrollTarget <- MathHelper.Clamp(this.ScrollTarget + delta, 0f, maxScroll)

    /// Jump straight to a vertical position without easing (e.g. on page load).
    member this.SnapScroll(y:float32) =
        this.ScrollTarget <- y
        this.Position <- Vector2(this.Position.X, y)

    /// Ease the camera toward ScrollTarget. `dt` is the frame time in seconds,
    /// so the motion is the same speed regardless of frame rate. `stiffness`
    /// controls how quickly it catches up (higher = snappier).
    member this.UpdateScroll(dt:float32, ?stiffness:float32) =
        let k = defaultArg stiffness 18f
        let t = 1f - MathF.Exp(-k * dt)
        let newY = MathHelper.Lerp(this.Position.Y, this.ScrollTarget, t)
        // settle exactly once we are within a sub-pixel of the target
        this.Position <- Vector2(this.Position.X, (if MathF.Abs(this.ScrollTarget - newY) < 0.5f then this.ScrollTarget else newY))

    member this.ZoomCamera(lvl:float32) =
        this.Zoom <- lvl

    member this.DeprojectScreenPosition(position:Point) = // for MouseState.Position
       Vector2.Transform(new Vector2((float32)position.X, (float32)position.Y), Matrix.Invert(this.ViewMatrix));

    member this.ProjectScreenPosition(position:Point) = // for MouseState.Position
       Vector2.Transform(new Vector2((float32)position.X, (float32)position.Y), this.ViewMatrix);
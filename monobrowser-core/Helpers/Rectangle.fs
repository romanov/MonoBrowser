module BasicRectangle

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type BorderBackground() =
    
      let mutable _texture = Unchecked.defaultof<Texture2D>
      
      member x.LoadContent(graphics:GraphicsDevice) =
            _texture <- new Texture2D(graphics, 1, 1)
            _texture.SetData([| Color.White |])
        
      member x.Draw(spriteBatch:SpriteBatch, rectangle:Rectangle, color:Color) =
            spriteBatch.Draw(_texture, rectangle, color)
        
        

type BorderRectangle() =

    let mutable _pointTexture = Unchecked.defaultof<Texture2D>
    
    member x.LoadContent(graphics:GraphicsDevice) =
        _pointTexture <- new Texture2D(graphics, 1, 1)
        _pointTexture.SetData([| Color.White |])
    
    member x.Draw(spriteBatch:SpriteBatch, rectangle:Rectangle, color:Color, lineWidth:int) =
        spriteBatch.Draw(_pointTexture, Rectangle(rectangle.X, rectangle.Y, lineWidth, rectangle.Height + lineWidth), color)
        spriteBatch.Draw(_pointTexture, Rectangle(rectangle.X, rectangle.Y, rectangle.Width + lineWidth, lineWidth), color)
        spriteBatch.Draw(_pointTexture, Rectangle(rectangle.X + rectangle.Width, rectangle.Y, lineWidth, rectangle.Height + lineWidth), color)
        spriteBatch.Draw(_pointTexture, Rectangle(rectangle.X, rectangle.Y + rectangle.Height, rectangle.Width + lineWidth, lineWidth), color)
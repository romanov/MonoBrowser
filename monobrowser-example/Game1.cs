using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Romanov.MonoBrowserCore;

namespace MonoBrowserExample;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    public Game1()
    {
        Window.Title = "MonoBrowser Example";
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        var browserWindow = new Rectangle(10, 10, _graphics.PreferredBackBufferWidth - 20,
            _graphics.PreferredBackBufferHeight - 20);

        var browser = new BrowserComponent(this, browserWindow)
        {
            EnableScrollbar = true,
            AllowScroll = true,
            DisableImages = true,
            EnableDebug = true,
        };

        browser.OnLinkClicked += (_, url) =>
        {
            // load new page or invoke method inside your game
            Console.WriteLine($"User clicked on: {url}");
        };
        
        Components.Add(browser);

        // load remote document
        browser.Navigate("https://raw.githubusercontent.com/romanov/monobrowser/refs/heads/main/README.md");
        
        // load local document
        // browser.LoadFile("path to your file");
        
        // load local document from Content folder of your app
        // browser.Navigate("content://TEST.md");
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}
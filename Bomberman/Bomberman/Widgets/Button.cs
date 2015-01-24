using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// A game button.
    /// </summary>
    public class Button : DrawableGameComponent
    {
        public string Text { get; set; }
        public bool Locked { get; set; }
        public Rectangle Bounds { get; set; }
        public Color BackgroundColor { set; get; }
        public Color TextColor { set; get; }

        private bool pressed;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Texture2D pressedTexture;
        private Texture2D releasedTexture;

        
        public Button(Game game) : base(game)
        {
            pressed = false;
            Text = "My Button";
            Bounds = new Rectangle(0, 0, 128, 16);
            TextColor = Color.Black;
            BackgroundColor = Color.White;

            DrawOrder = Utilities.Order.OverlayLevel;

            game.Components.Add(this);
        }
        
        public override void Update(GameTime gameTime)
        {
            if (Locked) return;

            Services.IUserInput userInput = (Services.IUserInput)Game.Services.GetService(typeof(Services.IUserInput));
            Vector2 position = userInput.MousePosition;
            bool inBoundaries = Bounds.Contains((int)position.X, (int)position.Y);

            if (userInput.MousePressed)
            {
                if (inBoundaries)
                {
                    pressed = true;
                }
            }
            else if (pressed)
            {
                if (userInput.MouseDown)
                {
                    if (!inBoundaries)
                    {
                        pressed = false;
                    }
                }
                else
                {
                    if (Pressed != null) Pressed(this, new EventArgs());
                    pressed = false;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            float alpha = Locked ? 0.5f : 1.0f;
            Point center = Bounds.Center;
            spriteBatch.Begin();
            spriteBatch.Draw(pressed ? pressedTexture : releasedTexture, Bounds, BackgroundColor * alpha);
            spriteBatch.DrawString(spriteFont, Text, new Vector2(center.X, center.Y), TextColor * alpha, 0.0f, spriteFont.MeasureString(Text) / 2, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("button");
            pressedTexture = Game.Content.Load<Texture2D>("gfx\\buttonPressed");
            releasedTexture = Game.Content.Load<Texture2D>("gfx\\buttonReleased");
        }

        // Events

        /// <summary>
        /// Fired when the user presses the button.
        /// </summary>
        public event EventHandler Pressed;
    }
}
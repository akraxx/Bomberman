using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;

namespace Bomberman.Widgets
{
    class Textbox : DrawableGameComponent
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public int MaxSize { get; set; }
        public bool Locked { get; set; }
        public Rectangle Bounds { get; set; }
        public Vector2 TitleOffset { get; set; }

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Texture2D texture;

        public Textbox(Game game) : base(game)
        {
            Title = "Textbox";
            Description = "Enter some text";
            Text = "My Text";
            MaxSize = 15;
            Bounds = new Rectangle(0, 0, 128, 16);
            TitleOffset = new Vector2(0, -16);

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
                if (Bounds.Contains((int)position.X, (int)position.Y))
                {
                    Guide.BeginShowKeyboardInput(PlayerIndex.One, Title, Description + " (" + MaxSize + " characters limit)", Text, OnInputCompleted, null);
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Point center = Bounds.Center;
            Vector2 titlePosition = new Vector2(center.X, center.Y) + TitleOffset;
            spriteBatch.Begin();
            spriteBatch.Draw(texture, Bounds, Color.White);
            spriteBatch.DrawString(spriteFont, Text, new Vector2(center.X, center.Y), Color.White, 0.0f, spriteFont.MeasureString(Text) / 2, 1.0f, SpriteEffects.None, 0.0f);
            Utilities.Drawing.DrawShadowedText(spriteFont, spriteBatch, Title, titlePosition, spriteFont.MeasureString(Title) / 2, Color.White);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("button");
            texture = Game.Content.Load<Texture2D>("gfx\\textbox");
        }

        protected void OnInputCompleted(IAsyncResult r)
        {
            string result = Guide.EndShowKeyboardInput(r);
            Text = (result != null ? result.Substring(0, Math.Min(MaxSize, result.Length)) : Text);
            Game.ResetElapsedTime();
        }
    }
}
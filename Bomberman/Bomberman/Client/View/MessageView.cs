using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Bomberman.Utilities;

namespace Bomberman.Client.View
{
    class MessageView : DrawableGameComponent
    {
        private static readonly Vector2 bubblePosition = new Vector2(207, 55);

        protected SpriteBatch spriteBatch;
        protected SpriteFont spriteFont;
        private Texture2D texture;

        public string[] Lines { get; set; }

        /// <summary>
        /// The origin of this view.
        /// </summary>
        public Vector2 Origin { get; set; }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("arialBold9");

            texture = Game.Content.Load<Texture2D>("gfx\\messageView");
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(texture, Origin, Color.White);
            float fontSize = spriteFont.MeasureString("N").Y;
            float offset = fontSize / 2 * (Lines.Length - 1);
            int loop = 0;
            for (float i = -offset; i <= (offset); i += fontSize)
            {
                Drawing.DrawCenteredText(spriteFont, spriteBatch, Lines[loop], Origin + bubblePosition + new Vector2(0, i), Color.Black, false);
                loop++;
            }

            spriteBatch.End();
        }

        public MessageView(Microsoft.Xna.Framework.Game game, Vector2 origin, params string[] lines) : base(game)
        {
            Origin = origin;
            Lines = lines;

            DrawOrder = Order.OverlayLevel;

            game.Components.Add(this);
        }
    }
}

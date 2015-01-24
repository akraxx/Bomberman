using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// An infinite scrolling background.
    /// </summary>
    class ScrollingBackground : DrawableGameComponent
    {
        private Vector2 offset;
        private SpriteBatch spriteBatch;
        private Texture2D background;

        /// <summary>
        /// Number of scrolled pixels per second.
        /// </summary>
        public Vector2 Rate { get; set; }

        /// <summary>
        /// Color to apply on the background texture.
        /// </summary>
        public Color Color { get; set; }

        public ScrollingBackground(Game game) : base(game)
        {
            Rate = new Vector2(30.0f);
            Color = Color.White;

            DrawOrder = Utilities.Order.BackgroundLevel;

            game.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            // Nothing to do
        }

        public override void Draw(GameTime gameTime)
        {
            offset += Rate * (float)(gameTime.ElapsedGameTime.TotalSeconds);

            Viewport v = GraphicsDevice.Viewport;
            Rectangle srcRect = new Rectangle((int)offset.X, (int)offset.Y, v.Width, v.Height);
            Rectangle dstRect = v.Bounds;

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.Default, RasterizerState.CullNone);
            spriteBatch.Draw(background, dstRect, srcRect, Color, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            background = Game.Content.Load<Texture2D>("gfx\\background");
        }
    }
}

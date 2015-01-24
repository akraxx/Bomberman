using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// Show a message that does a fade effect to appear/disappear.
    /// </summary>
    class FadingMessage : DrawableGameComponent
    {
        private static readonly TimeSpan fadeTimer = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan holdTimer = TimeSpan.FromSeconds(1.0);
        private static readonly TimeSpan totalTimer = fadeTimer + holdTimer + fadeTimer;
        private static readonly float scrollOffset = 100.0f;

        private TimeSpan elapsed;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        public string Text { get; set; }
        public Vector2 Offset { get; set; }

        public float Progression
        {
            get { return MathHelper.Min(1.0f, (float)(elapsed.TotalMilliseconds / totalTimer.TotalMilliseconds)); }
        }

        public FadingMessage(Game game) : base(game)
        {
            Text = "MY MESSAGE";
            Offset = Vector2.Zero;
            Visible = false;
            VisibleChanged += CheckVisible;
            DrawOrder = Utilities.Order.PopupLevel;
            game.Components.Add(this);
        }

        public override void Draw(GameTime gameTime)
        {
            Viewport viewport = Game.GraphicsDevice.Viewport;
            Vector2 center = new Vector2(viewport.Width, viewport.Height) / 2 + Offset;

            TimeSpan holdStart = fadeTimer;
            TimeSpan holdEnd = holdStart + holdTimer;
            TimeSpan finish = holdEnd + fadeTimer;

            float offsetFactor = 0.0f;
            float alpha = 1.0f;

            if (elapsed < holdStart)
            {
                float p = Utilities.Smoothing.IncreasingExponential((float)(elapsed.TotalMilliseconds / holdStart.TotalMilliseconds));
                offsetFactor = MathHelper.Lerp(1.0f, 0.0f, p);
                alpha = p;
            }
            else if (elapsed > holdEnd)
            {
                float p = Utilities.Smoothing.DecreasingExponential((float)((elapsed - holdEnd).TotalMilliseconds / (finish - holdEnd).TotalMilliseconds));
                offsetFactor = MathHelper.Lerp(0.0f, -1.0f, p);
                alpha = 1.0f - p;
            }

            spriteBatch.Begin();
            Utilities.Drawing.DrawShadowedText(spriteFont, spriteBatch, Text, center + new Vector2(scrollOffset * offsetFactor, 0), spriteFont.MeasureString(Text) / 2, Color.White * alpha);
            spriteBatch.End();

            elapsed += gameTime.ElapsedGameTime;

            if (elapsed > finish)
                this.Visible = false;
        }

        private void CheckVisible(Object sender, EventArgs e)
        {
            if (Visible)
            {
                elapsed = TimeSpan.FromSeconds(0);
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("smallMessage");
        }
    }
}

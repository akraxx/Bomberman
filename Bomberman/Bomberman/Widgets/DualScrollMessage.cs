using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// Show two messages that collide horizontally.
    /// </summary>
    class DualScrollMessage : DrawableGameComponent
    {
        private static readonly TimeSpan scrollInTimer = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan holdTimer = TimeSpan.FromSeconds(0.5);
        private static readonly TimeSpan scrollOutTimer = TimeSpan.FromSeconds(1.0);

        private TimeSpan elapsed;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        public string LeftText { get; set; }
        public string RightText { get; set; }
        public int Spacing { get; set; }
        public Vector2 Offset { get; set; }

        public DualScrollMessage(Game game) : base(game)
        {
            LeftText = "LEFT";
            RightText = "RIGHT";
            Spacing = 0;
            Offset = Vector2.Zero;
            Visible = false;
            VisibleChanged += CheckVisible;
            DrawOrder = Utilities.Order.PopupLevel;
            game.Components.Add(this);
        }

        public override void Draw(GameTime gameTime)
        {
            float progression = this.Progress(gameTime.ElapsedGameTime);
            Viewport viewport = Game.GraphicsDevice.Viewport;
            Vector2 position = new Vector2(viewport.Width, viewport.Height) / 2 + Offset;

            Vector2 leftSize = spriteFont.MeasureString(LeftText);
            Vector2 rightSize = spriteFont.MeasureString(RightText);
            Vector2 size = leftSize + rightSize + new Vector2(Spacing, 0);

            Vector2 leftStart = new Vector2(viewport.Width + leftSize.X / 2, viewport.Height / 2 + Offset.Y);
            Vector2 leftFinish = position + new Vector2(leftSize.X / 2 - size.X / 2, 0);
            Vector2 leftCurrent = Utilities.Smoothing.InterpolateVector(leftStart, leftFinish, progression);
            Vector2 rightStart = new Vector2(-rightSize.X / 2, viewport.Height / 2 + Offset.Y);
            Vector2 rightFinish = position - new Vector2(rightSize.X / 2 - size.X / 2, 0);
            Vector2 rightCurrent = Utilities.Smoothing.InterpolateVector(rightStart, rightFinish, progression);

            spriteBatch.Begin();
            Utilities.Drawing.DrawShadowedText(spriteFont, spriteBatch, LeftText, leftCurrent, leftSize / 2);
            Utilities.Drawing.DrawShadowedText(spriteFont, spriteBatch, RightText, rightCurrent, rightSize / 2);
            spriteBatch.End();
        }

        private void CheckVisible(Object sender, EventArgs e)
        {
            if (Visible)
            {
                elapsed = TimeSpan.FromSeconds(0);

                Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Game.Services.GetService(typeof(Services.ISoundPlayer));
                soundPlayer.Play("woosh");
            }
        }

        private float Progress(TimeSpan increment)
        {
            TimeSpan holdStart = scrollInTimer;
            TimeSpan holdEnd = holdStart + holdTimer;
            TimeSpan finish = holdEnd + scrollOutTimer;

            float progression = 1.0f;

            if (elapsed < holdStart)
            {
                progression = Utilities.Smoothing.IncreasingExponential((float)(elapsed.TotalMilliseconds / holdStart.TotalMilliseconds));
            }
            else if (elapsed > holdEnd)
            {
                progression = 1.0f + Utilities.Smoothing.DecreasingExponential((float)((elapsed - holdEnd).TotalMilliseconds / (finish - holdEnd).TotalMilliseconds));
            }

            elapsed += increment;

            if (elapsed > finish)
                this.Visible = false;

            return progression;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("message");
        }
    }
}

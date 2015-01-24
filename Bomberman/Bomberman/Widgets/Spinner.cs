using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// A spinner graphic component.
    /// </summary>
    class Spinner : DrawableGameComponent
    {
        public enum Modes
        {
            None,
            Spin,
            Uniform,
        }

        private static TimeSpan fullTimer = TimeSpan.FromSeconds(1.0);
        private TimeSpan timer;

        private SpriteBatch spriteBatch;
        private Texture2D texture;

        /// <summary>
        /// Where the spinner should be placed.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        /// The radius of the spinner.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// The number of dots of the spinner.
        /// </summary>
        public int Dots { get; set; }

        /// <summary>
        /// The color of the spinner.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The mode of the spinner.
        /// </summary>
        public Modes Mode { get; set; }

        public Spinner(Game game) : base(game)
        {
            Radius = 12.0f;
            Dots = 10;
            Color = Color.White;
            Mode = Modes.Spin;

            game.Components.Add(this);
        }

        public override void Draw(GameTime gameTime)
        {
            timer += gameTime.ElapsedGameTime;

            float progression = (float)((timer.TotalSeconds / fullTimer.TotalSeconds) % 1.0);

            spriteBatch.Begin();
            for (int i = 0; i < Dots; i++)
            {
                float angle = MathHelper.TwoPi / Dots * i - MathHelper.PiOver2;
                float alpha = 1.0f;
                if (Mode == Modes.Spin) alpha = MathHelper.Max(0.0f, 1.0f - Model.Orientation.GetAngleDistance(angle, progression * MathHelper.TwoPi) / MathHelper.PiOver2);
                if (Mode == Modes.Uniform) alpha = 0.5f + 0.5f * (float)Math.Cos(progression * MathHelper.TwoPi);
                Vector2 dotPosition = Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Radius;
                spriteBatch.Draw(texture, dotPosition, null, Color * alpha, 0.0f, new Vector2(texture.Width, texture.Height) / 2, Vector2.One, SpriteEffects.None, 0.0f);
            }
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            texture = Game.Content.Load<Texture2D>("gfx\\dot");
        }
    }
}
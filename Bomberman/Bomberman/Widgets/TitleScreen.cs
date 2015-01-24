using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// The title screen of the game.
    /// </summary>
    class TitleScreen : DrawableGameComponent
    {
        public enum State
        {
            Disabled, Showing, Waiting, Hiding
        }

        private TimeSpan timer;
        private static readonly TimeSpan fadeTimer = TimeSpan.FromSeconds(1.0);
        private static readonly TimeSpan pulseTimer = TimeSpan.FromSeconds(1.0);

        public State Status { get; protected set; }
        public string Text { get; set; }

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Texture2D background;

        public TitleScreen(Game game) : base(game)
        {
            Text = "Touch to start";

            this.Hide(true);

            DrawOrder = Utilities.Order.SpriteLevel;

            game.Components.Add(this);
        }

        public void Show(bool immediate)
        {
            if (immediate)
            {
                Status = State.Waiting;
                timer = TimeSpan.Zero;
            }
            else if (Status != State.Showing)
            {
                Status = State.Showing;
                timer = TimeSpan.Zero;
            }
        }

        public void Hide(bool immediate)
        {
            if (immediate)
            {
                Status = State.Disabled;
                timer = TimeSpan.Zero;
            }
            else if (Status != State.Hiding)
            {
                Status = State.Hiding;
                timer = TimeSpan.Zero;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Status == State.Waiting)
            {
                Services.IUserInput userInput = (Services.IUserInput)Game.Services.GetService(typeof(Services.IUserInput));
                if (userInput.MousePressed)
                {
                    if (Validated != null) Validated(this, new EventArgs());
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (Status != State.Disabled)
            {
                timer += gameTime.ElapsedGameTime;

                // Update animation state
                float alpha = 1.0f;
                float textAlpha = 0.0f;
                if (Status == State.Showing)
                {
                    alpha = (float)Math.Min(1.0, timer.TotalSeconds / fadeTimer.TotalSeconds);
                    if (timer > fadeTimer)
                    {
                        this.Show(true);
                    }
                }
                else if (Status == State.Hiding)
                {
                    alpha = 1.0f - (float)Math.Min(1.0, timer.TotalSeconds / fadeTimer.TotalSeconds);
                    if (timer > fadeTimer)
                    {
                        this.Hide(true);
                    }
                }
                else if (Status == State.Waiting)
                {
                    if (Enabled)
                    {
                        textAlpha = (float)(0.5 - 0.5 * Math.Cos(timer.TotalSeconds / pulseTimer.TotalSeconds * MathHelper.TwoPi));
                    }
                }

                Viewport v = GraphicsDevice.Viewport;
                Rectangle rect = new Rectangle(0, 0, v.Width, v.Height);
                Vector2 textPosition = new Vector2(v.Width / 2, v.Height * 0.8f);
                Vector2 textSize = spriteFont.MeasureString(Text);

                spriteBatch.Begin();
                spriteBatch.Draw(background, rect, Color.White * alpha);
                Utilities.Drawing.DrawShadowedText(spriteFont, spriteBatch, Text, textPosition, textSize / 2, Color.White * textAlpha);
                spriteBatch.End();
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("default");
            background = Game.Content.Load<Texture2D>("gfx\\titleScreen");
        }

        // Events

        /// <summary>
        /// Fired when the user validates the title screen (touch the screen).
        /// </summary>
        public event EventHandler Validated;
    }
}
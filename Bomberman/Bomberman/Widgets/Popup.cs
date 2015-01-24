using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// A popup shown when an operation is being performed or has ended correctly or with an error.
    /// </summary>
    class Popup : DrawableGameComponent
    {
        public enum Types
        {
            Message, Error, Operation
        }

        public Types Type { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }

        private Spinner spinner;
        private Button button;

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Texture2D background;

        public Popup(Game game) : base(game)
        {
            Type = Types.Operation;
            Text1 = "Description here 1";
            Text2 = "Description here 2";

            spinner = new Spinner(game) { Center = new Vector2(128, 104) };
            button = new Button(game) { Bounds = new Rectangle(136, 136, 128, 16) };
            button.Pressed += new EventHandler(Button_Pressed);

            EnabledChanged += new EventHandler<EventArgs>(Popup_EnabledChanged);
            VisibleChanged += new EventHandler<EventArgs>(Popup_VisibleChanged);
            DrawOrderChanged += new EventHandler<EventArgs>(Popup_DrawOrderChanged);

            DrawOrder = Utilities.Order.PopupLevel;

            game.Components.Add(this);
        }

        private void Button_Pressed(object sender, EventArgs e)
        {
            if (ClosedOrCancelled != null) ClosedOrCancelled(this, new EventArgs());
        }

        private void Popup_VisibleChanged(object sender, EventArgs e)
        {
            spinner.Visible = Visible;
            button.Visible = Visible;
        }

        private void Popup_EnabledChanged(object sender, EventArgs e)
        {
            spinner.Enabled = Enabled;
            button.Enabled = Enabled;
        }

        private void Popup_DrawOrderChanged(object sender, EventArgs e)
        {
            spinner.DrawOrder = DrawOrder + 1;
            button.DrawOrder = DrawOrder + 2;
        }

        public override void Draw(GameTime gameTime)
        {
            Viewport v = Game.GraphicsDevice.Viewport;
            Rectangle rect = new Rectangle(104, 80, 192, 80);
            Vector2 textCenterPosition = new Vector2(216, 104);
            Vector2 textSize1 = spriteFont.MeasureString(Text1);
            Vector2 textSize2 = spriteFont.MeasureString(Text2);

            spriteBatch.Begin();
            spriteBatch.Draw(background, v.Bounds, Color.Black * 0.5f);
            spriteBatch.Draw(background, rect, Color.Black * 0.8f);
            spriteBatch.DrawString(spriteFont, Text1, textCenterPosition - new Vector2(0, textSize2.Y / 2), Color.White, 0.0f, textSize1 / 2, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.DrawString(spriteFont, Text2, textCenterPosition + new Vector2(0, textSize1.Y / 2), Color.White, 0.0f, textSize2 / 2, 1.0f, SpriteEffects.None, 0.0f);
            spriteBatch.End();

            if (Type == Types.Message)
            {
                button.Text = "OK";
                spinner.Color = Color.Green;
                spinner.Mode = Spinner.Modes.None;
            }
            else if (Type == Types.Error)
            {
                button.Text = "OK";
                spinner.Color = Color.Red;
                spinner.Mode = Spinner.Modes.Uniform;
            }
            else if (Type == Types.Operation)
            {
                button.Text = "CANCEL";
                spinner.Color = Color.Yellow;
                spinner.Mode = Spinner.Modes.Spin;
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("button");
            background = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            background.SetData<Color>(new Color[1] { Color.White });
        }

        // Events

        /// <summary>
        /// Fired when the user closes or cancels the popup.
        /// </summary>
        public event EventHandler ClosedOrCancelled;
    }
}
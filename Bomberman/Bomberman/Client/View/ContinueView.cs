using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman.Utilities;
using Bomberman.Widgets;
using Bomberman.Model;

namespace Bomberman.Client.View
{
    class ContinueView : MessageView
    {
        private static readonly Rectangle YesButtonBounds = new Rectangle(157, 129, 100, 16);
        private static readonly Rectangle GiveUpButtonBounds = new Rectangle(157, 150, 100, 16);
        private static readonly Vector2 timerPosition = new Vector2(202, 86);

        public Button YesButton { get; private set; }
        public Button GiveUpButton { get; private set; }

        private ClientController controller;
        private Status status;

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            spriteBatch.Begin();

            Utilities.Drawing.DrawCenteredText(spriteFont, spriteBatch, Miscellaneous.GetTimerString(status.TimeLimit), Origin + timerPosition, Color.Red, false);

            spriteBatch.End();
        }

        public ContinueView(Microsoft.Xna.Framework.Game game, Vector2 origin, ClientController controller, params string[] lines) 
            : base(game, origin, lines)
        {
            Origin = origin;
            Lines = lines;

            this.controller = controller;
            this.status = controller.Game.Status;

            Rectangle RealYesButtonBounds = YesButtonBounds;
            RealYesButtonBounds.Offset((int)Origin.X, (int)Origin.Y);
            YesButton = new Button(game) { Text = "Yes", Bounds = RealYesButtonBounds, BackgroundColor = Color.Gray, TextColor = Color.White };
            YesButton.Pressed += new EventHandler(YesButton_Pressed);

            Rectangle RealGiveUpButtonBounds = GiveUpButtonBounds;
            RealGiveUpButtonBounds.Offset((int)Origin.X, (int)Origin.Y);
            GiveUpButton = new Button(game) { Text = "Give Up", Bounds = RealGiveUpButtonBounds, BackgroundColor = Color.Gray, TextColor = Color.White };
            GiveUpButton.Pressed += new EventHandler(GiveUpButton_Pressed);

            EnabledChanged += new EventHandler<EventArgs>(ContinueView_EnabledChanged);
            VisibleChanged += new EventHandler<EventArgs>(ContinueView_VisibleChanged);
            DrawOrderChanged += new EventHandler<EventArgs>(ContinueView_DrawOrderChanged);

            ContinueView_DrawOrderChanged(this, new EventArgs());
        }

        private void YesButton_Pressed(object sender, EventArgs e)
        {
            controller.RequestContinue(true);
        }

        private void GiveUpButton_Pressed(object sender, EventArgs e)
        {
            controller.RequestContinue(false);
        }

        private void ContinueView_VisibleChanged(object sender, EventArgs e)
        {
            YesButton.Visible = Visible;
            GiveUpButton.Visible = Visible;
        }

        private void ContinueView_EnabledChanged(object sender, EventArgs e)
        {
            YesButton.Enabled = Enabled;
            GiveUpButton.Enabled = Enabled;
        }

        private void ContinueView_DrawOrderChanged(object sender, EventArgs e)
        {
            YesButton.DrawOrder = DrawOrder + 1;
            GiveUpButton.DrawOrder = DrawOrder + 2;
        }
    }
}

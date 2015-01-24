using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Model;
using Bomberman.Utilities;

namespace Bomberman.Client.View
{
    /// <summary>
    /// The top view displays informations about the status of the game.
    /// </summary>
    public sealed class TopView : DrawableGameComponent
    {
        private const int elementTimer = 0;
        private const int elementMode = 1;
        private const int elementRound = 2;
        private const int elementClock = 3;
        private const int elementLogo = 4;
        private static readonly Vector2[] elementPositions = { new Vector2(235, 14), new Vector2(70, 14), new Vector2(162, 14), new Vector2(191, 2), new Vector2(283, 1) };

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFontArial9;
        private Texture2D baseTexture;
        private Texture2D logoTexture;
        private Texture2D clockTexture;


        private Status status;

        private string GetModeString(Status.Modes mode)
        {
            if (mode == Status.Modes.Cooperation)
            {
                return "coop";
            }
            else if (mode == Status.Modes.Versus)
            {
                return "vs";
            }
            else
            {
                return "?";
            }
        }

        /// <summary>
        /// The origin of this view.
        /// </summary>
        public Vector2 Origin { get; set; }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFontArial9 = Game.Content.Load<SpriteFont>("arialBold9");
            
            baseTexture = Game.Content.Load<Texture2D>("gfx\\top\\base");
            clockTexture = Game.Content.Load<Texture2D>("gfx\\top\\clock");
            logoTexture = Game.Content.Load<Texture2D>("gfx\\top\\logo");
             
        }

        public override void Draw(GameTime gameTime)
        {
            string timerString = Miscellaneous.GetTimerString(status.TimeLimit);
            string modeString = this.GetModeString(status.Mode);
            string roundString = status.Round.ToString();

            spriteBatch.Begin();
            
            spriteBatch.Draw(baseTexture, Origin, Color.White);
            spriteBatch.Draw(clockTexture, Origin + elementPositions[elementClock], Color.White);
            spriteBatch.Draw(logoTexture, Origin + elementPositions[elementLogo], Color.White);
            
            Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, timerString, Origin + elementPositions[elementTimer], Color.Black, false);
            Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, modeString, Origin + elementPositions[elementMode], Color.Black, false);
            Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, roundString, Origin + elementPositions[elementRound], Color.Black, false);
            spriteBatch.End();
        }

        public TopView(Microsoft.Xna.Framework.Game game, Status status, Vector2 origin) : base(game)
        {
            if (status != null)
            {
                this.status = status;
                Origin = origin;

                DrawOrder = Order.OverlayLevel;

                game.Components.Add(this);
            }
            else
            {
                throw new ArgumentNullException("status");
            }
        }
    }
}
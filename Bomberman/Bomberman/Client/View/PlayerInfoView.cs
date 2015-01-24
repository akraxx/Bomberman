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
    public sealed class PlayerInfoView : DrawableGameComponent
    {
        private const int elementId = 0;
        private const int elementGlobal = 1;
        private const int elementSprite = 2;
        private const int elementScore = 3;
        private const int elementLives = 4;
        private static readonly Vector2[] elementPositions = { new Vector2(32, 8), new Vector2(32, 47), new Vector2(32, 33), new Vector2(45, 54), new Vector2(45, 71) };

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFontMain;
        private Texture2D baseTexture;
        private Texture2D gameoverTexture;
        private Texture2D playingTexture;
        private Texture2D legendPlayingTexture;
        private Texture2D legendGameoverTexture;

        private Player player;

        /// <summary>
        /// The origin of this view.
        /// </summary>
        public Vector2 Origin { get; set; }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFontMain = Game.Content.Load<SpriteFont>("arialBold9");

            baseTexture = Game.Content.Load<Texture2D>("gfx\\playerInfos\\base");
            gameoverTexture = Game.Content.Load<Texture2D>("gfx\\playerInfos\\gameover");
            playingTexture = Game.Content.Load<Texture2D>("gfx\\playerInfos\\playing");
            legendPlayingTexture = Game.Content.Load<Texture2D>("gfx\\playerInfos\\legendPlaying");
            legendGameoverTexture = Game.Content.Load<Texture2D>("gfx\\playerInfos\\legendGameover");
        }

        public override void Draw(GameTime gameTime)
        {
            Texture2D texture = baseTexture;
            string playerText = "PLAYER " + (player.ID + 1).ToString();
            Color playerColor = PlayerColors.List[player.ID];

            spriteBatch.Begin();
            spriteBatch.Draw(baseTexture, Origin, Color.White);

            if (!player.Playing)
            {
                Vector2 offset = new Vector2(0, spriteFontMain.MeasureString("N").Y / 2);
                Drawing.DrawCenteredText(spriteFontMain, spriteBatch, "Not", Origin + elementPositions[elementGlobal] - offset, playerColor, false);
                Drawing.DrawCenteredText(spriteFontMain, spriteBatch, "Present", Origin + elementPositions[elementGlobal] + offset, playerColor, false);
            }
            else if(player.Waiting)
            {
                Vector2 offset = Vector2.Zero;
                if (player.GameOver)
                {
                    offset = new Vector2(0, spriteFontMain.MeasureString("W").Y / 2);
                    Drawing.DrawCenteredText(spriteFontMain, spriteBatch, "GameOver", Origin + elementPositions[elementGlobal] + offset, playerColor, false);
                }

                Drawing.DrawCenteredText(spriteFontMain, spriteBatch, "Waiting", Origin + elementPositions[elementGlobal] - offset, playerColor, false);     
            }
            else
            {
                string scoreText = player.Score.ToString();
                string livesText = player.Stock.ToString();

                if (!player.GameOver)
                {
                    spriteBatch.Draw(playingTexture, Origin, Color.White);
                    spriteBatch.Draw(legendPlayingTexture, Origin, playerColor);
                    Drawing.DrawCenteredText(spriteFontMain, spriteBatch, livesText, Origin + elementPositions[elementLives], playerColor, false);
                }
                else
                {
                    spriteBatch.Draw(gameoverTexture, Origin, Color.White);
                    spriteBatch.Draw(legendGameoverTexture, Origin, playerColor);
                }

                Drawing.DrawCenteredText(spriteFontMain, spriteBatch, scoreText, Origin + elementPositions[elementScore], playerColor, false);

                if (player.Bomberman != null)
                {
                    Services.ISpriteDrawer spriteDrawer = (Services.ISpriteDrawer)Game.Services.GetService(typeof(Services.ISpriteDrawer));
                    spriteDrawer.DrawCreature(spriteBatch, player.Bomberman, Origin + elementPositions[elementSprite], Color.White, 0, Vector2.One);
                }
            }

            Drawing.DrawCenteredText(spriteFontMain, spriteBatch, playerText, Origin + elementPositions[elementId], playerColor, false);

            spriteBatch.End();
        }

        public PlayerInfoView(Microsoft.Xna.Framework.Game game, Player player, Vector2 origin) : base(game)
        {
            if (player != null)
            {
                this.player = player;
                Origin = origin;

                DrawOrder = Order.OverlayLevel;

                game.Components.Add(this);
            }
            else
            {
                throw new ArgumentNullException("player");
            }
        }
    }
}
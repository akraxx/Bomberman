using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Bomberman.Model;
using Bomberman.Utilities;
using Bomberman.Widgets;

namespace Bomberman.Client.View
{
    public sealed class LobbyView : DrawableGameComponent
    {
        private static readonly Vector2 titlePosition = new Vector2(200, 24);
        private static readonly Rectangle addMessageButtonBounds = new Rectangle(48, 200, 128, 16);
        private static readonly Rectangle startButtonBounds = new Rectangle(224, 200, 128, 16);
        private static readonly Rectangle instructionRectangle = new Rectangle(232, 48, 128, 128);
        private static readonly Vector2 firstRow = new Vector2(0, 48);

        private SpriteBatch spriteBatch;
        private SpriteFont titleFont;
        private SpriteFont rowFont;
        private Texture2D instructions;
        private Texture2D lobbyRow;

        //public Button AddMessageButton { get; private set; }
        public Button ChangeModeButton { get; private set; }
        public Button StartButton { get; private set; }

        private ClientController controller;

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            titleFont = Game.Content.Load<SpriteFont>("message");
            rowFont = Game.Content.Load<SpriteFont>("arialBold9");
            instructions = Game.Content.Load<Texture2D>("gfx\\instructions");
            lobbyRow = Game.Content.Load<Texture2D>("gfx\\lobbyRow");
        }

        public override void Update(GameTime gameTime)
        {
            Status status = controller.Game.Status;
            Player localPlayer = status.LocalPlayer;
            bool isHost = localPlayer != null && localPlayer.Host;
            if (status.Startable)
            {
                StartButton.Text = isHost ? "Start" : "Start (host only)";
                StartButton.Locked = !isHost;
            }
            else
            {
                StartButton.Text = "Not enough players";
                StartButton.Locked = true;
            }
            ChangeModeButton.Text = isHost ? "Change mode" : "Mode (host only)";
            ChangeModeButton.Locked = !isHost;
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            Status status = controller.Game.Status;
            string modeText = (status.Mode == Status.Modes.Cooperation ? "Cooperation mode" : "Versus mode");
            Drawing.DrawCenteredText(titleFont, spriteBatch, modeText, titlePosition, Color.White, true);
            spriteBatch.Draw(instructions, instructionRectangle, Color.White);
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                Vector2 position = firstRow + new Vector2(0, 2 * lobbyRow.Height * i);
                Player player = status.Players[i];
                string text = string.Format("{0}P - ", player.ID + 1);
                if (player.Playing)
                {
                    text += player.Name + (player.Host ? " [host]" : "");
                }
                else
                {
                    text += "Not Joined";
                }
                spriteBatch.Draw(lobbyRow, position, null, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(rowFont, text, position + new Vector2(8, 1), PlayerColors.List[player.ID]);
            }
            spriteBatch.End();
        }

        public LobbyView(Microsoft.Xna.Framework.Game game, ClientController controller) : base(game)
        {
            if (controller != null)
            {
                this.controller = controller;
                controller.ChatReset += new EventHandler<EventArgs>(Controller_ChatReset);
                controller.ChatMessageReceived += new EventHandler<EventArgs<string>>(Controller_ChatMessageReceived);

                /*
                AddMessageButton = new Button(game) { Text = "Add message", Bounds = addMessageButtonBounds };
                AddMessageButton.Pressed += new EventHandler(AddMessageButton_Pressed);
                AddMessageButton.Locked = true; // TODO: pas encore implémenté
                */

                ChangeModeButton = new Button(game) { Text = "", Bounds = addMessageButtonBounds };
                ChangeModeButton.Pressed += new EventHandler(ChangeModeButton_Pressed);

                StartButton = new Button(game) { Text = "", Bounds = startButtonBounds };
                StartButton.Pressed += new EventHandler(StartButton_Pressed);

                EnabledChanged += new EventHandler<EventArgs>(LobbyView_EnabledChanged);
                VisibleChanged += new EventHandler<EventArgs>(LobbyView_VisibleChanged);
                DrawOrderChanged += new EventHandler<EventArgs>(LobbyView_DrawOrderChanged);

                DrawOrder = Order.StaticLevel;

                game.Components.Add(this);
            }
            else
            {
                throw new ArgumentNullException("controller");
            }
        }

        // Event handlers

        private void Controller_ChatReset(object sender, EventArgs e)
        {
            // TODO: piste d'amélioration -> ajout d'une fenêtre de discussion
            System.Diagnostics.Debug.WriteLine("Chat reset");
        }

        private void Controller_ChatMessageReceived(object sender, EventArgs<string> e)
        {
            // TODO: piste d'amélioration -> ajout d'une fenêtre de discussion
            System.Diagnostics.Debug.WriteLine("Chat message received: " + e.Value);
        }

        /*
        private void AddMessageButton_Pressed(object sender, EventArgs e)
        {
            Guide.BeginShowKeyboardInput(PlayerIndex.One, "Add message", "Enter your message here.", "", AddMessageButton_InputCompleted, null);
        }

        private void AddMessageButton_InputCompleted(IAsyncResult r)
        {
            string result = Guide.EndShowKeyboardInput(r);
            controller.SendChatMessage(result);
            Game.ResetElapsedTime();
        }
        */

        private void ChangeModeButton_Pressed(object sender, EventArgs e)
        {
            controller.RequestModeToggle();
        }

        private void StartButton_Pressed(object sender, EventArgs e)
        {
            controller.RequestStart();
        }

        private void LobbyView_EnabledChanged(object sender, EventArgs e)
        {
            //AddMessageButton.Enabled = Enabled;
            ChangeModeButton.Enabled = Enabled;
            StartButton.Enabled = Enabled;
        }

        private void LobbyView_VisibleChanged(object sender, EventArgs e)
        {
            //AddMessageButton.Visible = Visible;
            ChangeModeButton.Visible = Visible;
            StartButton.Visible = Visible;
        }

        private void LobbyView_DrawOrderChanged(object sender, EventArgs e)
        {
            //AddMessageButton.DrawOrder = DrawOrder + 1;
            ChangeModeButton.Visible = Visible;
            StartButton.DrawOrder = DrawOrder + 2;
        }
    }
}
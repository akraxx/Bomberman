using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Model;
using Bomberman.Widgets;
using Bomberman.Utilities;

namespace Bomberman.Client.View
{
    public sealed class ViewManager : GameComponent
    {
        private static readonly Vector2 topViewPosition = Vector2.Zero;
        private static readonly Vector2 mapViewPosition = new Vector2(64, 32);
        private static readonly Vector2 weaponViewPosition = new Vector2(0, 208);
        private static readonly Vector2[] playerViewPositions = new Vector2[4] { new Vector2(0, 32), new Vector2(0, 120), new Vector2(336, 32), new Vector2(336, 120) };

        private static readonly Vector2 fadingMessageOffset = new Vector2(0, -24);
        private static readonly Vector2 dualScrollMessageOffset = new Vector2(0, 8);

        private Status status;
        private Map map;

        private Queue<string> messageQueue = new Queue<string>();

        // Core views
        private LobbyView lobbyView;
        private MapView mapView;
        private TopView topView;
        private WeaponView weaponView;
        private ContinueView continueView;
        private MessageView clearedView;
        private MessageView gameOverView;
        private PlayerInfoView[] playerInfoView;

        // Kikoo views
        private FadingMessage fadingMessage;
        private DualScrollMessage dualScrollMessage;
        private Spinner spinner;

        /// <summary>
        /// Access the lobby view.
        /// </summary>
        public LobbyView LobbyView { get { return lobbyView; } }

        /// <summary>
        /// Access the map view.
        /// </summary>
        public MapView MapView { get { return mapView; } }

        /// <summary>
        /// Access the weapon view.
        /// </summary>
        public WeaponView WeaponView { get { return weaponView; } }

        private void ApplyPhase()
        {
            Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Game.Services.GetService(typeof(Services.ISoundPlayer));

            this.ToggleMusic("lobby", status.Phase == Status.Phases.Lobby);
            this.ToggleMusic("normal", status.Phase == Status.Phases.Ready || status.Phase == Status.Phases.Ingame);
            this.ToggleMusic("continue", status.Phase == Status.Phases.Continue);

            if (status.Phase == Status.Phases.Lobby)
            {
                // Nothing to do
            }
            else if (status.Phase == Status.Phases.Load)
            {
                // Nothing to do
            }
            else if (status.Phase == Status.Phases.Ready)
            {
                messageQueue.Enqueue("GET READY!!");
            }
            else if (status.Phase == Status.Phases.Ingame)
            {
                dualScrollMessage.LeftText = "ROUND ";
                dualScrollMessage.RightText = " START";
                dualScrollMessage.Visible = true;
            }
            else if (status.Phase == Status.Phases.Continue)
            {
                // Nothing to do
            }
            else if (status.Phase == Status.Phases.Cleared)
            {
                Player winner = status.Winner;
                if (status.Winner != null)
                {
                    if (winner == status.LocalPlayer)
                    {
                        clearedView.Lines = new string[2] { "CONGRATULATIONS", "You won!" };
                    }
                    else
                    {
                        clearedView.Lines = new string[2] { string.Format("{0} won!", winner.Name), "Better luck next time!" };
                    }
                }
                else
                {
                    clearedView.Lines = new string[2] { "CONGRATULATIONS", "All rounds beaten!" };
                }
                soundPlayer.Play("cleared");
            }
            else if (status.Phase == Status.Phases.GameOver)
            {
                soundPlayer.Play("gameOver");
            }
        }

        private void UpdateVisibility()
        {
            bool lobbyRunning = Enabled && status.Phase == Status.Phases.Lobby;
            bool loading = Enabled && status.Phase == Status.Phases.Load;
            bool gameRunning = Enabled && status.Phase != Status.Phases.Lobby && status.Phase != Status.Phases.Ended;
            bool gameplayRunning = Enabled && (status.Phase == Status.Phases.Ingame || status.Phase == Status.Phases.Ready);
            bool continueRunning = Enabled && status.Phase == Status.Phases.Continue;
            bool clearedRunning = Enabled && status.Phase == Status.Phases.Cleared;
            bool gameOverRunning = Enabled && status.Phase == Status.Phases.GameOver;

            lobbyView.Enabled = lobbyView.Visible = lobbyRunning;
            mapView.Visible = gameplayRunning;
            topView.Visible = gameRunning;
            weaponView.Visible = gameRunning;
            continueView.Visible = continueRunning;
            clearedView.Visible = clearedRunning;
            gameOverView.Visible = gameOverRunning;
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                playerInfoView[i].Visible = gameRunning;
            }
            spinner.Visible = loading;
        }

        private void ToggleMusic(string name, bool state)
        {
            Services.IMusicPlayer musicPlayer = (Services.IMusicPlayer)Game.Services.GetService(typeof(Services.IMusicPlayer));
            if (state && (musicPlayer.Name != name || !musicPlayer.IsPlaying))
            {
                musicPlayer.Load(name);
                musicPlayer.Play();
            }
            else if(!state && (musicPlayer.Name == name && musicPlayer.IsPlaying))
            {
                musicPlayer.Stop(TimeSpan.FromSeconds(3));
            }
        }

        private void StopMusic()
        {
            Services.IMusicPlayer musicPlayer = (Services.IMusicPlayer)Game.Services.GetService(typeof(Services.IMusicPlayer));
            musicPlayer.Stop(TimeSpan.FromSeconds(3));
        }

        public override void Update(GameTime gameTime)
        {
            // Process the next fading message to display.
            if (messageQueue.Count > 0 && !fadingMessage.Visible)
            {
                fadingMessage.Text = messageQueue.Dequeue();
                fadingMessage.Visible = true;
            }
        }

        public override void Initialize()
        {
            GraphicsDevice graphics = Game.GraphicsDevice;
            Viewport viewport = graphics.Viewport;
            spinner.Center = new Vector2(viewport.Width, viewport.Height) / 2;
        }

        public ViewManager(Microsoft.Xna.Framework.Game game, ClientController controller) : base(game)
        {
            status = controller.Game.Status;
            map = controller.Game.Map;
            Debug.Assert(Status.MaxPlayers == playerViewPositions.Length);

            // Create core views
            lobbyView = new LobbyView(game, controller);
            topView = new TopView(game, status, topViewPosition);
            mapView = new MapView(game, controller, mapViewPosition);
            weaponView = new WeaponView(game, status, weaponViewPosition);
            continueView = new ContinueView(game, mapViewPosition, controller, "DO YOU WANT TO", "CONTINUE ?");
            clearedView = new MessageView(game, mapViewPosition, "");
            gameOverView = new MessageView(game, mapViewPosition, "GAME OVER");
            playerInfoView = new PlayerInfoView[Status.MaxPlayers];
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                playerInfoView[i] = new PlayerInfoView(game, status.Players[i], playerViewPositions[i]);
            }

            // Create kikoo views
            fadingMessage = new FadingMessage(game) { Offset = fadingMessageOffset };
            dualScrollMessage = new DualScrollMessage(game) { Offset = dualScrollMessageOffset };
            spinner = new Spinner(game) { Color = Color.Yellow, DrawOrder = Order.PopupLevel };

            // Register events
            status.PhaseChanged += new EventHandler<EventArgs>(Status_PhaseChanged);
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                status.Players[i].Left += new EventHandler<EventArgs>(Player_Left);
                status.Players[i].Joined += new EventHandler<EventArgs>(Player_Joined);
            }
            controller.TimeUp += new EventHandler<EventArgs>(Controller_TimeUp);

            EnabledChanged += new EventHandler<EventArgs>(ViewManager_EnabledChanged);
            Enabled = false;

            game.Components.Add(this);
        }

        // Event handlers

        private void ViewManager_EnabledChanged(object sender, EventArgs e)
        {
            if (Enabled)
            {
                this.ApplyPhase();
            }
            else
            {
                this.StopMusic();
            }
            this.UpdateVisibility();
        }

        private void Status_PhaseChanged(object sender, EventArgs e)
        {
            if (Enabled)
            {
                this.ApplyPhase();
                this.UpdateVisibility();
            }
        }

        private void Player_Joined(object sender, EventArgs e)
        {
            if (Enabled)
            {
                Player player = (Player)sender;
                messageQueue.Enqueue(string.Format(player.Local ? "Welcome, {0}!" : "[{0}] joined the game.", player.Name));
            }
        }

        private void Player_Left(object sender, EventArgs e)
        {
            if (Enabled)
            {
                Player player = (Player)sender;
                if (!player.Local)
                {
                    messageQueue.Enqueue(string.Format("[{0}] left the game.", player.Name));
                }
            }
        }

        private void Controller_TimeUp(object sender, EventArgs e)
        {
            if (Enabled)
            {
                messageQueue.Enqueue("TIME UP!!");
            }
        }
    }
}

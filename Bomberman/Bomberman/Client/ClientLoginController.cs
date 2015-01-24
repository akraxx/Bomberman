using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Bomberman.Network;
using Bomberman.Widgets;
using Bomberman.Utilities;
using Bomberman.Client.View;

namespace Bomberman.Client
{
    /// <summary>
    /// The controller of the login phase of the game.
    /// It manages state transitions.
    /// </summary>
    class ClientLoginController : GameComponent
    {
        private enum State
        {
            Starting,
            WaitForTitleScreen,
            WaitForModeSelection,
            WaitForMenu,
            WaitForConnection,
            WaitForNegociation,
            WaitForGame,
            WaitForEndPopup,
            Finished
        }

        private State status;
        private bool isCreating;

        private ClientController clientController;
        private ViewManager viewManager;
        private IMessageInterface serverInterface;

        private TitleScreen titleScreen;
        private Button createGameButton;
        private Button joinGameButton;
        private CreateGameMenu createGameMenu;
        private JoinGameMenu joinGameMenu;
        private Popup popup;

        private void HandleMessages()
        {
            // Only get messages from server interface while we are still in login phase.
            if (status == State.WaitForNegociation)
            {
                if (!serverInterface.Empty)
                {
                    MessageEvent m = serverInterface.Pull();
                    if (m.Type == MessageEvent.Types.DeniedGame)
                    {
                        popup.Visible = popup.Enabled = true;
                        popup.Type = Popup.Types.Error;
                        popup.Text1 = "Cannot enter game!";
                        popup.Text2 = ((ReasonCodes)m.Payload).ToString();
                    }
                    else if (m.Type == MessageEvent.Types.EnteredGame)
                    {
                        popup.Visible = popup.Enabled = false;

                        this.ChangeStatus(State.WaitForGame);

                        SocketMessageInterface _serverInterface = (SocketMessageInterface)serverInterface;
                        _serverInterface.Suspend();
                        _serverInterface.EndpointDown -= ServerInterface_EndpointDown;
                        clientController.ReplaceInterface(_serverInterface);
                        _serverInterface.Resume();

                        clientController.Enabled = true;
                        viewManager.Enabled = true;
                    }
                    else
                    {
                        Debug.WriteLine("[CLIENT] Unsupported login message received: " + m.Type.ToString());
#if DEBUG
                        Debugger.Break();
#endif
                    }
                }
            }
        }

        public ClientLoginController(Game game, ClientController clientController, ViewManager viewManager) : base(game)
        {
            this.clientController = clientController;
            this.viewManager = viewManager;

            serverInterface = new NullMessageInterface();

            status = State.Starting;

            titleScreen = new TitleScreen(game);
            titleScreen.Validated += new EventHandler(TitleScreen_Validated);

            createGameButton = new Button(game) { Text = "Create game", Bounds = new Rectangle(48, 176, 128, 16), Enabled = false, Visible = false };
            createGameButton.Pressed += new EventHandler(CreateGame_Pressed);

            joinGameButton = new Button(game) { Text = "Join game", Bounds = new Rectangle(224, 176, 128, 16), Enabled = false, Visible = false };
            joinGameButton.Pressed += new EventHandler(JoinGame_Pressed);

            createGameMenu = new CreateGameMenu(game);
            createGameMenu.Create.Pressed += new EventHandler(Connect_Pressed);

            joinGameMenu = new JoinGameMenu(game);
            joinGameMenu.Join.Pressed += new EventHandler(Connect_Pressed);

            popup = new Popup(game) { Enabled = false, Visible = false };
            popup.ClosedOrCancelled += new EventHandler(Popup_ClosedOrCancelled);

            Game.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            this.HandleMessages();

            if ((status == State.Starting || status == State.Finished) && !clientController.Enabled)
            {
                this.ChangeStatus(State.WaitForTitleScreen);
            }
            else if (status == State.WaitForTitleScreen)
            {
                if (titleScreen.Status == TitleScreen.State.Disabled)
                {
                    this.ChangeStatus(State.WaitForModeSelection);
                }
            }
            else if (status == State.WaitForGame && clientController.Ended)
            {
                clientController.Disconnect();

                clientController.Enabled = false;
                viewManager.Enabled = false;

                ReasonCodes reasonCode = clientController.DisconnectReason;
                if (reasonCode == ReasonCodes.EndKick)
                {
                    // Game ended normally
                    this.ChangeStatus(State.WaitForTitleScreen);
                }
                else
                {
                    // Game ended in a special way (kick or disconnection)
                    popup.Visible = popup.Enabled = true;
                    popup.Text1 = "Game interrupted!";
                    popup.Text2 = reasonCode.ToString();
                    popup.Type = Popup.Types.Error;

                    this.ChangeStatus(State.WaitForEndPopup);
                }
            }

            bool selectingMode = (status == State.WaitForModeSelection);
            createGameButton.Visible = createGameButton.Enabled = selectingMode;
            joinGameButton.Visible = joinGameButton.Enabled = selectingMode;

            createGameMenu.Visible = createGameMenu.Enabled = (status == State.WaitForMenu && isCreating);
            joinGameMenu.Visible = joinGameMenu.Enabled = (status == State.WaitForMenu && !isCreating);
        }

        private void ChangeStatus(State status)
        {
            if (status == State.WaitForTitleScreen)
            {
                titleScreen.Enabled = true;
                titleScreen.Show(this.status == State.Starting);
            }
            else if (status == State.WaitForConnection)
            {
                popup.Visible = popup.Enabled = true;
                popup.Text1 = "Please wait!";
                popup.Text2 = "Trying to connect...";
                popup.Type = Popup.Types.Operation;
            }
            else if (status == State.WaitForNegociation)
            {
                popup.Visible = popup.Enabled = true;
                popup.Type = Popup.Types.Operation;
                popup.Text1 = "Please wait!";
                popup.Text2 = "Negociating with server...";
            }
            this.status = status;
        }

        private void BeginConnection(string host)
        {
            if (host != null && host.Length > 0)
            {
                ClientConnection clientConnection = new ClientConnection();
                clientConnection.Completed += new EventHandler<EventArgs<SocketMessageInterface>>(ClientConnection_Completed);
                clientConnection.Connect(host);
            }
            else
            {
                popup.Type = Popup.Types.Error;
                popup.Text1 = "Invalid host name!";
                popup.Text2 = "Please try again.";
            }
        }

        // Event handlers

        void TitleScreen_Validated(object sender, EventArgs e)
        {
            if (status == State.WaitForTitleScreen)
            {
                titleScreen.Enabled = false;

                this.ChangeStatus(State.WaitForModeSelection);

                // TODO: son
            }
        }

        void CreateGame_Pressed(object sender, EventArgs e)
        {
            if (status == State.WaitForModeSelection)
            {
                isCreating = true;

                titleScreen.Hide(false);

                this.ChangeStatus(State.WaitForMenu);
            }
        }

        void JoinGame_Pressed(object sender, EventArgs e)
        {
            if (status == State.WaitForModeSelection)
            {
                isCreating = false;

                titleScreen.Hide(false);

                this.ChangeStatus(State.WaitForMenu);
            }
        }

        void Connect_Pressed(object sender, EventArgs e)
        {
            if (status == State.WaitForMenu)
            {
                this.ChangeStatus(State.WaitForConnection);

                this.BeginConnection(createGameMenu.HostName.Text);
            }
        }

        void Popup_ClosedOrCancelled(object sender, EventArgs e)
        {
            if (status == State.WaitForConnection || status == State.WaitForNegociation || status == State.WaitForEndPopup)
            {
                if (status == State.WaitForNegociation)
                {
                    serverInterface.Close();
                }
                this.ChangeStatus(State.Finished);
            }
            popup.Visible = popup.Enabled = false;
        }

        void ClientConnection_Completed(object sender, EventArgs<SocketMessageInterface> e)
        {
            if (status == State.WaitForConnection)
            {
                ClientConnection clientConnection = (ClientConnection)sender;
                if (clientConnection.Result == SocketError.Success)
                {
                    this.ChangeStatus(State.WaitForNegociation);

                    serverInterface = e.Value;
                    serverInterface.EndpointDown += new EventHandler(ServerInterface_EndpointDown);

                    string token = isCreating ? createGameMenu.AccessToken.Text : joinGameMenu.AccessToken.Text;
                    string name = isCreating ? createGameMenu.PlayerName.Text : joinGameMenu.PlayerName.Text;
                    LoginPayload loginPayload = new LoginPayload(token, name, Protocol.Version);
                    serverInterface.Send(new MessageEvent(isCreating ? MessageEvent.Types.CreateGame : MessageEvent.Types.JoinGame, loginPayload)); 
                }
                else
                {
                    popup.Visible = popup.Enabled = true;
                    popup.Type = Popup.Types.Error;
                    popup.Text1 = "Could not connect!";
                    popup.Text2 = clientConnection.Result.ToString();
                }
            }
        }

        void ServerInterface_EndpointDown(object sender, EventArgs e)
        {
            if (status == State.WaitForNegociation)
            {
                popup.Visible = popup.Enabled = true;
                popup.Type = Popup.Types.Error;
                popup.Text1 = "Connection interrupted!";
                popup.Text2 = "Please try again.";
            }
        }
    }
}
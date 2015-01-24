using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Server
{
    class GameServer
    {
        private Socket socket;

        private List<string> removeSessionList = new List<string>();
        private List<SocketMessageInterface> removeLoginList = new List<SocketMessageInterface>();

        public enum States
        {
            Stopped,
            Starting,
            Started,
            Stopping,
            Error,
        }

        /// <summary>
        /// Return the current server state.
        /// </summary>
        public States State { get; protected set; }

        /// <summary>
        /// Return the current server sessions.
        /// </summary>
        public Dictionary<string, Session> Sessions { get; protected set; }

        /// <summary>
        /// List of players in pending login situation.
        /// </summary>
        public List<SocketMessageInterface> Logins { get; protected set; }

        public GameServer()
        {
            State = States.Stopped;

            Sessions = new Dictionary<string, Session>();
            Logins = new List<SocketMessageInterface>();
        }

        public void Start()
        {
            if (State == States.Stopped || State == States.Error)
            {
                State = States.Starting;

                try
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    socket.ExclusiveAddressUse = true;
                    socket.Bind(new IPEndPoint(IPAddress.Any, Protocol.Port));
                    socket.Listen(10 * Bomberman.Model.Status.MaxPlayers);

                    Debug.WriteLine("Server started on port: " + Protocol.Port);

                    State = States.Started;

                    this.BeginAccept();
                }
                catch (Exception e)
                {
                    State = States.Error;

                    Debug.WriteLine("Couldn't bind socket: " + e);
                }
            }
        }

        public void Stop()
        {
            if (State == States.Started)
            {
                State = States.Stopping;

                foreach (KeyValuePair<string, Session> p in Sessions)
                {
                    p.Value.Controller.Shutdown();
                }

                Sessions.Clear();

                socket.Close(1);

                State = States.Stopped;
            }
        }

        public void Tick(TimeSpan elapsed)
        {
            if (State == States.Started)
            {
                this.HandleLogins();

                this.HandleSessions(elapsed);
            }
        }

        private void BeginAccept()
        {
            ServerConnection serverConnection = new ServerConnection();
            serverConnection.Completed += new EventHandler<EventArgs<SocketMessageInterface>>(ServerConnection_Completed);
            serverConnection.Accept(socket);
        }

        private void HandleLogins()
        {
            foreach (SocketMessageInterface clientInterface in Logins)
            {
                if (!clientInterface.Up)
                {
                    removeLoginList.Add(clientInterface);
                }
                else if (!clientInterface.Empty)
                {
                    MessageEvent m = clientInterface.Pull();
                    bool close = false;

                    try
                    {
                        if (m.Type == MessageEvent.Types.CreateGame || m.Type == MessageEvent.Types.JoinGame)
                        {
                            LoginPayload loginPayload = (LoginPayload)m.Payload;

                            if (loginPayload.Compatible)
                            {
                                if (loginPayload.Valid)
                                {
                                    if (m.Type == MessageEvent.Types.CreateGame)
                                    {
                                        if (!Sessions.ContainsKey(loginPayload.Token))
                                        {
                                            close = !this.CreateGame(clientInterface, loginPayload);
                                        }
                                        else
                                        {
                                            close = true;
                                            clientInterface.Send(new MessageEvent(MessageEvent.Types.DeniedGame, ReasonCodes.IllegalLogin));
                                        }
                                    }
                                    else
                                    {
                                        if (Sessions.ContainsKey(loginPayload.Token))
                                        {
                                            close = !this.JoinGame(clientInterface, loginPayload);
                                        }
                                        else
                                        {
                                            close = true;
                                            clientInterface.Send(new MessageEvent(MessageEvent.Types.DeniedGame, ReasonCodes.GameNotFound));
                                        }
                                    }
                                }
                                else
                                {
                                    close = true;
                                    clientInterface.Send(new MessageEvent(MessageEvent.Types.DeniedGame, ReasonCodes.IllegalLogin));
                                }
                            }
                            else
                            {
                                close = true;
                                clientInterface.Send(new MessageEvent(MessageEvent.Types.DeniedGame, ReasonCodes.VersionMismatch));
                            }
                        }
                        else
                        {
                            Debug.WriteLine("[SERVER] Unsupported login message received: " + m.Type.ToString());
#if DEBUG
                            Debugger.Break();
#endif
                        }
                    }
                    catch (Exception e)
                    {
                        close = true;

                        Debug.WriteLine("[SERVER] Encountered error: " + e);
                    }

                    if (close)
                    {
                        clientInterface.Close();
                    }
                    removeLoginList.Add(clientInterface);
                }
            }

            foreach (SocketMessageInterface clientInterface in removeLoginList)
            {
                Logins.Remove(clientInterface);
            }

            removeLoginList.Clear();
        }

        private void HandleSessions(TimeSpan elapsed)
        {
            foreach (KeyValuePair<string, Session> p in Sessions)
            {
                Session s = p.Value;
                if (!s.Ended)
                {
                    s.Tick(elapsed);
                }
                else
                {
                    removeSessionList.Add(p.Key);
                }
            }

            foreach (string s in removeSessionList)
            {
                Sessions.Remove(s);
            }

            removeSessionList.Clear();
        }

        private bool CreateGame(SocketMessageInterface clientInterface, LoginPayload loginPayload)
        {
            Session session = new Session();
            ServerController serverController = session.Controller;

            // Register the player in the game
            Player player = new Player(0);
            player.Join(loginPayload.Name, true, true);
            serverController.Join(player);
            clientInterface.Send(new MessageEvent(MessageEvent.Types.EnteredGame, null));

            // Now transfer the socket
            clientInterface.Suspend();
            serverController.ReplaceInterface(player.ID, clientInterface);
            clientInterface.Resume();

            Sessions.Add(loginPayload.Token, session);

            return true;
        }

        private bool JoinGame(SocketMessageInterface clientInterface, LoginPayload loginPayload)
        {
            Session session = Sessions[loginPayload.Token];
            ServerController serverController = session.Controller;

            Status status = serverController.Game.Status;
            if (status.Joinable)
            {
                Player unusedPlayer = status.NextUnusedPlayer;
                if (unusedPlayer != null)
                {
                    // Register the player in the game
                    Player player = new Player(unusedPlayer.ID);
                    player.Join(loginPayload.Name, false, true);
                    serverController.Join(player);
                    clientInterface.Send(new MessageEvent(MessageEvent.Types.EnteredGame, null));

                    // Now transfer the socket
                    clientInterface.Suspend();
                    serverController.ReplaceInterface(player.ID, clientInterface);
                    clientInterface.Resume();

                    return true;
                }
                else
                {
                    clientInterface.Send(new MessageEvent(MessageEvent.Types.DeniedGame, ReasonCodes.GameFull));
                }
            }
            else
            {
                clientInterface.Send(new MessageEvent(MessageEvent.Types.DeniedGame, ReasonCodes.GameNotJoinable));
            }
            return false;
        }

        private void ServerConnection_Completed(object sender, EventArgs<SocketMessageInterface> e)
        {
            if (e.Value != null)
            {
                Logins.Add(e.Value);

                this.BeginAccept();
            }
        }
    }
}
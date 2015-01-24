using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Server
{
    /// <summary>
    /// Server-side game controller.
    /// It contains most of the game rules and is tasked of synchronizing all players.
    /// </summary>
    public sealed class ServerController
    {
        private const int maxChatMessageLength = 128;
        private const int initialRound = 1;
        private static readonly TimeSpan loadTimer = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan readyTimer = TimeSpan.FromSeconds(3);
        private static readonly TimeSpan continueTimer = TimeSpan.FromSeconds(20.9);
        private static readonly TimeSpan gameOverTimer = TimeSpan.FromSeconds(8.0);
        private static readonly TimeSpan clearTimer = TimeSpan.FromSeconds(10.0);
        private TimeSpan phaseTimer = TimeSpan.Zero;

        private ServerMessageExecuter executer;

        private IMessageInterface[] clientInterfaces;

        private int findInterfaceID(IMessageInterface clientInterface)
        {
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                if (clientInterfaces[i] == clientInterface) return i;
            }
            throw new ArgumentOutOfRangeException("clientInterface");
        }

        private void _sendToOne(int interfaceIndex, MessageEvent m)
        {
            clientInterfaces[interfaceIndex].Send(m);
        }

        private void _sendToAll(MessageEvent m)
        {
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                _sendToOne(i, m);
            }
        }

        private void _notifyMap(int interfaceIndex)
        {
            IMessageInterface clientInterface = clientInterfaces[interfaceIndex];
            Level level = Game.Level;
            if (level != null)
            {
                Map map = Game.Map;
                clientInterface.Send(new MessageEvent(MessageEvent.Types.SetMap, new SetMapPayload(level.Number, level.Map.Width, level.Map.Height, level.Map.Theme)));

                foreach (Model.Object o in map.Objects)
                {
                    clientInterface.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Spawn, o)));
                }
                foreach (Creature c in map.SortedCreatures)
                {
                    clientInterface.Send(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Spawn, c)));
                }

                _notifyTimeLimit();
            }
        }

        private void _notifyMap()
        {
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                _notifyMap(i);
            }
        }

        private void _notifyOptions()
        {
            Status status = Game.Status;
            _sendToAll(new MessageEvent(MessageEvent.Types.Options, new OptionsPayload(status.Mode, status.Continuable, status.WinsThreshold)));
        }

        private void _notifyPlayer(Player player)
        {
            _sendToAll(new MessageEvent(MessageEvent.Types.PlayerState, new PlayerPayload(player)));
        }

        private void _notifyPhase()
        {
            _sendToAll(new MessageEvent(MessageEvent.Types.ChangePhase, Game.Status.Phase));
        }

        private void _notifyTimeLimit()
        {
            _sendToAll(new MessageEvent(MessageEvent.Types.SetTimeLimit, Game.Status.TimeLimit));
        }

        private void _showContinue()
        {
            Status status = Game.Status;
            status.SetPhase(Status.Phases.Continue);
            status.TimeLimit = continueTimer;
            _notifyTimeLimit();
            _notifyPhase();
        }

        private void _showGameOver()
        {
            Game.Status.SetPhase(Status.Phases.GameOver);
            phaseTimer = gameOverTimer;
            _notifyPhase();
        }

        private void _endGame(ReasonCodes reason)
        {
            Status status = Game.Status;
            if (status.Phase != Status.Phases.Ended)
            {
                status.SetPhase(Status.Phases.Ended);

                foreach (Player p in status.Players)
                {
                    if (p.Playing)
                    {
                        this.Kick(p, reason);
                    }
                }
            }
        }

        private void HandleMessages()
        {
            bool continueRoundRobin = true;
            while (continueRoundRobin)
            {
                continueRoundRobin = false;
                for (int i = 0; i < Status.MaxPlayers; i++)
                {
                    if (!clientInterfaces[i].Empty)
                    {
                        MessageEvent m = clientInterfaces[i].Pull();
                        executer.Execute(i, m);
                        continueRoundRobin = true;
                    }
                }
            }
        }

        /// <summary>
        /// True if the underlying game is ended.
        /// </summary>
        public bool Ended { get { return Game.Status.Phase == Status.Phases.Ended; } }

        /// <summary>
        /// The animator used by this server controller.
        /// </summary>
        public ServerAnimator Animator { get; private set; }

        /// <summary>
        /// The server version of the game.
        /// </summary>
        public Model.Game Game { get; private set; }

        /// <summary>
        /// Update the game state server-side.
        /// </summary>
        public void Update(TimeSpan elapsed)
        {

            // Message handling
            this.HandleMessages();

            // Periodic update
            Status status = Game.Status;
            if (status.Phase == Status.Phases.Lobby)
            {
                // Nothing to do
            }
            else if (status.Phase == Status.Phases.Load)
            {
                if (phaseTimer > TimeSpan.Zero)
                {
                    phaseTimer -= elapsed;
                }
                else
                {
                    status.SetPhase(Status.Phases.Ready);
                    phaseTimer = readyTimer;
                    _notifyPhase();
                }
            }
            else if (status.Phase == Status.Phases.Ready)
            {
                if (phaseTimer > TimeSpan.Zero)
                {
                    phaseTimer -= elapsed;
                }
                else
                {
                    status.SetPhase(Status.Phases.Ingame);
                    _notifyPhase();
                    Animator.Begin();
                }
            }
            else if (status.Phase == Status.Phases.Ingame)
            {
                if (Animator.State == ServerAnimator.States.Playing)
                {
                    Animator.Animate(elapsed);
                }
                else
                {
                    this.StartNextLevel();
                }
            }
            else if (status.Phase == Status.Phases.Continue)
            {
                if (status.TimeLimit > TimeSpan.Zero)
                {
                    status.TimeLimit -= elapsed;
                    if (status.TimeLimit < TimeSpan.Zero) status.TimeLimit = TimeSpan.Zero;
                }
                else
                {
                    foreach (Player p in status.Players)
                    {
                        if (p.GameOver && p.Waiting) this.ContinueGame(p.ID, false); // Players who haven't answered default to "give up".
                    }
                    this.StartNextLevel();
                }
            }
            else if (status.Phase == Status.Phases.Cleared)
            {
                if (phaseTimer > TimeSpan.Zero)
                {
                    phaseTimer -= elapsed;
                }
                else
                {
                    _showGameOver();
                }
            }
            else if (status.Phase == Status.Phases.GameOver)
            {
                if (phaseTimer > TimeSpan.Zero)
                {
                    phaseTimer -= elapsed;
                }
                else
                {
                    _endGame(ReasonCodes.EndKick);
                }
            }
        }

        /// <summary>
        /// Should be called when a player has joined the game after completing the login process.
        /// </summary>
        public void Join(Player player)
        {
            if (player != null)
            {
                Player targetPlayer = Game.Status.Players[player.ID];
                targetPlayer.Join(player.Name, player.Host, false);

                // Tell existing players of the new player
                PlayerPayload playerPayload = new PlayerPayload(targetPlayer);
                _sendToAll(new MessageEvent(MessageEvent.Types.PlayerJoined, playerPayload));
            }
            else
            {
                throw new ArgumentNullException("player");
            }
        }

        /// <summary>
        /// Kick the provided player with the specified reason.
        /// </summary>
        public void Kick(Player player, ReasonCodes reason)
        {
            if (player != null)
            {
                _sendToOne(player.ID, new MessageEvent(MessageEvent.Types.Kicked, reason));
                clientInterfaces[player.ID].Close();
            }
            else
            {
                throw new ArgumentNullException("player");
            }
        }

        /// <summary>
        /// Send a generic message to all players.
        /// </summary>
        public void Send(MessageEvent m)
        {
            if (m != null) _sendToAll(m);
        }

        /// <summary>
        /// Send a chat message from a player to all players.
        /// </summary>
        public void SendChatMessage(Player player, string text)
        {
            if (player != null && text != null)
            {
                string message = string.Format("[{0}] {1}", player.Name, text.Substring(0, Math.Min(text.Length, maxChatMessageLength)));
                _sendToAll(new MessageEvent(MessageEvent.Types.Chat, message));
            }
        }

        /// <summary>
        /// Send player state to all players.
        /// </summary>
        public void SendPlayerState(byte id)
        {
            _notifyPlayer(Game.Status.Players[id]);
        }

        /// <summary>
        /// Send creature state to all players.
        /// </summary>
        public void SendCreatureState(Creature c)
        {
            if (c != null)
            {
                _sendToAll(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.State, c)));
            }
        }

        /// <summary>
        /// Send creature position to all players.
        /// </summary>
        public void SendCreaturePosition(Creature c)
        {
            if (c != null)
            {
                _sendToAll(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Position, c)));
            }
        }

        /// <summary>
        /// Send creature animation to all players.
        /// </summary>
        public void SendCreatureAnimation(Creature c)
        {
            if (c != null)
            {
                _sendToAll(new MessageEvent(MessageEvent.Types.MapEvent, new MapEvent(MapEvent.Actions.Animation, c)));
            }
        }

        /// <summary>
        /// Set the options of the game.
        /// </summary>
        public void SetOptions(OptionsPayload options)
        {
            if (options != null)
            {
                Status status = Game.Status;
                if (status.Phase == Status.Phases.Lobby)
                {
                    status.SetMode(options.Mode);
                    status.Continuable = options.Continuable;
                    status.WinsThreshold = options.WinsThreshold;
                    _notifyOptions();
                }
            }
        }

        /// <summary>
        /// Should be called when the host wants to begin the game now.
        /// </summary>
        public void StartGame()
        {
            if (Game.Status.Startable)
            {
                this.StartLevel(initialRound);
            }
        }

        /// <summary>
        /// Should be called when a player has given his answer to
        /// whether he wants to continue the game or give up.
        /// </summary>
        public void ContinueGame(byte id, bool continueGame)
        {
            Status status = Game.Status;
            if (status.Phase == Status.Phases.Continue)
            {
                Player player = status.Players[id];
                if (player.GameOver)
                {
                    if (continueGame)
                    {
                        player.Continue();
                    }
                    else
                    {
                        player.Waiting = false;
                    }
                    _notifyPlayer(player);
                }
            }
        }

        /// <summary>
        /// Should be called when it's time to begin the specified game level.
        /// Will trigger the 'Cleared' state if the game should not continue.
        /// </summary>
        private void StartLevel(int number)
        {
            Status status = Game.Status;
            Level level = Level.GetPredefinedLevel(status.Mode, number);

            // Start back to level 1 in vs. mode
            if (level == null && status.Mode == Status.Modes.Versus)
            {
                level = Level.GetPredefinedLevel(status.Mode, 1);
            }

            // Check if we are out of levels or if there is a winner in vs. mode
            if (level == null || status.Winner != null)
            {
                status.SetPhase(Status.Phases.Cleared);
                phaseTimer = clearTimer;
                _notifyPhase();
            }
            else
            {
                Game.LoadLevel(level);
                phaseTimer = loadTimer;
                _notifyPhase();
                _notifyMap();
            }
        }

        /// <summary>
        /// Should be called when it's time to begin the next game level.
        /// Will show continue screen if a player is in game over state and may continue.
        /// Will show game over screen if all players are in game over state.
        /// Will throw an exception if the animator is still running.
        /// </summary>
        private void StartNextLevel()
        {
            if (Animator.State != ServerAnimator.States.Playing)
            {
                Status status = Game.Status;
                if (status.Players.Any(p => p.GameOver && p.Waiting))
                {
                    _showContinue();
                }
                else if (!status.Players.Any(p => p.Playing && !p.GameOver))
                {
                    _showGameOver();
                }
                else
                {
                    if (Animator.State == ServerAnimator.States.NextLevel)
                    {
                        this.StartLevel(status.Round + 1); // Advance to the next level.
                    }
                    else
                    {
                        this.StartLevel(status.Round); // Retry the current level.
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Animator is still running");
            }
        }

        /// <summary>
        /// Remove an object from the map.
        /// </summary>
        public void RemoveObject(Model.Object o)
        {
            if (o != null)
            {
                MapEvent mapEvent = new MapEvent(MapEvent.Actions.Despawn, o);
                _sendToAll(new MessageEvent(MessageEvent.Types.MapEvent, mapEvent));
                Game.Map.Remove(o);
            }
        }

        /// <summary>
        /// Remove a creature from the map.
        /// </summary>
        public void RemoveCreature(Creature c)
        {
            if (c != null)
            {
                MapEvent mapEvent = new MapEvent(MapEvent.Actions.Despawn, c);
                _sendToAll(new MessageEvent(MessageEvent.Types.MapEvent, mapEvent));
                Game.Map.Remove(c);
            }
        }

        /// <summary>
        /// Stops brutally the game.
        /// </summary>
        public void Shutdown()
        {
            _endGame(ReasonCodes.Disconnected);
        }

        /// <summary>
        /// Replace a client message interface by a new one.
        /// </summary>
        public void ReplaceInterface(int index, IMessageInterface newInterface)
        {
            if (newInterface != null)
            {
                if (!clientInterfaces[index].Up)
                {
                    if (!newInterface.Up)
                    {
                        clientInterfaces[index] = newInterface;
                        newInterface.EndpointUp += new EventHandler(ServerController_EndpointUp);
                        newInterface.EndpointDown += new EventHandler(ServerController_EndpointDown);
                    }
                    else
                    {
                        throw new InvalidOperationException("New interface must be down");
                    }
                }
                else
                {
                    throw new InvalidOperationException("Previous interface is still up");
                }
            }
            else
            {
                throw new ArgumentNullException("newInterface");
            }
        }

        public ServerController()
        {
            Game = new Model.Game();

            Animator = new ServerAnimator(this);
            executer = new ServerMessageExecuter(this);

            clientInterfaces = new IMessageInterface[Status.MaxPlayers];
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                clientInterfaces[i] = new NullMessageInterface();
            }
        }

        // Event handlers

        private void ServerController_EndpointUp(object sender, EventArgs e)
        {
            IMessageInterface clientInterface = (IMessageInterface)sender;
            int id = this.findInterfaceID(clientInterface);
            Status status = Game.Status;
            Level level = Game.Level;

            // Send player list
            for (int i = 0; i < Status.MaxPlayers; i++)
            {
                Player player = status.Players[i];
                if (player.Playing)
                {
                    PlayerPayload playerPayload = new PlayerPayload(player);
                    playerPayload.Local = (player.ID == id);
                    clientInterface.Send(new MessageEvent(MessageEvent.Types.PlayerJoined, playerPayload));
                }
            }

            // Send game status
            clientInterface.Send(new MessageEvent(MessageEvent.Types.ChangePhase, status.Phase));

            // Send game map
            _notifyMap(id);
        }

        private void ServerController_EndpointDown(object sender, EventArgs e)
        {
            IMessageInterface clientInterface = (IMessageInterface)sender;
            int id = this.findInterfaceID(clientInterface);
            Player player = Game.Status.Players[id];

            // Remove the bomberman of this player
            this.RemoveCreature(Game.Map.Bombermen.FirstOrDefault(b => b.Player == player.ID));

            // Remove the player
            player.Leave();
            _sendToAll(new MessageEvent(MessageEvent.Types.PlayerLeft, player.ID));

            // Check if all players have left
            if (Game.Status.ActivePlayers == 0) _endGame(ReasonCodes.AbortedKick);

            // Check if host has left in lobby phase
            if (Game.Status.HostPlayer == null && Game.Status.Phase == Status.Phases.Lobby) _endGame(ReasonCodes.AbortedKick);
        }
    }
}
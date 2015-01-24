using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Client
{
    /// <summary>
    /// Client-side game controller.
    /// It is responsible for moving the local player character and handling his input.
    /// </summary>
    public sealed class ClientController : GameComponent
    {
        private ClientGameInput input;
        private ClientMessageExecuter executer;
        private CreatureMover mover;

        private IMessageInterface serverInterface;

        /// <summary>
        /// The client version of the game.
        /// </summary>
        public new Model.Game Game { get; private set; }

        /// <summary>
        /// True if the underlying game is ended.
        /// </summary>
        public bool Ended { get { return Game.Status.Phase == Status.Phases.Ended; } }

        /// <summary>
        /// This contains the reason why the client has been disconnected from the server.
        /// This property is not final until the status is set to 'Ended'.
        /// </summary>
        public ReasonCodes DisconnectReason { get; internal set; }

        private void _send(MessageEvent m)
        {
            serverInterface.Send(m);
        }

        private Model.Bomberman _getControlledBomberman()
        {
            Status status = Game.Status;
            if (status.Phase == Status.Phases.Ingame)
            {
                Player localPlayer = status.LocalPlayer;
                if (localPlayer != null)
                {
                    Model.Bomberman bomberman = localPlayer.Bomberman;
                    if (bomberman != null && !bomberman.Dead)
                    {
                        return bomberman;
                    }
                }
            }
            return null;
        }

        private void HandleMessages()
        {
            while (!serverInterface.Empty)
            {
                MessageEvent m = serverInterface.Pull();
                executer.Execute(m);
            }
        }

        /// <summary>
        /// Update the game state client-side.
        /// </summary>
        public void Update(TimeSpan elapsed)
        {
            // Message handling
            this.HandleMessages();

            // Periodic update
            Status status = Game.Status;
            if (status.Phase == Status.Phases.Ingame)
            {
                Map map = Game.Map;

                // Time limit
                if (status.TimeLimit > TimeSpan.Zero)
                {
                    status.TimeLimit -= elapsed;
                    if (status.TimeLimit < TimeSpan.Zero) status.TimeLimit = TimeSpan.Zero;
                }

                // User input
                Model.Bomberman bomberman = _getControlledBomberman();
                Vector2 bombermanPosition = bomberman != null ? bomberman.Position : Vector2.Zero;
                if(bomberman != null)
                {
                    Vector2 movement = input.Moving ? (CreatureMover.AlignPosition(input.Movement, true) - bomberman.Position) : Vector2.Zero;
                    //Vector2 movement = input.Moving ? input.Movement - bomberman.Position : Vector2.Zero;
                    if (movement.Length() > CreatureMover.StopThreshold)
                    {
                        movement.Normalize();
                        bomberman.Velocity = movement * bomberman.Speed * CreatureMover.BaseSpeed;
                    }
                    else
                    {
                        bomberman.Velocity = Vector2.Zero;
                    }
                }

                // Animate objects
                foreach (Model.Object o in map.Objects)
                {
                    if (o is TimedObject)
                    {
                        TimedObject obj = (TimedObject)o;
                        obj.Timer -= elapsed;
                        if (obj.Timer < TimeSpan.Zero) obj.Timer = TimeSpan.Zero;
                    }
                }

                // Animate creatures
                foreach (Creature c in map.SortedCreatures)
                {
                    // Invulnerability
                    c.Invulnerability -= elapsed;
                    if (c.Invulnerability < TimeSpan.Zero) c.Invulnerability = TimeSpan.Zero;

                    // Movement
                    mover.SetActive(c);
                    mover.MoveBy(c.Velocity * (float)elapsed.TotalSeconds);

                    // Sprite animation
                    if (c.Velocity != Vector2.Zero)
                    {
                        c.SpriteState.Orientation = Orientation.OrientationOf(c.Velocity);
                        if (c is Model.Bomberman && !c.Dead)
                        {
                            c.SpriteState.Begin(Actions.Walk);
                        }
                    }
                    else
                    {
                        if (c is Model.Bomberman && !c.Dead)
                        {
                            c.SpriteState.Begin(Actions.Idle);
                        }
                    }
                    c.SpriteState.Tick();
                }
                map.SortCreatures();

                // Position feedback
                if (bomberman != null)
                {
                    //if (bomberman.Position != bombermanPosition)
                    //{
                        PositionPayload positionPayload = new PositionPayload(bomberman.Position, bomberman.Velocity);
                        serverInterface.Send(new MessageEvent(MessageEvent.Types.PlayerPosition, positionPayload));
                    //}
                }
            }
            else if (status.Phase == Status.Phases.Continue)
            {
                // Continue timer
                status.TimeLimit -= elapsed;
                if (status.TimeLimit < TimeSpan.Zero) status.TimeLimit = TimeSpan.Zero;
            }
        }

        /// <summary>
        /// Leave the game.
        /// </summary>
        public void Disconnect()
        {
            serverInterface.Close();
        }

        /// <summary>
        /// Send a chat message to the client.
        /// </summary>
        public void SendChatMessage(string text)
        {
            if (text != null) serverInterface.Send(new MessageEvent(MessageEvent.Types.Chat, text));
        }

        /// <summary>
        /// Should be called when a chat message is received.
        /// </summary>
        public void NotifyChatMessage(string text)
        {
            if (text != null && ChatMessageReceived != null) ChatMessageReceived(this, new EventArgs<string>(text));
        }

        /// <summary>
        /// Should be called when an object is destroyed.
        /// </summary>
        public void NotifyDestroy(Model.Object obj)
        {
            if (ObjectDestroyed != null) ObjectDestroyed(this, new EventArgs<Model.Object>(obj));
        }

        /// <summary>
        /// Should be called when a creature has its health changed.
        /// </summary>
        public void NotifyHealth(Creature c)
        {
            if (CreatureHealthChanged != null) CreatureHealthChanged(this, new EventArgs<Creature>(c));
        }

        /// <summary>
        /// Should be called when an object is picked.
        /// </summary>
        public void NotifyPicked(Model.Object obj)
        {
            if (ObjectPicked != null) ObjectPicked(this, new EventArgs<Model.Object>(obj));
        }

        /// <summary>
        /// Should be called when time's up.
        /// </summary>
        public void NotifyTimeUp()
        {
            if (TimeUp != null) TimeUp(this, new EventArgs());
        }

        /// <summary>
        /// Ask the server to start the game.
        /// </summary>
        public void RequestStart()
        {
            serverInterface.Send(new MessageEvent(MessageEvent.Types.StartGame, null));
        }

        /// <summary>
        /// Tell the server we want to continue/give up the game on the continue screen.
        /// </summary>
        public void RequestContinue(bool continueGame)
        {
            serverInterface.Send(new MessageEvent(MessageEvent.Types.ContinueGame, continueGame));
        }

        /// <summary>
        /// Tell the server we want to change the game mode.
        /// </summary>
        public void RequestModeToggle()
        {
            Status.Modes newMode = (Game.Status.Mode == Status.Modes.Cooperation ? Status.Modes.Versus : Status.Modes.Cooperation);
            OptionsPayload newOptions = new OptionsPayload(newMode, Game.Status.Continuable, Game.Status.WinsThreshold);
            serverInterface.Send(new MessageEvent(MessageEvent.Types.Options, newOptions));
        }

        /// <summary>
        /// Connect the game input that the client controller listens to.
        /// </summary>
        public void ConnectInput(ClientGameInput input)
        {
            if (this.input == null)
            {
                this.input = input;
                input.RequestBomb += new EventHandler<EventArgs<Bomb.Types>>(Input_RequestBomb);
                input.RequestDetonate += new EventHandler<EventArgs<Bomb>>(Input_RequestDetonate);
            }
            else
            {
                throw new InvalidOperationException("An input is already connected");
            }
        }

        /// <summary>
        /// Replace the server message interface by a new one.
        /// </summary>
        public void ReplaceInterface(IMessageInterface newInterface)
        {
            if (newInterface != null)
            {
                if (!serverInterface.Up)
                {
                    if (!newInterface.Up)
                    {
                        serverInterface = newInterface;
                        newInterface.EndpointUp += new EventHandler(ClientController_EndpointUp);
                        newInterface.EndpointDown += new EventHandler(ClientController_EndpointDown);
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

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            this.Update(gameTime.ElapsedGameTime);
        }

        public ClientController(Microsoft.Xna.Framework.Game game) : base(game)
        {
            Game = new Model.Game();
            DisconnectReason = ReasonCodes.Disconnected;

            input = null;
            executer = new ClientMessageExecuter(this);
            mover = new CreatureMover(Game.Map);

            serverInterface = new NullMessageInterface();

            game.Components.Add(this);
        }

        // Event handlers

        private void ClientController_EndpointUp(object sender, EventArgs e)
        {
            Game.Reset();

            // Reset the disconnection reason
            DisconnectReason = ReasonCodes.Disconnected;
        }

        private void ClientController_EndpointDown(object sender, EventArgs e)
        {
            Game.Status.SetPhase(Status.Phases.Ended);

            if (ChatReset != null) ChatReset(this, new EventArgs());
        }

        private void Input_RequestDetonate(object sender, EventArgs<Bomb> e)
        {
            Model.Bomberman bomberman = _getControlledBomberman();
            if (bomberman != null)
            {
                serverInterface.Send(new MessageEvent(MessageEvent.Types.PlayerDetonate, e.Value.ID));
            }
        }

        private void Input_RequestBomb(object sender, EventArgs<Bomb.Types> e)
        {
            Model.Bomberman bomberman = _getControlledBomberman();
            if (bomberman != null)
            {
                serverInterface.Send(new MessageEvent(MessageEvent.Types.PlayerBomb, e.Value));
            }
        }

        // Events

        /// <summary>
        /// Fired when a chat message is received.
        /// </summary>
        public event EventHandler<EventArgs<string>> ChatMessageReceived;

        /// <summary>
        /// Fired when the chat is reset.
        /// </summary>
        public event EventHandler<EventArgs> ChatReset;

        /// <summary>
        /// Fired when an object is destroyed.
        /// </summary>
        public event EventHandler<EventArgs<Model.Object>> ObjectDestroyed;

        /// <summary>
        /// Fired when an object is picked.
        /// </summary>
        public event EventHandler<EventArgs<Model.Object>> ObjectPicked;

        /// <summary>
        /// Fired when a creature has its health changed.
        /// </summary>
        public event EventHandler<EventArgs<Creature>> CreatureHealthChanged;

        /// <summary>
        /// Fired when time's up.
        /// </summary>
        public event EventHandler<EventArgs> TimeUp;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bomberman.Model;

namespace Bomberman.Network
{
    /// <summary>
    /// Top-level portion of all network messages.
    /// </summary>
    public sealed class MessageEvent
    {
        public enum Types
        {
            // ***** Client exclusive login commands *****

            /// <summary>
            /// Request to create a game.
            /// Payload: LoginPayload
            /// </summary>
            CreateGame,

            /// <summary>
            /// Request to join a game.
            /// Payload: LoginPayload
            /// </summary>
            JoinGame,

            // ***** Client exclusive game commands *****

            /// <summary>
            /// Request to start the game (restricted to host).
            /// Payload: null
            /// </summary>
            StartGame,

            /// <summary>
            /// Gives answer to the continue screen.
            /// Payload: bool (true: continue, false: give up)
            /// </summary>
            ContinueGame,

            /// <summary>
            /// Gives latest position of the player.
            /// Payload: PositionPayload
            /// </summary>
            PlayerPosition,

            /// <summary>
            /// Request a bomb drop.
            /// Payload: BombType
            /// </summary>
            PlayerBomb,

            /// <summary>
            /// Request a remote bomb detonation.
            /// Payload: Bomb ID
            /// </summary>
            PlayerDetonate,

            // ***** Server general commands *****

            /// <summary>
            /// Notify a player he has been removed from the server.
            /// The connection should be closed immediately after.
            /// Payload: ReasonCode
            /// </summary>
            Kicked,

            // ***** Server exclusive login commands *****

            /// <summary>
            /// Notify a player he cannot enter the requested game.
            /// Payload: ReasonCode
            /// </summary>
            DeniedGame,

            /// <summary>
            /// Notify a player he has been accepted in the game.
            /// Payload: null
            /// </summary>
            EnteredGame,

            // ***** Server exclusive game commands *****

            /// <summary>
            /// Notify a player has joined the game.
            /// Payload: PlayerPayload
            /// </summary>
            PlayerJoined,

            /// <summary>
            /// Notify a player has left the game.
            /// Payload: Player ID
            /// </summary>
            PlayerLeft,

            /// <summary>
            /// Notify a player state has changed.
            /// State includes waiting/gameover flags, score, wins, stock.
            /// Payload: PlayerPayload
            /// </summary>
            PlayerState,

            /// <summary>
            /// Notify game phase has changed.
            /// Payload: Phase
            /// </summary>
            ChangePhase,

            /// <summary>
            /// Notify a new map.
            /// Payload: SetMapPayload
            /// </summary>
            SetMap,

            /// <summary>
            /// Notify a time limit.
            /// Payload: TimeLimit
            /// </summary>
            SetTimeLimit,

            /// <summary>
            /// Notify time up.
            /// Payload: null
            /// </summary>
            TimeUp,

            /// <summary>
            /// Notify a map event occured.
            /// Payload: MapEvent
            /// </summary>
            MapEvent,

            // ***** Shared game commands *****

            /// <summary>
            /// Change (client) / Notify (server) chat text message.
            /// Payload: string (message)
            /// </summary>
            Chat,

            /// <summary>
            /// Change (client) / Notify (server) game options.
            /// Payload: OptionsPayload
            /// </summary>
            Options,

            /// <summary>
            /// Change (client) / Notify (server) pause.
            /// Payload: bool (new pause state)
            /// </summary>
            PauseGame,
        }

        /// <summary>
        /// Type of the message event.
        /// </summary>
        public Types Type { get; set; }

        /// <summary>
        /// Payload of the message event.
        /// Refer to the types above for what should be expected as payload.
        /// </summary>
        public System.Object Payload { get; set; }

        /// <summary>
        /// Read a MessageEvent from the specified binary reader.
        /// </summary>
        public static MessageEvent Read(BinaryReader r)
        {
            MessageEvent m = new MessageEvent();
            m.Type = (Types)r.ReadByte();
            if (m.Type == Types.CreateGame || m.Type == Types.JoinGame)
            {
                m.Payload = LoginPayload.Read(r);
            }
            else if (m.Type == Types.ContinueGame)
            {
                m.Payload = r.ReadBoolean();
            }
            else if (m.Type == Types.PlayerPosition)
            {
                m.Payload = PositionPayload.Read(r);
            }
            else if (m.Type == Types.PlayerBomb)
            {
                m.Payload = (Bomb.Types)r.ReadInt32();
            }
            else if (m.Type == Types.PlayerDetonate)
            {
                m.Payload = r.ReadUInt16();
            }
            else if (m.Type == Types.Kicked || m.Type == Types.DeniedGame)
            {
                m.Payload = (ReasonCodes)r.ReadInt32();
            }
            else if (m.Type == Types.PlayerJoined)
            {
                m.Payload = PlayerPayload.Read(r);
            }
            else if (m.Type == Types.PlayerLeft)
            {
                m.Payload = r.ReadByte();
            }
            else if (m.Type == Types.PlayerState)
            {
                m.Payload = PlayerPayload.Read(r);
            }
            else if (m.Type == Types.ChangePhase)
            {
                m.Payload = (Status.Phases)r.ReadInt32();
            }
            else if (m.Type == Types.SetMap)
            {
                m.Payload = SetMapPayload.Read(r);
            }
            else if (m.Type == Types.SetTimeLimit)
            {
                m.Payload = TimeSpan.FromTicks(r.ReadInt64());
            }
            else if (m.Type == Types.MapEvent)
            {
                m.Payload = MapEvent.Read(r);
            }
            else if (m.Type == Types.Chat)
            {
                m.Payload = r.ReadString();
            }
            else if (m.Type == Types.Options)
            {
                m.Payload = OptionsPayload.Read(r);
            }
            else if (m.Type == Types.PauseGame)
            {
                m.Payload = r.ReadBoolean();
            }
            else
            {
                m.Payload = null;
            }
            return m;
        }

        /// <summary>
        /// Write the MessageEvent to the specified binary writer.
        /// </summary>
        public void Write(BinaryWriter w)
        {
            w.Write((byte)Type);
            if (Type == Types.CreateGame || Type == Types.JoinGame)
            {
                ((LoginPayload)Payload).Write(w);
            }
            else if (Type == Types.ContinueGame)
            {
                w.Write((bool)Payload);
            }
            else if (Type == Types.PlayerPosition)
            {
                ((PositionPayload)Payload).Write(w);
            }
            else if (Type == Types.PlayerBomb)
            {
                w.Write((int)Payload);
            }
            else if (Type == Types.PlayerDetonate)
            {
                w.Write((ushort)Payload);
            }
            else if (Type == Types.Kicked || Type == Types.DeniedGame)
            {
                w.Write((int)Payload);
            }
            else if (Type == Types.PlayerJoined)
            {
                ((PlayerPayload)Payload).Write(w);
            }
            else if (Type == Types.PlayerLeft)
            {
                w.Write((byte)Payload);
            }
            else if (Type == Types.PlayerState)
            {
                ((PlayerPayload)Payload).Write(w);
            }
            else if (Type == Types.ChangePhase)
            {
                w.Write((int)Payload);
            }
            else if (Type == Types.SetMap)
            {
                ((SetMapPayload)Payload).Write(w);
            }
            else if (Type == Types.SetTimeLimit)
            {
                w.Write(((TimeSpan)Payload).Ticks);
            }
            else if (Type == Types.MapEvent)
            {
                ((MapEvent)Payload).Write(w);
            }
            else if (Type == Types.Chat)
            {
                w.Write((string)Payload);
            }
            else if (Type == Types.Options)
            {
                ((OptionsPayload)Payload).Write(w);
            }
            else if (Type == Types.PauseGame)
            {
                w.Write((bool)Payload);
            }
        }

        public MessageEvent(Types type, System.Object payload)
        {
            Type = type;
            Payload = payload;
        }

        public MessageEvent() : this(Types.Chat, "") { }
    }
}
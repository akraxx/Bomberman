using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents the status of a game.
    /// </summary>
    public class Status
    {
        /// <summary>
        /// The available game modes.
        /// </summary>
        public enum Modes
        {
            /// <summary>
            /// Cooperation: the vs. monsters mode.
            /// </summary>
            Cooperation,

            /// <summary>
            /// Versus: the vs. players mode.
            /// </summary>
            Versus,
        }

        /// <summary>
        /// The available game phases.
        /// </summary>
        public enum Phases
        {
            /// <summary>
            /// Game is in startup phase in the lobby.
            /// </summary>
            Lobby,

            /// <summary>
            /// Game is loading a map.
            /// </summary>
            Load,

            /// <summary>
            /// Game is about to start.
            /// </summary>
            Ready,

            /// <summary>
            /// Game is playing.
            /// </summary>
            Ingame,

            /// <summary>
            /// Showing a continue for at least one player.
            /// </summary>
            Continue,

            /// <summary>
            /// Showing game cleared.
            /// </summary>
            Cleared,

            /// <summary>
            /// Showing game over.
            /// </summary>
            GameOver,

            /// <summary>
            /// Server-side, game should be removed from server list when possible. Client-side, the player should return to title screen.
            /// </summary>
            Ended,
        };

        /// <summary>
        /// The current mode of the game.
        /// </summary>
        public Modes Mode { get; protected set; }

        /// <summary>
        /// The current phase of the game.
        /// </summary>
        public Phases Phase { get; protected set; }

        /// <summary>
        /// True if the game is paused.
        /// TODO: non implémenté (piste d'amélioration pour plus tard)
        /// </summary>
        public bool Paused { get; protected set; }

        /// <summary>
        /// The current round of the game.
        /// </summary>
        public int Round { get; protected set; }

        /// <summary>
        /// The time limit for the current round.
        /// </summary>
        public TimeSpan TimeLimit { get; set; }

        /// <summary>
        /// True if players are allowed to continue upon losing all their lives.
        /// </summary>
        public bool Continuable { get; set; }

        /// <summary>
        /// The number of wins required to win, in the case of a vs. mode.
        /// Unused in cooperation mode.
        /// </summary>
        public int WinsThreshold { get; set; }

        /// <summary>
        /// The maximum number of supported players.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The list of players currently in the game.
        /// </summary>
        public Player[] Players { get; protected set; }

        /// <summary>
        /// The number of players still playing the game.
        /// </summary>
        public int ActivePlayers { get { return Players.Count(p => p.Playing); } }

        /// <summary>
        /// Get the local player. Null if there is none.
        /// </summary>
        public Player LocalPlayer { get { return Players.FirstOrDefault(p => p.Local); } }

        /// <summary>
        /// Get the host player. Null if there is none.
        /// </summary>
        public Player HostPlayer { get { return Players.FirstOrDefault(p => p.Host); } }

        /// <summary>
        /// Return the next unused player (the first non-playing player).
        /// </summary>
        public Player NextUnusedPlayer { get { return Players.FirstOrDefault(p => !p.Playing); } }

        /// <summary>
        /// Returns the winner, in case of a versus game. Null if there is no winner yet or if this is not a versus game.
        /// </summary>
        public Player Winner { get { return Mode == Modes.Versus ? Players.FirstOrDefault(p => p.Wins >= WinsThreshold) : null; } }

        /// <summary>
        /// Return true if the game can be started.
        /// </summary>
        public bool Startable { get { return Phase == Phases.Lobby && ((Mode == Modes.Cooperation && ActivePlayers == 1) || ActivePlayers > 1); } }

        /// <summary>
        /// Return true if extra players are allowed to join the game.
        /// </summary>
        public bool Joinable { get { return Phase == Phases.Lobby || (Continuable && (Phase == Phases.Load || Phase == Phases.Ready || Phase == Phases.Ingame || Phase == Phases.Continue)); } }

        /// <summary>
        /// Reset the status.
        /// </summary>
        public void Reset()
        {
            Mode = Modes.Cooperation;
            Phase = Phases.Lobby;
            Paused = false;
            Round = 0;
            TimeLimit = TimeSpan.Zero;
            Continuable = true;
            WinsThreshold = 3;
            for (byte i = 0; i < MaxPlayers; i++)
            {
                Players[i].Leave();
            }
        }

        /// <summary>
        /// Change the current mode of the game. Can only be done in lobby phase.
        /// </summary>
        public void SetMode(Modes m)
        {
            if (Mode != m)
            {
                if (Phase == Phases.Lobby)
                {
                    Mode = m;
                }
                else
                {
                    throw new InvalidOperationException("Can't change mode outside of lobby phase");
                }
            }
        }

        /// <summary>
        /// Change the current phase of the game. Invalid operation if the game is already ended.
        /// </summary>
        public void SetPhase(Phases p)
        {
            if (Phase != p)
            {
                if (Phase != Phases.Ended)
                {
                    Phase = p;
                    if (PhaseChanged != null) PhaseChanged(this, new EventArgs());
                }
                else
                {
                    throw new InvalidOperationException("Can't change phase of a game already ended");
                }
            }
        }

        /// <summary>
        /// Change the current round of the game. Can only be done during loading phase.
        /// </summary>
        public void SetRound(int r)
        {
            if (Round != r)
            {
                if (Phase == Phases.Load)
                {
                    Round = r;
                }
                else
                {
                    throw new InvalidOperationException("Can't change round outside of loading phase");
                }
            }
        }

        /// <summary>
        /// Change the pause state of the game.
        /// </summary>
        public void SetPause(bool p)
        {
            if (Paused != p)
            {
                Paused = p;
                if (PauseChanged != null) PauseChanged(this, new EventArgs());
            }
        }

        public Status()
        {
            Players = new Player[MaxPlayers];
            for (byte i = 0; i < MaxPlayers; i++)
            {
                Players[i] = new Player(i);
            }
            this.Reset();
        }

        // Events

        /// <summary>
        /// Fired when pause state is changed.
        /// </summary>
        public event EventHandler<EventArgs> PauseChanged;

        /// <summary>
        /// Fired when current phase is changed.
        /// </summary>
        public event EventHandler<EventArgs> PhaseChanged;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Model
{
    /// <summary>
    /// The model of a game of Bomberman.
    /// </summary>
    public sealed class Game
    {
        /// <summary>
        /// The current map for the game.
        /// Should be rendered only when the current game phase is "ingame".
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        /// The status information for the game.
        /// It contains in particular the current game phase.
        /// </summary>
        public Status Status { get; private set; }

        /// <summary>
        /// The current level of the game.
        /// The level contains the initial state of the map as well as
        /// some other parameters such as time limit and the level number.
        /// </summary>
        public Level Level { get; private set; }

        /// <summary>
        /// Reset the game to initial state.
        /// </summary>
        public void Reset()
        {
            Map.RemoveAll();
            Status.Reset();
            Level = null;
        }

        /// <summary>
        /// Load a level in the game.
        /// The game is put in "Load" phase.
        /// </summary>
        public void LoadLevel(Level level)
        {
            if (level != null)
            {
                Level = level;

                Status.SetPhase(Status.Phases.Load);
                Status.SetRound(Level.Number);
                Status.TimeLimit = Level.TimeLimit;

                Map.Transfer(level.Map);
            }
            else
            {
                throw new ArgumentNullException("level");
            }
        }

        public Game()
        {
            Map = new Map(0, 0, 0);
            Status = new Status();
            Level = null;
        }
    }
}
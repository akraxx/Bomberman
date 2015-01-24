using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a game level.
    /// </summary>
    public sealed class Level
    {
        /// <summary>
        /// Number of the level.
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Map for the level. Shouldn't be modified, should be copied.
        /// </summary>
        public Map Map { get; private set; }

        /// <summary>
        /// Time limit for the level.
        /// </summary>
        public TimeSpan TimeLimit { get; private set; }

        public Level(int number, Map map, TimeSpan timeLimit)
        {
            if (map != null)
            {
                Number = number;
                Map = map;
                TimeLimit = timeLimit;
            }
            else
            {
                throw new ArgumentNullException("map");
            }
        }

        /// <summary>
        /// Create an empty level, suitable as placeholder client-side data
        /// while waiting for map content from the server.
        /// </summary>
        public static Level GetEmptyLevel(int number, int width, int height, int theme)
        {
            return new Level(number, new Map(width, height, theme), TimeSpan.Zero);
        }

        /// <summary>
        /// Get a predefined level of the specified mode. Return null if level doesn't exist.
        /// </summary>
        public static Level GetPredefinedLevel(Status.Modes mode, int number)
        {
            Map map = MapFactory.Build(mode, number);
            return map != null ? new Level(number, map, TimeSpan.FromSeconds(120.9)) : null;
        }
    }
}
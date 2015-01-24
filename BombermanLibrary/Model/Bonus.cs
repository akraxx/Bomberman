using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    public class Bonus : TimedObject
    {
        public static readonly TimeSpan DefaultTimer = TimeSpan.FromSeconds(15.0);
        public static readonly TimeSpan ShortTimer = TimeSpan.FromSeconds(9.0);

        private static int[] valueBySpriteIndex = { 300, 200, 300, 500, 1000 };

        /// <summary>
        /// The sprite that should be displayed to render this bonus.
        /// </summary>
        public int SpriteIndex { get; protected set; }

        /// <summary>
        /// Value in points.
        /// </summary>
        public int Value { get; protected set; }

        public Bonus(int spriteIndex, TimeSpan timer, Point position) : base(timer, position)
        {
            SpriteIndex = spriteIndex;
            Value = valueBySpriteIndex[spriteIndex];
        }

        /// <summary>
        /// Get a random sprite of bonus.
        /// Bigger bonuses have a lower chance of being chosen.
        /// </summary>
        public static int GetRandomSprite(double roll)
        {
            if (roll < 0.40)
                return 1;

            else if (roll < 0.70)
                return 2;

            else if (roll < 0.85)
                return 3;

            else
                return 4;
        }

        /// <summary>
        /// Get a random sprite of bonus.
        /// Bigger bonuses have a lower chance of being chosen.
        /// </summary>
        public static int GetRandomSprite(Random r)
        {
            return GetRandomSprite(r.NextDouble());
        }
    }
}
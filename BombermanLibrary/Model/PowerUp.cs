using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    public class PowerUp : TimedObject
    {
        public static readonly TimeSpan DefaultTimer = TimeSpan.FromSeconds(30.0);
        public static readonly TimeSpan ShortTimer = TimeSpan.FromSeconds(15.0);

        /// <summary>
        /// The different types of powerups.
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// Grants an extra bomb.
            /// </summary>
            Bomb,

            /// <summary>
            /// Grants the maximum number of bombs.
            /// </summary>
            FullBomb,

            /// <summary>
            /// Grants one unit of bomb power.
            /// </summary>
            Power,

            /// <summary>
            /// Grants maximum bomb power.
            /// </summary>
            FullPower,

            /// <summary>
            /// Grants remote bombs.
            /// </summary>
            RemoteBomb,

            /// <summary>
            /// Grants split bombs.
            /// </summary>
            SplitBomb,

            /// <summary>
            /// Grants one unit of speed.
            /// </summary>
            Speed,

            /// <summary>
            /// Grants full speed.
            /// </summary>
            FullSpeed,

            /// <summary>
            /// Grants an extra life.
            /// </summary>
            ExtraLife,

            /// <summary>
            /// Grants short invulnerability.
            /// </summary>
            Invulnerability,

            /// <summary>
            /// Reduces bombs and power to minimum.
            /// </summary>
            Weaken,

            /// <summary>
            /// Stops monsters for a while (should never be given in PvP)
            /// TODO: non implémenté (piste d'amélioration pour plus tard)
            /// </summary>
            TimeStop,
        }

        /// <summary>
        /// The action of the powerup.
        /// </summary>
        public Types Type { get; protected set; }

        public PowerUp(Types type, TimeSpan timer, Point position) : base(timer, position)
        {
            Type = type;
        }

        /// <summary>
        /// Get a random type of powerup.
        /// More powerful powerups have a lower chance of being chosen.
        /// </summary>
        public static Types GetRandomType(double roll)
        {
            if (roll < 0.20)
                return Types.Bomb;

            else if (roll < 0.40)
                return Types.Power;

            else if (roll < 0.55)
                return Types.Speed;

            else if (roll < 0.62)
                return Types.FullBomb;

            else if (roll < 0.69)
                return Types.FullPower;

            else if (roll < 0.76)
                return Types.RemoteBomb;

            else if (roll < 0.81)
                return Types.SplitBomb;

            else if (roll < 0.86)
                return Types.FullSpeed;

            else if (roll < 0.89)
                return Types.ExtraLife;

            else if (roll < 0.94)
                return Types.Invulnerability;

            else
                return Types.Weaken;
        }

        /// <summary>
        /// Get a random type of powerup.
        /// More powerful powerups have a lower chance of being chosen.
        /// </summary>
        public static Types GetRandomType(Random r)
        {
            return GetRandomType(r.NextDouble());
        }
    }
}
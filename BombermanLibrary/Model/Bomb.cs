using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a bomb object.
    /// </summary>
    public class Bomb : TimedObject
    {
        public static readonly TimeSpan DefaultTimer = TimeSpan.FromSeconds(3.0);

        /// <summary>
        /// Number of bomb types.
        /// </summary>
        public const int NumTypes = 3;

        /// <summary>
        /// The different types of bombs.
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// The regular bomb. Explodes after a while.
            /// </summary>
            Normal,

            /// <summary>
            /// A dangerous bomb whose blast is able to split into corners.
            /// </summary>
            Split,

            /// <summary>
            /// A bomb that is fired on demand.
            /// </summary>
            Remote,
        }

        /// <summary>
        /// The action of the bomb.
        /// </summary>
        public Types Type { get; protected set; }

        /// <summary>
        /// The ID of the entity that owns this bomb.
        /// </summary>
        public ushort Owner { get; protected set; }

        /// <summary>
        /// The power of the bomb.
        /// </summary>
        public int Power { get; protected set; }

        public Bomb(Types type, ushort owner, int power, TimeSpan timer, Point position) : base(timer, position)
        {
            Type = type;
            Owner = owner;
            Power = power;
        }
    }
}

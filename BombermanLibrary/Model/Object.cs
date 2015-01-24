using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// The base class of all game objects.
    /// </summary>
    public abstract class Object
    {
        /// <summary>
        /// The unique ID given to this object.
        /// The unicity is used to uniquely identify this object in network communications.
        /// </summary>
        public ushort ID { get { return (ushort)(Position.X + Position.Y * Map.MaxWidth); } } // The one object per tile constraint guarantees the unicity.

        /// <summary>
        /// Position of the object (given in tiles).
        /// </summary>
        public Point Position { get; private set; }

        /// <summary>
        /// Creates a copy of the object.
        /// </summary>
        public virtual Object Copy() { return (Object)this.MemberwiseClone(); }

        public Object(Point position)
        {
            Position = position;
        }
    }
}
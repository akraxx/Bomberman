using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a wall object.
    /// </summary>
    public class Wall : TimedObject
    {
        public static readonly TimeSpan DefaultTimer = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// Return true if the wall can be destroyed.
        /// </summary>
        public bool Destructible { get; protected set; }

        /// <summary>
        /// Return true if the wall is being destroyed.
        /// </summary>
        public bool Destroying { get; protected set; }

        /// <summary>
        /// The item that is spawned when the wall is destroyed.
        /// </summary>
        public Object HiddenItem { get; protected set; }

        /// <summary>
        /// Start the destruction of the wall over the specified duration.
        /// This method can be called on normally indestructible walls.
        /// </summary>
        public void Destroy(TimeSpan timer)
        {
            if (!Destroying)
            {
                Timer = timer;
                MaxTimer = timer;
                Destroying = true;
            }
        }

        /// <summary>
        /// Creates a copy of the wall.
        /// The hidden item contained in the wall is copied as well.
        /// </summary>
        public override Object Copy()
        {
            Wall wall = (Wall)base.Copy();
            if (wall.HiddenItem != null) wall.HiddenItem = wall.HiddenItem.Copy();
            return wall;
        }

        public Wall(bool destructible, Object hiddenItem, Point position) : base(TimeSpan.FromTicks(1), position)
        {
            Destructible = destructible;
            Destroying = false;
            HiddenItem = hiddenItem;
        }
    }
}
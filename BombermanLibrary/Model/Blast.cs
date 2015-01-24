using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a blast object.
    /// </summary>
    public class Blast : TimedObject
    {
        public static readonly TimeSpan DefaultTimer = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// The different varieties of blasts.
        /// </summary>
        public enum Types
        {
            Multiple,
            Single,
            Vertical,
            Horizontal,
            Edge,
        }

        /// <summary>
        /// The action of the blast.
        /// </summary>
        public Types Type { get; protected set; }

        /// <summary>
        /// The orientation of the blast. Generally only used for Edge-action blasts.
        /// </summary>
        public Orientations Orientation { get; protected set; }

        /// <summary>
        /// The ID of the entity that owns this blast.
        /// </summary>
        public ushort Owner { get; protected set; }

        public Blast(Types type, Orientations orientation, ushort owner, TimeSpan timer, Point position) : base(timer, position)
        {
            Type = type;
            Orientation = orientation;
            Owner = owner;
        }
    }
}
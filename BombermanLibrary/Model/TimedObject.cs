using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// The base class of all game objects working on a timer.
    /// </summary>
    public abstract class TimedObject : Object
    {
        /// <summary>
        /// The timer of the object.
        /// </summary>
        public TimeSpan Timer { get; set; }

        /// <summary>
        /// The initial timer of the object.
        /// </summary>
        public TimeSpan MaxTimer { get; protected set; }

        /// <summary>
        /// The relative progression of the timer against its maximum, given between 0 and 1.
        /// </summary>
        public float Progression { get { return (float)Math.Max(0.0, Math.Min(1.0, Timer.TotalSeconds / MaxTimer.TotalSeconds)); } }

        public TimedObject(TimeSpan timer, Point position) : base(position)
        {
            if (timer > TimeSpan.Zero)
            {
                Timer = timer;
                MaxTimer = timer;
            }
            else
            {
                throw new ArgumentOutOfRangeException("timer");
            }
        }
    }
}
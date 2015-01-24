using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Model
{
    /// <summary>
    /// Contains infos used to determinate sprite state.
    /// </summary>
    public struct SpriteState
    {
        /// <summary>
        /// The action currently displayed by the sprite.
        /// </summary>
        public Actions Action { get; private set; }

        /// <summary>
        /// The frame counter of the sprite.
        /// </summary>
        public int Frame { get; private set; }

        /// <summary>
        /// The display orientation of the sprite.
        /// </summary>
        public Orientations Orientation { get; set; }

        /// <summary>
        /// Change the action performed by the sprite.
        /// </summary>
        public void Begin(Actions a)
        {
            if (Action != a)
            {
                Action = a;
                Frame = 0;
            }
        }

        /// <summary>
        /// Advance frame counter.
        /// </summary>
        public void Tick()
        {
            Frame++;
        }
    }
}
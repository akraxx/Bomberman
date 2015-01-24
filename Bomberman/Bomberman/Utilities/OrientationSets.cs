using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Built-in orientation sets.
    /// </summary>
    public enum OrientationSets
    {
        /// <summary>
        /// In this set, only the bottom graphic is available.
        /// </summary>
        BottomOnly,

        /// <summary>
        /// In this set, graphics for all orientations are available.
        /// </summary>
        Full,

        /// <summary>
        /// In this set, the right graphic is obtained by an horizontal flip.
        /// </summary>
        FullFlipHorizontal,

        /// <summary>
        /// In this set, the top and right graphics are obtained by flips.
        /// </summary>
        FullFlipBoth,
    }
}
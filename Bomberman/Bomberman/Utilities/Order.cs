using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Defines some common draw orders for the various graphical elements of the game.
    /// Child elements should use sub units (example: ForegroundLevel + 1).
    /// </summary>
    static class Order
    {
        public const int ForegroundLevel = 500;
        public const int PopupLevel      = 400;
        public const int OverlayLevel    = 300;
        public const int SpriteLevel     = 200;
        public const int StaticLevel     = 100;
        public const int BackgroundLevel = 000;
    }
}
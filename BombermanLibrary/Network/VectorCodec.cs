using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Network
{
    /// <summary>
    /// Gives tools to transmit acceptable Vector2 over the network as int[2].
    /// </summary>
    public static class VectorCodec
    {
        private static float factor = 10000f;

        public static int[] Encode(Vector2 v)
        {
            return new int[2] { (int)(v.X * factor), (int)(v.Y * factor) };
        }

        public static Vector2 Decode(int[] i)
        {
            System.Diagnostics.Debug.Assert(i.Length == 2);
            return new Vector2(((float)i[0]) / factor, ((float)i[1]) / factor);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// The standard orientations.
    /// </summary>
    public enum Orientations
    {
        Bottom,
        Top,
        Left,
        Right
    }

    /// <summary>
    /// Provide tools to work with Orientations enum.
    /// </summary>
    public static class Orientation
    {
        private static readonly Point[] offset = { new Point(0, 1), new Point(0, -1), new Point(-1, 0), new Point(1, 0) };
        private static readonly int[] opposite = { 1, 0, 3, 2 };

        /// <summary>
        /// Number of orientations.
        /// </summary>
        public const int NumOrientations = 4;

        /// <summary>
        /// Get an orientation by index.
        /// </summary>
        public static Orientations ByIndex(int i)
        {
            Debug.Assert(i >= 0 && i < NumOrientations);
            return (Orientations)i;
        }

        /// <summary>
        /// Get the offset associated with an orientation.
        /// </summary>
        public static Point GetOffset(Orientations o)
        {
            return offset[(int)o];
        }

        /// <summary>
        /// Check if an orientation is horizontal.
        /// </summary>
        public static bool IsHorizontal(Orientations o)
        {
            return o == Orientations.Left || o == Orientations.Right;
        }

        /// <summary>
        /// Check if an orientation is vertical.
        /// </summary>
        public static bool IsVertical(Orientations o)
        {
            return o == Orientations.Bottom || o == Orientations.Top;
        }

        /// <summary>
        /// Get the orientation that is opposite to the provided orientation.
        /// </summary>
        public static Orientations OppositeOf(Orientations o)
        {
            return (Orientations)opposite[(int)o];
        }

        /// <summary>
        /// Get the orientation of the provided vector.
        /// </summary>
        public static Orientations OrientationOf(Vector2 delta)
        {
            if (delta.Y >= Math.Abs(delta.X))
            {
                return Orientations.Bottom;
            }
            else if (delta.Y <= -Math.Abs(delta.X))
            {
                return Orientations.Top;
            }
            if (delta.X <= -Math.Abs(delta.Y))
            {
                return Orientations.Left;
            }
            else
            {
                return Orientations.Right;
            }
        }

        /// <summary>
        /// Compute the distance between two angles.
        /// </summary>
        public static float GetAngleDistance(float a, float b)
        {
            float d1 = (float)Math.Abs((MathHelper.TwoPi - a + b) % MathHelper.TwoPi);
            float d2 = (float)Math.Abs((MathHelper.TwoPi + a - b) % MathHelper.TwoPi);
            return MathHelper.Min(d1, d2);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    public static class Tools
    {
        public static Point Vector2Point(Vector2 v)
        {
            return new Point((int)v.X, (int)v.Y);
        }

        public static Vector2 Point2Vector(Point p)
        {
            return new Vector2(p.X, p.Y);
        }
        public static Point ToPoint(this Vector2 v)
        {
            return new Point((int)v.X, (int)v.Y);
        }

        public static Vector2 ToVector(this Point p)
        {
            return new Vector2(p.X, p.Y);
        }

        public static int Manhattan(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
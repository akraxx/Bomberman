using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// This class knows how to flag spots of the map hit by blasts from bombs.
    /// It can also tells which bomberman started the blast, if there is one.
    /// </summary>
    public sealed class BlastMapper
    {
        private Map map;

        private bool[,] hit;

        private struct Fragment
        {
            public int Power;
            public Orientations Orientation;
            public Point Point;
            public bool Split;

            public Fragment(int power, Orientations orientation, Point point, bool split)
            {
                this.Power = power;
                this.Orientation = orientation;
                this.Point = point;
                this.Split = split;
            }
        }

        private Queue<Fragment> fragments = new Queue<Fragment>(64);

        private List<Orientations> neighbors = new List<Orientations>(Orientation.NumOrientations);

        private void _processQueue()
        {
            while (fragments.Count > 0)
            {
                Fragment f = fragments.Dequeue();
                if (map.InBoundaries(f.Point))
                {
                    hit[f.Point.Y, f.Point.X] = true;
                    if (f.Power > 0)
                    {
                        Point offset = Orientation.GetOffset(f.Orientation);
                        Point next = new Point(f.Point.X + offset.X, f.Point.Y + offset.Y);

                        Model.Object obj = map.GetObject(next);

                        if (!map.InBoundaries(next) || (obj != null && obj is Wall))
                        {
                            // Would hit a wall next, the fragment needs to split or die.
                            if (f.Split)
                            {
                                if (Orientation.IsHorizontal(f.Orientation))
                                {
                                    fragments.Enqueue(new Fragment(f.Power - 1, Orientations.Top, new Point(f.Point.X, f.Point.Y - 1), f.Split));
                                    fragments.Enqueue(new Fragment(f.Power - 1, Orientations.Bottom, new Point(f.Point.X, f.Point.Y + 1), f.Split));
                                }
                                else if (Orientation.IsVertical(f.Orientation))
                                {
                                    fragments.Enqueue(new Fragment(f.Power - 1, Orientations.Left, new Point(f.Point.X - 1, f.Point.Y), f.Split));
                                    fragments.Enqueue(new Fragment(f.Power - 1, Orientations.Right, new Point(f.Point.X + 1, f.Point.Y), f.Split));
                                }
                            }
                            fragments.Enqueue(new Fragment(0, f.Orientation, next, f.Split));
                        }
                        else
                        {
                            // Continue the propagation.
                            fragments.Enqueue(new Fragment(f.Power - 1, f.Orientation, next, f.Split));
                        }
                    }
                }
            }
        }

        private void _syncArray()
        {
            if (hit == null || hit.GetLength(0) != map.Height || hit.GetLength(1) != map.Width)
            {
                hit = new bool[map.Height, map.Width];
            }
        }

        /// <summary>
        /// True if the blast mapper is started.
        /// </summary>
        public bool Started { get; private set; }

        /// <summary>
        /// The bomberman owning the blast being computed.
        /// Null if the blast was started by a monster or the environment.
        /// </summary>
        public Bomberman Owner { get; private set; }

        /// <summary>
        /// Check if the specified spot of the map is hit by the blast.
        /// </summary>
        public bool IsHit(Point p)
        {
            _syncArray();
            return map.InBoundaries(p) && hit[p.Y, p.X];
        }

        /// <summary>
        /// Erase the blast mapper.
        /// This will cancel the Start method, allowing it to be called again.
        /// </summary>
        public void Clear()
        {
            Started = false;
            _syncArray();
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    hit[y, x] = false;
                }
            }
            Owner = null;
        }

        /// <summary>
        /// Nuke the whole map (except walls).
        /// Calling this will implicitly start the blast mapper.
        /// </summary>
        public void Nuke()
        {
            if (!Started)
            {
                Started = true;
                Owner = null;
                _syncArray();
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        Model.Object o = map.GetObject(new Point(x, y));
                        if (!(o != null && o is Wall))
                        {
                            hit[y, x] = true;
                        }
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Already began computing the blast");
            }
        }

        /// <summary>
        /// Start computing the blast area in the blast mapper.
        /// Will throw an illegal operation exception if the blast mapper is already started.
        /// </summary>
        public void Start(Bomb b)
        {
            if (!Started)
            {
                Started = true;
                Owner = map.Bombermen.FirstOrDefault(bomberman => bomberman.ID == b.Owner);
                this.Add(b);
            }
            else
            {
                throw new InvalidOperationException("Already began computing the blast");
            }
        }

        /// <summary>
        /// Add another bomb in the blast mapper.
        /// Will throw an illegal operation exception if Start wasn't called beforehand.
        /// </summary>
        public void Add(Bomb b)
        {
            if (Started)
            {
                if (map.InBoundaries(b.Position))
                {
                    if (b.Power > 0)
                    {
                        _syncArray();
                        for (int i = 0; i < Orientation.NumOrientations; i++)
                        {
                            fragments.Enqueue(new Fragment(b.Power - 1, Orientation.ByIndex(i), b.Position, b.Type == Bomb.Types.Split));
                        }
                        _processQueue();
                    }
                }
                else
                {
                    throw new InvalidOperationException("Bomb is out of map boundaries");
                }
            }
            else
            {
                throw new InvalidOperationException("BlastMapper is not started");
            }
        }

        /// <summary>
        /// Create a blast for the specified position, with the specified despawn timer.
        /// </summary>
        public Blast MakeBlast(TimeSpan timer, Point p)
        {
            neighbors.Clear();
            Blast.Types type = Blast.Types.Multiple;
            Orientations orientation = Orientations.Bottom;
            for (int i = 0; i < Orientation.NumOrientations; i++)
            {
                Orientations o = Orientation.ByIndex(i);
                Point offset = Orientation.GetOffset(o);
                Point neighbor = new Point(p.X + offset.X, p.Y + offset.Y);
                if (this.IsHit(neighbor))
                {
                    neighbors.Add(o);
                }
            }
            if (neighbors.Count == 2)
            {
                if (Orientation.IsHorizontal(neighbors[0]) && Orientation.IsHorizontal(neighbors[1]))
                {
                    type = Blast.Types.Horizontal;
                }
                else if (Orientation.IsVertical(neighbors[0]) && Orientation.IsVertical(neighbors[1]))
                {
                    type = Blast.Types.Vertical;
                }
            }
            else if (neighbors.Count == 1)
            {
                type = Blast.Types.Edge;
                orientation = Orientation.OppositeOf(neighbors[0]);
            }
            else if (neighbors.Count == 0)
            {
                type = Blast.Types.Single;
            }
            return new Blast(type, orientation, Owner != null ? Owner.ID : (ushort)0, timer, p);
        }

        public BlastMapper(Map map)
        {
            if (map != null)
            {
                this.map = map;

                _syncArray();
            }
            else
            {
                throw new ArgumentNullException("map");
            }
        }
    }
}
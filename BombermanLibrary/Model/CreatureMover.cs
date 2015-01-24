using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// A class that knows how to move a creature within the map and detects its environment.
    /// </summary>
    public sealed class CreatureMover
    {
        public const float BaseSpeed = 3.0f; // The base speed of all creatures, before applying factors.
        public const float AlignThreshold = 0.2f; // The threshold for finding the next integer position.
        public const float StopThreshold = 0.01f; // The length of movement vector under which the movement is completed.
        public const float DummyThreshold = 0.001f; // A length of movement vector you can use to give an orientation but prevent movement.

        private Map map;
        private Creature creature;
        private Vector2 initialPosition;
        private List<Model.Object> initialObjectCollisions = new List<Model.Object>(4);
        private List<Model.Object> objectCollisions = new List<Model.Object>(4);
        private List<Orientations> availableOrientations = new List<Orientations>(Orientation.NumOrientations);

        /// <summary>
        /// True if the creature is in authorized spot.
        /// This gets set to false if a creature is forced on an impassable position.
        /// </summary>
        public bool Legal { get; private set; }

        /// <summary>
        /// True if the creature has changed tile. Only valid to check after doing a MoveBy.
        /// </summary>
        public bool ChangedTile { get; private set; }

        /// <summary>
        /// Read-only list of objects the creature has collided with.
        /// </summary>
        public IList<Model.Object> ObjectCollisions { get { return objectCollisions.AsReadOnly(); } }

        /// <summary>
        /// Read-only list of orientations available to the creature.
        /// </summary>
        public IList<Orientations> AvailableOrientations { get { return availableOrientations.AsReadOnly(); } }

        private bool _collectObjectCollisions(List<Model.Object> list, Vector2 position)
        {
            list.Clear();
            bool outOfMap = false;
            int minX = (int)Math.Floor(position.X);
            int maxX = (int)Math.Ceiling(position.X);
            int minY = (int)Math.Floor(position.Y);
            int maxY = (int)Math.Ceiling(position.Y);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Point p = new Point(x, y);
                    if (map.InBoundaries(p))
                    {
                        Object obj = map.GetObject(p);
                        if (obj != null)
                        {
                            list.Add(obj);
                        }
                    }
                    else
                    {
                        outOfMap = true;
                    }
                }
            }
            return outOfMap;
        }

        private float _getAxisMovement(float current, float delta)
        {
            float nextTileMovement = AlignAxis(current + delta, true) - current;
            if (delta > 0)
            {
                if (nextTileMovement > 0 && delta > nextTileMovement)
                {
                    ChangedTile = true;
                    return nextTileMovement;
                }
                else
                {
                    return delta;
                }
            }
            else if (delta < 0)
            {
                if (nextTileMovement < 0 && delta < nextTileMovement)
                {
                    ChangedTile = true;
                    return nextTileMovement;
                }
                else
                {
                    return delta;
                }
            }
            return 0;
        }

        private void _updateState()
        {
            objectCollisions.Clear();
            availableOrientations.Clear();

            Vector2 pos = creature.Position;

            // Object collisions
            bool outOfMap = _collectObjectCollisions(objectCollisions, pos);

            // Available orientations
            for (int i = 0; i < Orientation.NumOrientations; i++)
            {
                Orientations o = Orientation.ByIndex(i);
                Point offset = Orientation.GetOffset(o);
                Point p = Tools.Vector2Point(AlignPosition(pos, true) + Tools.Point2Vector(offset));
                if (IsPathable(p)) availableOrientations.Add(o);
            }

            // Legal flag
            Legal = !objectCollisions.Any(o => (o is Bomb || o is Wall) && !initialObjectCollisions.Contains(o)) && !outOfMap;
        }

        /// <summary>
        /// Return true if the provided point is pathable.
        /// </summary>
        public bool IsPathable(Point p)
        {
            if (!map.InBoundaries(p)) return false;
            Object obj = map.GetObject(p);
            if (obj != null && (obj is Wall || obj is Bomb) && !initialObjectCollisions.Contains(obj)) return false;
            return true;
        }

        /// <summary>
        /// Set the active creature in the creature mover.
        /// Calling this will reset the initial position of the movement.
        /// </summary>
        public void SetActive(Creature creature)
        {
            if (creature != null)
            {
                this.creature = creature;
                initialPosition = creature.Position;
                _collectObjectCollisions(initialObjectCollisions, initialPosition);
                _updateState();
            }
            else
            {
                throw new ArgumentNullException("creature");
            }
        }

        /// <summary>
        /// Move the active creature to the target destination.
        /// </summary>
        public void MoveTo(Vector2 destination)
        {
            if (creature != null)
            {
                creature.Position = destination;
                _updateState();
            }
            else
            {
                throw new InvalidOperationException("No active creature set");
            }
        }

        /// <summary>
        /// Move the active creature by the specified delta, taking in account
        /// collision with walls and adjusting the position as needed.
        /// </summary>
        public void MoveBy(Vector2 delta)
        {
            if (creature != null)
            {
                ChangedTile = false;
                Vector2 initial = creature.Position;
                int dimension = Math.Abs(delta.X) > Math.Abs(delta.Y) ? 0 : 1;
                while (delta.Length() > StopThreshold)
                {
                    if (dimension == 0)
                    {
                        float movement = _getAxisMovement(creature.Position.X, delta.X);
                        delta.X -= movement;
                        creature.Position += new Vector2(movement, 0);
                        _updateState();

                        if (!Legal)
                        {
                            creature.Position = new Vector2(creature.Position.X, AlignAxis(creature.Position.Y, false));
                            _updateState();
                        }
                        if (!Legal)
                        {
                            creature.Position = initial;
                            _updateState();
                        }
                        if(Legal)
                        {
                            initial = creature.Position;
                        }
                    }
                    else if (dimension == 1)
                    {
                        float movement = _getAxisMovement(creature.Position.Y, delta.Y);
                        delta.Y -= movement;
                        creature.Position += new Vector2(0, movement);
                        _updateState();

                        if (!Legal)
                        {
                            creature.Position = new Vector2(AlignAxis(creature.Position.X, false), creature.Position.Y);
                            _updateState();
                        }
                        if (!Legal)
                        {
                            creature.Position = initial;
                            _updateState();
                        }
                        if(Legal)
                        {
                            initial = creature.Position;
                        }
                    }
                    dimension = (dimension + 1) % 2;
                }
            }
            else
            {
                throw new InvalidOperationException("No active creature set");
            }
        }

        /// <summary>
        /// Move the creature back to its initial position.
        /// </summary>
        public void Rollback()
        {
            if (creature != null)
            {
                creature.Position = initialPosition;
                _updateState();
            }
            else
            {
                throw new InvalidOperationException("No active creature set");
            }
        }

        public CreatureMover(Map map)
        {
            if (map != null)
            {
                this.map = map;
            }
            else
            {
                throw new ArgumentNullException("map");
            }
        }

        /// <summary>
        /// Align the specified position to the closest integer position.
        /// Will do nothing if no integer position is close the specified position, unless force is true.
        /// This uses Manhattan distance.
        /// </summary>
        public static Vector2 AlignPosition(Vector2 pos, bool force)
        {
            return new Vector2(AlignAxis(pos.X, force), AlignAxis(pos.Y, force));
        }

        /// <summary>
        /// Align the specified axis position to the closest integer position.
        /// </summary>
        public static float AlignAxis(float pos, bool force)
        {
            float alignPos = pos;
            float minDistance = force ? float.PositiveInfinity : AlignThreshold;
            int min = (int)Math.Floor(pos);
            int max = (int)Math.Ceiling(pos);
            for (int i = min; i <= max; i++)
            {
                float distance = Math.Abs(i - pos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    alignPos = i;
                }
            }
            return alignPos;
        }
    }
}
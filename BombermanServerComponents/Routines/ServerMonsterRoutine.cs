using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Server
{
    /// <summary>
    /// The base class for monster routines.
    /// Contains some services available to the derived classes for the artificial intelligence.
    /// </summary>
    public abstract class ServerMonsterRoutine
    {
        protected ServerController controller;
        protected Monster monster;
        protected CreatureMover mover;
        protected Random random;

        /// <summary>
        /// True if the monster has changed tile.
        /// </summary>
        protected bool ChangedTile { get; private set; }

        /// <summary>
        /// The current direction of the monster.
        /// </summary>
        protected Orientations Direction { get; private set; }

        /// <summary>
        /// Get the tile currently occupied by a creature.
        /// </summary>
        protected Point GetTile(Creature c)
        {
            return Tools.Vector2Point(CreatureMover.AlignPosition(c.Position, true));
        }

        /// <summary>
        /// True if the specified point is pathable.
        /// </summary>
        protected bool IsPathable(Point p)
        {
            return mover.IsPathable(p);
        }

        /// <summary>
        /// True if the target is a valid target.
        /// </summary>
        protected bool IsValidTarget(Creature c)
        {
            return c is Model.Bomberman && !c.Dead && !c.Invulnerable;
        }

        /// <summary>
        /// Set velocity and apply monster movement.
        /// </summary>
        protected void Move(TimeSpan elapsed)
        {
            mover.SetActive(monster);
            monster.Velocity = Tools.Point2Vector(Orientation.GetOffset(Direction)) * monster.Speed * CreatureMover.BaseSpeed;

            Vector2 movement = monster.Velocity * (float)elapsed.TotalSeconds;
            mover.MoveBy(movement);
            controller.SendCreaturePosition(monster);
            ChangedTile = mover.ChangedTile;
        }

        /// <summary>
        /// Stop the monster (set velocity to zero).
        /// </summary>
        protected void Stop(Actions action)
        {
            monster.Velocity = Vector2.Zero;
            controller.SendCreaturePosition(monster);

            this.StartAnimation(action);

            ChangedTile = false;
        }

        /// <summary>
        /// Begin a new movement in the specified orientation.
        /// </summary>
        protected void Go(Orientations orientation, Actions action)
        {
            Direction = orientation;

            mover.SetActive(monster);
            mover.MoveTo(Tools.Point2Vector(this.GetTile(monster)));

            this.StartAnimation(action);

            ChangedTile = false;
        }

        /// <summary>
        /// Pick a random orientation in a list of orientations.
        /// </summary>
        protected Orientations PickRandomOrientation(IList<Orientations> list)
        {
            if (list.Count > 0)
            {
                return list[random.Next(0, list.Count)];
            }
            else
            {
                return Direction;
            }
        }

        /// <summary>
        /// Try to pick an orientation that is not the opposte direction of the current direction of the monster.
        /// Will pick the opposite direction if no other direction is available and it is in the list.
        /// Return the current direction if the list is empty.
        /// </summary>
        protected Orientations PickForwardOrientation(IList<Orientations> list)
        {
            Orientations oppositeDirection = Orientation.OppositeOf(Direction);
            Orientations[] notOppositeList = list.Where(o => o != oppositeDirection).ToArray();

            if (notOppositeList.Length > 0)
            {
                return this.PickRandomOrientation(notOppositeList);
            }
            else if (list.Contains(oppositeDirection))
            {
                return oppositeDirection;
            }
            else
            {
                return Direction;
            }
        }

        /// <summary>
        /// Try to pick an orientation that is will give the shortest manhattan path to the specified goal point.
        /// If allowTurnOver is true, the monster will be unable to suddenly go the opposite direction.
        /// This flag is automatically turned off if there is only 1 available direction.
        /// </summary>
        protected Orientations PickShortestOrientation(IList<Orientations> list, Point start, Point goal, bool allowTurnOver)
        {
            Orientations opposite = Orientation.OppositeOf(Direction);
            Orientations shortest = Direction;
            int lowestDistance = int.MaxValue;
            if (list.Count == 1) allowTurnOver = true;
            for (int i = 0; i < list.Count; i++)
            {
                Orientations orientation = list[i];
                if(allowTurnOver || orientation != opposite) {
                    Point offset = Orientation.GetOffset(orientation);
                    Point p = new Point(start.X + offset.X, start.Y + offset.Y);
                    int distance = Tools.Manhattan(p, goal);
                    if (distance < lowestDistance || (distance == lowestDistance && random.Next(0, 2) == 0))
                    {
                        lowestDistance = distance;
                        shortest = orientation;
                    }
                }
            }
            return shortest;
        }

        /// <summary>
        /// Start a new animation for the monster's sprite.
        /// </summary>
        protected void StartAnimation(Actions action)
        {
            if (monster.SpriteState.Action != action)
            {
                monster.SpriteState.Begin(action);
                controller.SendCreatureAnimation(monster);
            }
        }

        /// <summary>
        /// Update the behavior of the monster.
        /// </summary>
        public abstract void Execute(TimeSpan elapsed);

        public ServerMonsterRoutine(ServerController controller, Monster monster)
        {
            if (monster != null)
            {
                this.controller = controller;
                this.monster = monster;
                mover = new CreatureMover(controller.Game.Map);
                mover.SetActive(monster);
                random = new Random((int)(DateTime.Today.TimeOfDay.Ticks + monster.ID));
                ChangedTile = false;
                Direction = Orientations.Bottom;
            }
            else
            {
                throw new ArgumentNullException("monster");
            }
        }
    }
}
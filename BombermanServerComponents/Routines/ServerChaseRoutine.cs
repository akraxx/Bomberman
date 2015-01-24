using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Server.Routines
{
    /// <summary>
    /// Server monster routine which chases after players.
    /// </summary>
    public class ServerChaseRoutine : ServerMonsterRoutine
    {
        private const int stunFrames = 45;
        private TimeSpan chaseCooldown;
        private TimeSpan chaseStart;

        private bool _checkLineOfSight(Point start, Point goal)
        {
            bool ok = false;
            if (start.X != goal.X || start.Y != goal.Y)
            {
                if (start.X == goal.X)
                {
                    ok = true;
                    if (goal.Y > start.Y)
                    {
                        for (int y = start.Y; y <= goal.Y && ok; y++)
                        {
                            ok = this.IsPathable(new Point(start.X, y));
                        }
                    }
                    else
                    {
                        for (int y = start.Y; y >= goal.Y && ok; y--)
                        {
                            ok = this.IsPathable(new Point(start.X, y));
                        }
                    }
                }
                else if (start.Y == goal.Y)
                {
                    ok = true;
                    if (goal.X > start.X)
                    {
                        for (int x = start.X; x <= goal.X && ok; x++)
                        {
                            ok = this.IsPathable(new Point(x, start.Y));
                        }
                    }
                    else
                    {
                        for (int x = start.X; x >= goal.X && ok; x--)
                        {
                            ok = this.IsPathable(new Point(x, start.Y));
                        }
                    }
                }
            }
            return ok;
        }

        public void _tryChase()
        {
            Point myPosition = this.GetTile(monster);
            foreach (Model.Bomberman b in controller.Game.Map.Bombermen)
            {
                if (this.IsValidTarget(b))
                {
                    Point hisPosition = this.GetTile(b);
                    if (_checkLineOfSight(myPosition, hisPosition))
                    {
                        chaseCooldown = TimeSpan.FromSeconds(7.5);
                        chaseStart = TimeSpan.FromSeconds(1.5);
                        Orientations direction = Orientation.OrientationOf(b.Position - monster.Position);

                        // Set dummy velocity to force clients to change orientation
                        monster.Velocity = Tools.Point2Vector(Orientation.GetOffset(direction)) * CreatureMover.DummyThreshold;
                        this.Go(direction, Actions.Special);
                        controller.SendCreaturePosition(monster);
                    }
                }
            }
        }

        public override void Execute(TimeSpan elapsed)
        {
            chaseCooldown -= elapsed;

            if (monster.SpriteState.Action != Actions.Stun || monster.SpriteState.Frame >= stunFrames)
            {
                if (monster.SpriteState.Action == Actions.Special)
                {
                    if (chaseStart > TimeSpan.Zero)
                    {
                        chaseStart -= elapsed;
                    }
                    else
                    {
                        this.StartAnimation(Actions.Run); // Angry!
                    }
                }
                else
                {
                    if (monster.SpriteState.Action == Actions.Run)
                    {
                        Point current = this.GetTile(monster);
                        Point offset = Orientation.GetOffset(Direction);
                        Point next = new Point(current.X + offset.X, current.Y + offset.Y);
                        if (!this.IsPathable(next))
                        {
                            this.StartAnimation(Actions.Stun); // Hit wall! Ouch!
                        }
                    }
                    else if (ChangedTile || monster.SpriteState.Action == Actions.Stun || monster.SpriteState.Action == Actions.Idle)
                    {
                        if (chaseCooldown <= TimeSpan.Zero)
                        {
                            _tryChase();
                        }
                        if(monster.SpriteState.Action != Actions.Special)
                        {
                            Orientations direction = this.PickForwardOrientation(mover.AvailableOrientations);
                            this.Go(direction, Actions.Walk);
                        }
                    }
                    if (monster.SpriteState.Action != Actions.Special)
                    {
                        monster.SpeedFactor = (monster.SpriteState.Action == Actions.Run ? 2.0f : 1.0f);
                        this.Move(elapsed);
                    }
                }
            }
        }

        public ServerChaseRoutine(ServerController controller, Monster monster) : base(controller, monster) { }
    }
}
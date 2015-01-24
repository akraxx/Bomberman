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
    /// Server monster routine which follows players.
    /// </summary>
    public class ServerFollowRoutine : ServerMonsterRoutine
    {
        private Model.Bomberman target = null;

        private void _checkTarget()
        {
            if (!this.IsValidTarget(target) || random.NextDouble() < SwitchTargetChance)
            {
                IList<Model.Bomberman> bombermen = controller.Game.Map.Bombermen;
                target = (bombermen.Count > 0 ? bombermen[random.Next(0, bombermen.Count)] : null);
            }
        }

        /// <summary>
        /// Chance (0~1) of the monster changing target each time it moves from a tile.
        /// </summary>
        public double SwitchTargetChance { get; set; }

        /// <summary>
        /// True if the monster is allowed to turn over when chasing its target.
        /// </summary>
        public bool AllowTurnOver { get; set; }

        public override void Execute(TimeSpan elapsed)
        {
            if (ChangedTile || monster.SpriteState.Action == Actions.Idle)
            {
                _checkTarget();
                Orientations direction;
                if (target != null)
                {
                    Point start = this.GetTile(monster);
                    Point goal = this.GetTile(target);
                    direction = this.PickShortestOrientation(mover.AvailableOrientations, start, goal, AllowTurnOver);
                }
                else
                {
                    direction = this.PickForwardOrientation(mover.AvailableOrientations);
                }
                this.Go(direction, Actions.Walk);
            }
            this.Move(elapsed);
        }

        public ServerFollowRoutine(ServerController controller, Monster monster) : base(controller, monster)
        {
            target = null;
        }
    }
}
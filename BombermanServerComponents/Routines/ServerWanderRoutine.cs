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
    /// Server monster routine which wanders randomly.
    /// </summary>
    public class ServerWanderRoutine : ServerMonsterRoutine
    {
        public override void Execute(TimeSpan elapsed)
        {
            if (ChangedTile || monster.SpriteState.Action == Actions.Idle)
            {
                Orientations direction = this.PickForwardOrientation(mover.AvailableOrientations);
                this.Go(direction, Actions.Walk);
            }
            this.Move(elapsed);
        }

        public ServerWanderRoutine(ServerController controller, Monster monster) : base(controller, monster) { }
    }
}
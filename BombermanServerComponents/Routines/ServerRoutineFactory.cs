using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman.Model;

namespace Bomberman.Server.Routines
{
    public static class ServerRoutineFactory
    {
        public static ServerMonsterRoutine Build(ServerController controller, Monster monster)
        {
            MonsterType type = monster.Type;
            if (type.Name == "zombie")
            {
                return new ServerWanderRoutine(controller, monster);
            }
            else if (type.Name == "mummy")
            {
                return new ServerFollowRoutine(controller, monster) { AllowTurnOver = false, SwitchTargetChance = 0.10 };
            }
            else if (type.Name == "chicken")
            {
                return new ServerChaseRoutine(controller, monster);
            }
            else
            {
                return null;
            }
        }
    }
}
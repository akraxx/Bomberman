using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman.Model;

namespace Bomberman.Network
{
    public static class PayloadFactory
    {
        public static ObjectPayload Build(Model.Object obj)
        {
            if (obj is Blast)
            {
                return new BlastPayload((Blast)obj);
            }
            else if (obj is Bomb)
            {
                return new BombPayload((Bomb)obj);
            }
            else if (obj is Bonus)
            {
                return new BonusPayload((Bonus)obj);
            }
            else if (obj is PowerUp)
            {
                return new PowerUpPayload((PowerUp)obj);
            }
            else if (obj is Wall)
            {
                return new WallPayload((Wall)obj);
            }
            else
            {
                throw new InvalidOperationException("Unsupported object");
            }
        }

        public static CreaturePayload Build(Creature creature)
        {
            if (creature is Model.Bomberman)
            {
                return new BombermanPayload((Model.Bomberman)creature);
            }
            else if (creature is Monster)
            {
                return new MonsterPayload((Monster)creature);
            }
            else
            {
                throw new InvalidOperationException("Unsupported creature");
            }
        }
    }
}
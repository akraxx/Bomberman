using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class MonsterPayload : CreaturePayload
    {
        public byte Type { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write(Type);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Type = r.ReadByte();
        }

        public static MonsterPayload Read(BinaryReader r)
        {
            MonsterPayload p = new MonsterPayload();
            p.Assign(r);
            return p;
        }

        public override Creature Build()
        {
            Monster monster = new Monster(MonsterType.GetByID(Type), ID, VectorCodec.Decode(Position));
            monster.SetState(this);
            return monster;
        }

        public MonsterPayload(Monster monster) : base(monster)
        {
            Type = monster.Type.ID;
        }

        public MonsterPayload()
        {
            Type = 0;
        }
    }
}
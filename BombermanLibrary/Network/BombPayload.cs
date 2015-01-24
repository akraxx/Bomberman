using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class BombPayload : TimedObjectPayload
    {
        public Bomb.Types Type { get; set; }
        public ushort Owner { get; set; }
        public int Power { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write((byte)Type);
            w.Write(Owner);
            w.Write(Power);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Type = (Bomb.Types)r.ReadByte();
            Owner = r.ReadUInt16();
            Power = r.ReadInt32();
        }

        public static BombPayload Read(BinaryReader r)
        {
            BombPayload p = new BombPayload();
            p.Assign(r);
            return p;
        }

        public override Model.Object Build()
        {
            return new Bomb(Type, Owner, Power, Timer, new Point(X, Y));
        }

        public BombPayload(Bomb bomb) : base(bomb)
        {
            Type = bomb.Type;
            Owner = bomb.Owner;
            Power = bomb.Power;
        }

        public BombPayload()
        {
            Type = Bomb.Types.Normal;
            Owner = 0;
            Power = 0;
        }
    }
}
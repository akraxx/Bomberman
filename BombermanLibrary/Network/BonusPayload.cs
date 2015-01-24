using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class BonusPayload : TimedObjectPayload
    {
        public int SpriteIndex { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write(SpriteIndex);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            SpriteIndex = r.ReadInt32();
        }

        public static BonusPayload Read(BinaryReader r)
        {
            BonusPayload p = new BonusPayload();
            p.Assign(r);
            return p;
        }

        public override Model.Object Build()
        {
            return new Bonus(SpriteIndex, Timer, new Point(X, Y));
        }

        public BonusPayload(Bonus bonus) : base(bonus)
        {
            SpriteIndex = bonus.SpriteIndex;
        }

        public BonusPayload()
        {
            SpriteIndex = 0;
        }
    }
}

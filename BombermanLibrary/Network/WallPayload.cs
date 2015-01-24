using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class WallPayload : TimedObjectPayload
    {
        public bool Destructible { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write(Destructible);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Destructible = r.ReadBoolean();
        }

        public static WallPayload Read(BinaryReader r)
        {
            WallPayload p = new WallPayload();
            p.Assign(r);
            return p;
        }

        public override Model.Object Build()
        {
            return new Wall(Destructible, null, new Point(X, Y));
        }

        public WallPayload(Wall wall) : base(wall)
        {
            Destructible = wall.Destructible;
        }

        public WallPayload()
        {
            Destructible = false;
        }
    }
}
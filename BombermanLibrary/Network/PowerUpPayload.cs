using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class PowerUpPayload : TimedObjectPayload
    {
        public PowerUp.Types Type { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write((byte)Type);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Type = (PowerUp.Types)r.ReadByte();
        }

        public static PowerUpPayload Read(BinaryReader r)
        {
            PowerUpPayload p = new PowerUpPayload();
            p.Assign(r);
            return p;
        }

        public override Model.Object Build()
        {
            return new PowerUp(Type, Timer, new Point(X, Y));
        }

        public PowerUpPayload(PowerUp powerUp) : base(powerUp)
        {
            Type = powerUp.Type;
        }

        public PowerUpPayload()
        {
            Type = PowerUp.Types.Bomb;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class BlastPayload : TimedObjectPayload
    {
        public Blast.Types Type { get; set; }
        public Orientations Orientation { get; set; }
        public ushort Owner { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write((byte)Type);
            w.Write((byte)Orientation);
            w.Write(Owner);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Type = (Blast.Types)r.ReadByte();
            Orientation = (Orientations)r.ReadByte();
            Owner = r.ReadUInt16();
        }

        public static BlastPayload Read(BinaryReader r)
        {
            BlastPayload p = new BlastPayload();
            p.Assign(r);
            return p;
        }

        public override Model.Object Build()
        {
            return new Blast(Type, Orientation, Owner, Timer, new Point(X, Y));
        }

        public BlastPayload(Blast blast) : base(blast)
        {
            Type = blast.Type;
            Orientation = blast.Orientation;
            Owner = blast.Owner;
        }

        public BlastPayload()
        {
            Type = Blast.Types.Single;
            Orientation = Orientations.Bottom;
            Owner = 0;
        }
    }
}
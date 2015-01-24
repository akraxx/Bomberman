using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Bomberman.Network
{
    public sealed class PositionPayload
    {
        public int[] Position { get; set; }
        public int[] Velocity { get; set; }

        public Vector2 DecodePosition()
        {
            return VectorCodec.Decode(Position);
        }

        public Vector2 DecodeVelocity()
        {
            return VectorCodec.Decode(Velocity);
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Position[0]);
            w.Write(Position[1]);
            w.Write(Velocity[0]);
            w.Write(Velocity[1]);
        }

        public static PositionPayload Read(BinaryReader r)
        {
            PositionPayload p = new PositionPayload();
            p.Position[0] = r.ReadInt32();
            p.Position[1] = r.ReadInt32();
            p.Velocity[0] = r.ReadInt32();
            p.Velocity[1] = r.ReadInt32();
            return p;
        }

        public PositionPayload(Vector2 position, Vector2 velocity)
        {
            Position = VectorCodec.Encode(position);
            Velocity = VectorCodec.Encode(velocity);
        }

        public PositionPayload() : this(Vector2.Zero, Vector2.Zero) { }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public abstract class ObjectPayload
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point DecodePosition()
        {
            return new Point(X, Y);
        }

        public virtual void Write(BinaryWriter w)
        {
            w.Write(X);
            w.Write(Y);
        }

        protected virtual void Assign(BinaryReader r)
        {
            X = r.ReadInt32();
            Y = r.ReadInt32();
        }

        public abstract Model.Object Build();

        public ObjectPayload(Model.Object obj)
        {
            X = obj.Position.X;
            Y = obj.Position.Y;
        }

        public ObjectPayload()
        {
            X = 0;
            Y = 0;
        }
    }
}
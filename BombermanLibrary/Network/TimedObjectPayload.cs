using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public abstract class TimedObjectPayload : ObjectPayload
    {
        public TimeSpan Timer { get; set; }
        public TimeSpan MaxTimer { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write(Timer.Ticks);
            w.Write(MaxTimer.Ticks);
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Timer = TimeSpan.FromTicks(r.ReadInt64());
            MaxTimer = TimeSpan.FromTicks(r.ReadInt64());
        }

        public TimedObjectPayload(TimedObject obj) : base(obj)
        {
            Timer = obj.Timer;
            MaxTimer = obj.MaxTimer;
        }

        public TimedObjectPayload()
        {
            Timer = TimeSpan.Zero;
            MaxTimer = TimeSpan.Zero;
        }
    }
}
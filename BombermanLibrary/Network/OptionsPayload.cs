using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bomberman.Model;

namespace Bomberman.Network
{
    public sealed class OptionsPayload
    {
        public Status.Modes Mode { get; set; }
        public bool Continuable { get; set; }
        public int WinsThreshold { get; set; }

        public void Write(BinaryWriter w)
        {
            w.Write((byte)Mode);
            w.Write(Continuable);
            w.Write(WinsThreshold);
        }

        public static OptionsPayload Read(BinaryReader r)
        {
            return new OptionsPayload()
            {
                Mode = (Status.Modes)r.ReadByte(),
                Continuable = r.ReadBoolean(),
                WinsThreshold = r.ReadInt32(),
            };
        }

        public OptionsPayload(Status.Modes mode, bool continuable, int winsThreshold)
        {
            Mode = mode;
            Continuable = continuable;
            WinsThreshold = winsThreshold;
        }

        public OptionsPayload() : this(Status.Modes.Cooperation, false, 0) { }
    }
}
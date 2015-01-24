using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bomberman.Network
{
    public sealed class SetMapPayload
    {
        public int Number { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Theme { get; set; }

        public void Write(BinaryWriter w)
        {
            w.Write(Number);
            w.Write(Width);
            w.Write(Height);
            w.Write(Theme);
        }

        public static SetMapPayload Read(BinaryReader r)
        {
            return new SetMapPayload()
            {
                Number = r.ReadInt32(),
                Width = r.ReadInt32(),
                Height = r.ReadInt32(),
                Theme = r.ReadInt32(),
            };
        }

        public SetMapPayload(int number, int width, int height, int theme)
        {
            Number = number;
            Width = width;
            Height = height;
            Theme = theme;
        }

        public SetMapPayload() : this(0, 0, 0, 0) { }
    }
}
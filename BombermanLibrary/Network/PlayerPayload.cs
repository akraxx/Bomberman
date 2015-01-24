using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Bomberman.Model;

namespace Bomberman.Network
{
    public sealed class PlayerPayload
    {
        public byte ID { get; set; }
        public bool Waiting { get; set; }
        public bool GameOver { get; set; }
        public bool Host { get; set; }
        public bool Local { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Wins { get; set; }
        public int Stock { get; set; }

        public void Write(BinaryWriter w)
        {
            w.Write(ID);
            w.Write(Waiting);
            w.Write(GameOver);
            w.Write(Host);
            w.Write(Local);
            w.Write(Name);
            w.Write(Score);
            w.Write(Wins);
            w.Write(Stock);
        }

        public static PlayerPayload Read(BinaryReader r)
        {
            return new PlayerPayload()
            {
                ID = r.ReadByte(),
                Waiting = r.ReadBoolean(),
                GameOver = r.ReadBoolean(),
                Host = r.ReadBoolean(),
                Local = r.ReadBoolean(),
                Name = r.ReadString(),
                Score = r.ReadInt32(),
                Wins = r.ReadInt32(),
                Stock = r.ReadInt32(),
            };
        }

        public PlayerPayload()
        {
            ID = 0;
            Waiting = false;
            GameOver = false;
            Host = false;
            Local = false;
            Name = "";
            Score = 0;
            Wins = 0;
            Stock = 0;
        }

        public PlayerPayload(Player player)
        {
            ID = player.ID;
            Waiting = player.Waiting;
            GameOver = player.GameOver;
            Host = player.Host;
            Local = player.Local;
            Name = player.Name;
            Score = player.Score;
            Wins = player.Wins;
            Stock = player.Stock;
        }
    }
}
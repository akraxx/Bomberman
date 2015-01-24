using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public class BombermanPayload : CreaturePayload
    {
        public byte Player { get; set; }
        public int Bombs { get; set; }
        public int Power { get; set; }
        public int Boots { get; set; }
        public int[] BombStock { get; set; }

        public override void Write(BinaryWriter w)
        {
            base.Write(w);
            w.Write(Player);
            w.Write(Bombs);
            w.Write(Power);
            w.Write(Boots);
            for (int i = 0; i < 3; i++)
            {
                w.Write(BombStock[i]);
            }
        }

        protected override void Assign(BinaryReader r)
        {
            base.Assign(r);
            Player = r.ReadByte();
            Bombs = r.ReadInt32();
            Power = r.ReadInt32();
            Boots = r.ReadInt32();
            for (int i = 0; i < 3; i++)
            {
                BombStock[i] = r.ReadInt32();
            }
        }

        public static BombermanPayload Read(BinaryReader r)
        {
            BombermanPayload p = new BombermanPayload();
            p.Assign(r);
            return p;
        }

        public override Creature Build()
        {
            Model.Bomberman bomberman = new Model.Bomberman(Player, ID, VectorCodec.Decode(Position));
            bomberman.SetState(this);
            return bomberman;
        }

        public BombermanPayload(Model.Bomberman bomberman) : base(bomberman)
        {
            Player = bomberman.Player;
            Bombs = bomberman.Bombs;
            Power = bomberman.Power;
            Boots = bomberman.Boots;
            BombStock = new int[3] { 0, 0, 0 };
            for (int i = 0; i < 3; i++)
            {
                BombStock[i] = bomberman.GetBombType((Bomb.Types)i);
            }
        }

        public BombermanPayload()
        {
            Player = 0;
            Bombs = 0;
            Power = 0;
            Boots = 0;
            BombStock = new int[3] { 0, 0, 0 };
        }
    }
}
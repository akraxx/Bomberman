using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    public abstract class CreaturePayload
    {
        public ushort ID { get; set; }
        public int[] Position { get; set; }
        public int[] Velocity { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public TimeSpan Invulnerability { get; set; }

        public virtual void Write(BinaryWriter w)
        {
            w.Write(ID);
            w.Write(Position[0]);
            w.Write(Position[1]);
            w.Write(Velocity[0]);
            w.Write(Velocity[1]);
            w.Write(Health);
            w.Write(MaxHealth);
            w.Write(Invulnerability.Ticks);
        }

        protected virtual void Assign(BinaryReader r)
        {
            ID = r.ReadUInt16();
            Position[0] = r.ReadInt32();
            Position[1] = r.ReadInt32();
            Velocity[0] = r.ReadInt32();
            Velocity[1] = r.ReadInt32();
            Health = r.ReadInt32();
            MaxHealth = r.ReadInt32();
            Invulnerability = TimeSpan.FromTicks(r.ReadInt64());
        }

        public abstract Creature Build();

        public CreaturePayload(Creature creature)
        {
            ID = creature.ID;
            Position = VectorCodec.Encode(creature.Position);
            Velocity = VectorCodec.Encode(creature.Velocity);
            Health = creature.Health;
            MaxHealth = creature.MaxHealth;
            Invulnerability = creature.Invulnerability;
        }

        public CreaturePayload()
        {
            ID = 0;
            Position = VectorCodec.Encode(Vector2.Zero);
            Velocity = VectorCodec.Encode(Vector2.Zero);
            Health = 0;
            MaxHealth = 0;
            Invulnerability = TimeSpan.Zero;
        }
    }
}
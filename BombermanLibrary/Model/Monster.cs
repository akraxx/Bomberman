using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a monster creature.
    /// </summary>
    public sealed class Monster : Creature
    {
        private byte typeId;

        /// <summary>
        /// The type of the monster.
        /// </summary>
        public MonsterType Type { get { return MonsterType.GetByID(typeId); } }

        /// <summary>
        /// Name of the monster.
        /// </summary>
        public override string Name { get { return Type.Name; } }

        /// <summary>
        /// Speed of the monster.
        /// </summary>
        public override float Speed { get { return Type.Speed * SpeedFactor; } }

        /// <summary>
        /// Speed factor of the monster (write-only).
        /// Some monsters get speed boosts under certain circumstances.
        /// </summary>
        public float SpeedFactor { private get; set; }

        /// <summary>
        /// Apply the state of a monster payload.
        /// State doesn't include type, position or ID.
        /// </summary>
        public override void SetState(Network.CreaturePayload creaturePayload)
        {
            base.SetState(creaturePayload);
        }

        /// <summary>
        /// Construct a monster using an ID specified by he user.
        /// </summary>
        /// <param name="type">The type of the monster. Mandatory.</param>
        /// <param name="id">The unique ID to assign to the creature. You should only put IDs that are trusted to be unique.</param>
        /// <param name="position">The position of the monster.</param>
        public Monster(MonsterType type, ushort id, Vector2 position) : base(id)
        {
            if (type != null)
            {
                Position = position;
                Health = type.MaxHealth;
                MaxHealth = type.MaxHealth;
                typeId = type.ID;
                SpeedFactor = 1.0f;
            }
            else
            {
                throw new ArgumentNullException("type");
            }
        }

        /// <summary>
        /// Construct a monster using an ID generated through an IDGenerator.
        /// </summary>
        /// <param name="type">The type of the monster. Mandatory.</param>
        /// <param name="generator">The IDGenerator that will be used to generate an unique ID for this creature. Mandatory.</param>
        /// <param name="position">The position of the monster.</param>
        public Monster(MonsterType type, IDGenerator generator, Vector2 position) : this(type, generator.GenerateID(), position) { }
    }
}
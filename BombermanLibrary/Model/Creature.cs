using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// The base class of all game creatures.
    /// </summary>
    public abstract class Creature
    {
        private static readonly TimeSpan damageInvulnerability = TimeSpan.FromSeconds(3.0);

        /// <summary>
        /// The unique ID given to this creature.
        /// The unicity is used to uniquely identify this creature in network communications.
        /// </summary>
        public ushort ID { get; private set; }

        /// <summary>
        /// Position of the creature (given as fraction of tiles).
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Velocity of the creature.
        /// </summary>
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// Name of the creature.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Speed of the creature.
        /// </summary>
        public abstract float Speed { get; }

        /// <summary>
        /// Sprite state data of the creature.
        /// </summary>
        public SpriteState SpriteState;

        /// <summary>
        /// Health of the creature. When depleted, the creature is considered dead.
        /// </summary>
        public int Health { get; protected set; }

        /// <summary>
        /// Max health of the creature.
        /// </summary>
        public int MaxHealth { get; protected set; }

        /// <summary>
        /// True if the creature is considered dead.
        /// </summary>
        public bool Dead { get { return Health == 0; } }

        /// <summary>
        /// Invulnerability timer for this creature.
        /// </summary>
        public TimeSpan Invulnerability { get; set; }

        /// <summary>
        /// True if the creature is invulnerable.
        /// </summary>
        public bool Invulnerable { get { return Invulnerability > TimeSpan.Zero; } }

        /// <summary>
        /// Creates a copy of the creature.
        /// </summary>
        public virtual Creature Copy() { return (Creature)this.MemberwiseClone(); }

        /// <summary>
        /// Apply the state of a creature payload.
        /// State should not include immutable data (such as ID), sprite state or position.
        /// </summary>
        public virtual void SetState(Network.CreaturePayload creaturePayload)
        {
            Health = creaturePayload.Health;
            MaxHealth = creaturePayload.MaxHealth;
            Invulnerability = creaturePayload.Invulnerability;
        }

        /// <summary>
        /// Update orientation of the sprite according to its velocity.
        /// </summary>
        public void UpdateOrientation()
        {
            if (Velocity != Vector2.Zero)
            {
                SpriteState.Orientation = Orientation.OrientationOf(Velocity);
            }
        }

        /// <summary>
        /// Inflicts health damage on the creature.
        /// </summary>
        public void Damage(int amount)
        {
            if (amount > 0)
            {
                Health = Math.Max(0, Health - amount);

                if(Health > 0) Invulnerability = damageInvulnerability;
            }
        }

        public Creature(ushort id)
        {
            ID = id;
        }

        public Creature(IDGenerator generator) : this(generator.GenerateID()) { }
    }
}
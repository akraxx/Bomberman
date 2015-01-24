using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a bomberman creature.
    /// </summary>
    public sealed class Bomberman : Creature
    {
        private static readonly TimeSpan spawnInvulnerability = TimeSpan.FromSeconds(5.0);

        private int[] bombStock = new int[Bomb.NumTypes] { int.MaxValue, 0, 0 };

        /// <summary>
        /// ID of the player who owns this bomberman.
        /// </summary>
        public byte Player { get; private set; }

        /// <summary>
        /// Simultaneous possible bombs for this bomberman.
        /// </summary>
        public int Bombs { get; private set; }

        /// <summary>
        /// Power of the bombs of this bomberman.
        /// </summary>
        public int Power { get; private set; }

        /// <summary>
        /// Number of boots powerups applied on this bomberman.
        /// </summary>
        public int Boots { get; private set; }

        /// <summary>
        /// Name of the bomberman.
        /// </summary>
        public override string Name { get { return "bomberman" + (Player + 1); } }

        /// <summary>
        /// Speed of the bomberman.
        /// </summary>
        public override float Speed { get { return 1.0f + Boots * 0.5f; } }

        /// <summary>
        /// Add one bomb to the bomb count of this bomberman.
        /// </summary>
        public void AddBomb()
        {
            if (Bombs < 3)
            {
                Bombs++;
            }
        }

        /// <summary>
        /// Add one unit of power to this bomberman.
        /// </summary>
        public void AddPower()
        {
            if (Power < 7)
            {
                Power++;
            }
        }

        /// <summary>
        /// Grants boots bonus to the bomberman.
        /// </summary>
        public void AddBoots()
        {
            if (Boots < 2)
            {
                Boots++;
            }
        }

        /// <summary>
        /// Depower the bomberman.
        /// </summary>
        public void Nerf()
        {
            Bombs = 1;
            Power = 2;
            Boots = 0;
            bombStock[(int)Bomb.Types.Remote] = 0;
            bombStock[(int)Bomb.Types.Split] = 0;
        }

        /// <summary>
        /// Get the bomb stock of the specified type of the bomberman.
        /// </summary>
        public int GetBombType(Bomb.Types type)
        {
            return bombStock[(int)type];
        }

        /// <summary>
        /// Add bombs of the specified type to the bomb stock of the bomberman.
        /// </summary>
        public void RefillBombType(Bomb.Types type, int count)
        {
            int added = Math.Min(9, bombStock[(int)type] + count) - bombStock[(int)type];
            if (added > 0)
            {
                bombStock[(int)type] += added;
            }
        }

        /// <summary>
        /// Try to remove one bomb of the specified type from the bomb stock of the bomberman.
        /// Return true if one bomb was removed successfully from the stock.
        /// </summary>
        public bool UseBombType(Bomb.Types type)
        {
            if (bombStock[(int)type] > 0)
            {
                bombStock[(int)type]--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Apply the state of a bomberman payload.
        /// State doesn't include player, position or ID.
        /// </summary>
        public override void SetState(Network.CreaturePayload creaturePayload)
        {
            base.SetState(creaturePayload);

            Network.BombermanPayload bombermanPayload = (Network.BombermanPayload)creaturePayload;
            Bombs = bombermanPayload.Bombs;
            Power = bombermanPayload.Power;
            Boots = bombermanPayload.Boots;
            for (int i = 0; i < Bomb.NumTypes; i++)
            {
                bombStock[i] = bombermanPayload.BombStock[i];
            }
        }

        /// <summary>
        /// Construct a bomberman using an ID specified by the user.
        /// </summary>
        /// <param name="player">The player owning this bomberman.</param>
        /// <param name="id">The unique ID to assign to the creature. You should only put IDs that are trusted to be unique.</param>
        /// <param name="position">The position of the bomberman.</param>
        public Bomberman(byte player, ushort id, Vector2 position) : base(id)
        {
            Position = position;
            Health = 1;
            MaxHealth = 1;
            Invulnerability = spawnInvulnerability;
            Player = player;
            Bombs = 1;
            Power = 3;
            Boots = 0;
        }

        /// <summary>
        /// Construct a bomberman using an ID generated through an IDGenerator.
        /// </summary>
        /// <param name="player">The player owning this bomberman.</param>
        /// <param name="generator">The IDGenerator that will be used to generate an unique ID for this creature. Mandatory.</param>
        /// <param name="position">The position of the bomberman.</param>
        public Bomberman(byte player, IDGenerator generator, Vector2 position) : this(player, generator.GenerateID(), position) { }
    }
}
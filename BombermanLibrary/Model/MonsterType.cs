using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Model
{
    public sealed class MonsterType
    {
        private static byte currentId = 0;
        private static readonly List<MonsterType> typesById = new List<MonsterType>();

        //                                                                       VAL   SPD  HP
        public static readonly MonsterType Zombie  = new MonsterType("zombie",   100, 0.5f, 1);
        public static readonly MonsterType Mummy   = new MonsterType("mummy",    300, 1.0f, 2);
        public static readonly MonsterType Chicken = new MonsterType("chicken", 2500, 1.5f, 10);

        public static MonsterType GetByID(byte ID)
        {
            return typesById[ID];
        }

        /// <summary>
        /// ID of this monster type.
        /// </summary>
        public byte ID { get; private set; }

        /// <summary>
        /// Name of the monster.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Value of the monster in points.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Base speed of the monster.
        /// </summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Max health of the monster.
        /// </summary>
        public int MaxHealth { get; private set; }

        private MonsterType(String name, int value, float speed, int maxHealth)
        {
            ID = currentId;
            currentId++;
            typesById.Add(this);

            Name = name;
            Value = value;
            Speed = speed;
            MaxHealth = maxHealth;
        }
    }
}
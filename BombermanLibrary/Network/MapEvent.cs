using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Bomberman.Model;

namespace Bomberman.Network
{
    /// <summary>
    /// Contains data for "Map" message events.
    /// </summary>
    public sealed class MapEvent
    {
        public enum Actions
        {
            /// <summary>
            /// Indicates an object or creature has spawned.
            /// Applicable to: all
            /// Argument: ObjectPayload or CreaturePayload
            /// </summary>
            Spawn,

            /// <summary>
            /// Indicates an object or creature has despawned.
            /// Applicable to: all
            /// Argument: null
            /// </summary>
            Despawn,

            /// <summary>
            /// Gives the full state of a creature.
            /// Applicable to: Bomberman, Monster
            /// Argument: CreaturePayload
            /// </summary>
            State,

            /// <summary>
            /// Indicates an object is being destroyed.
            /// Applicable to: Bomb, Bonus, PowerUp, Wall
            /// Argument: Wall                 => Timer
            /// Argument: Bomb, Bonus, PowerUp => null
            /// </summary>
            Destroy,

            /// <summary>
            /// Indicates an object has been picked up.
            /// Applicable to: Bonus, PowerUp
            /// Argument: null
            /// </summary>
            Picked,

            /// <summary>
            /// Indicates an object has begun to play an animation.
            /// Applicable to: Bomberman, Monster
            /// Argument: Action
            /// </summary>
            Animation,

            /// <summary>
            /// Applicable to: Bomberman, Monster
            /// Argument: PositionPayload
            /// </summary>
            Position,
        }

        /// <summary>
        /// The possible targets of the map event.
        /// </summary>
        public enum Targets
        {
            Object,
            Creature,
        }

        /// <summary>
        /// The possible concrete game object types in the argument.
        /// </summary>
        public enum ArgumentTypes
        {
            None,
            Timer,
            Action,
            Position,
            Blast,
            Bomb,
            Bonus,
            PowerUp,
            Wall,
            Bomberman,
            Monster,
        }

        private static ArgumentTypes GetArgumentType(System.Object argument)
        {
            if (argument == null)
            {
                return ArgumentTypes.None;
            }
            else if (argument is TimeSpan)
            {
                return ArgumentTypes.Timer;
            }
            else if (argument is Model.Actions)
            {
                return ArgumentTypes.Action;
            }
            else if (argument is PositionPayload)
            {
                return ArgumentTypes.Position;
            }
            else if (argument is BlastPayload)
            {
                return ArgumentTypes.Blast;
            }
            else if (argument is BombPayload)
            {
                return ArgumentTypes.Bomb;
            }
            else if (argument is BonusPayload)
            {
                return ArgumentTypes.Bonus;
            }
            else if (argument is BonusPayload)
            {
                return ArgumentTypes.Bonus;
            }
            else if (argument is PowerUpPayload)
            {
                return ArgumentTypes.PowerUp;
            }
            else if (argument is WallPayload)
            {
                return ArgumentTypes.Wall;
            }
            else if (argument is BombermanPayload)
            {
                return ArgumentTypes.Bomberman;
            }
            else if (argument is MonsterPayload)
            {
                return ArgumentTypes.Monster;
            }
            else
            {
                throw new InvalidOperationException("Unsupported argument");
            }
        }

        /// <summary>
        /// Action of the map event.
        /// </summary>
        public Actions Action { get; set; }

        /// <summary>
        /// Target of the map event.
        /// </summary>
        public Targets Target { get; set; }

        /// <summary>
        /// Entity identified by its ID.
        /// </summary>
        public ushort ID { get; set; }

        /// <summary>
        /// Type of the argument.
        /// </summary>
        public ArgumentTypes ArgumentType { get; set; }

        /// <summary>
        /// Argument of the action.
        /// See actions above for details.
        /// </summary>
        public System.Object Argument { get; set; }

        /// <summary>
        /// Read a MapEvent from the specified binary reader.
        /// </summary>
        public static MapEvent Read(BinaryReader r)
        {
            MapEvent m = new MapEvent();
            m.Action = (Actions)r.ReadByte();
            m.Target = (Targets)r.ReadByte();
            m.ID = r.ReadUInt16();
            m.ArgumentType = (ArgumentTypes)r.ReadByte();
            if (m.ArgumentType == ArgumentTypes.Timer)
            {
                m.Argument = TimeSpan.FromTicks(r.ReadInt64());
            }
            else if (m.ArgumentType == ArgumentTypes.Action)
            {
                m.Argument = (Model.Actions)r.ReadInt32();
            }
            else if (m.ArgumentType == ArgumentTypes.Position)
            {
                m.Argument = PositionPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.Blast)
            {
                m.Argument = BlastPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.Bomb)
            {
                m.Argument = BombPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.Bonus)
            {
                m.Argument = BonusPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.PowerUp)
            {
                m.Argument = PowerUpPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.Wall)
            {
                m.Argument = WallPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.Bomberman)
            {
                m.Argument = BombermanPayload.Read(r);
            }
            else if (m.ArgumentType == ArgumentTypes.Monster)
            {
                m.Argument = MonsterPayload.Read(r);
            }
            return m;
        }

        /// <summary>
        /// Write the MapEvent to the specified binary writer.
        /// </summary>
        public void Write(BinaryWriter w)
        {
            w.Write((byte)Action);
            w.Write((byte)Target);
            w.Write(ID);
            w.Write((byte)ArgumentType);
            if (ArgumentType == ArgumentTypes.Timer)
            {
                w.Write(((TimeSpan)Argument).Ticks);
            }
            else if (ArgumentType == ArgumentTypes.Action)
            {
                w.Write((int)Argument);
            }
            else if (ArgumentType == ArgumentTypes.Position)
            {
                ((PositionPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.Blast)
            {
                ((BlastPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.Bomb)
            {
                ((BombPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.Bonus)
            {
                ((BonusPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.PowerUp)
            {
                ((PowerUpPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.Wall)
            {
                ((WallPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.Bomberman)
            {
                ((BombermanPayload)Argument).Write(w);
            }
            else if (ArgumentType == ArgumentTypes.Monster)
            {
                ((MonsterPayload)Argument).Write(w);
            }
        }

        public MapEvent(Actions action, Targets target, ushort id, ArgumentTypes argumentType, System.Object argument)
        {
            Action = action;
            Target = target;
            ID = id;
            ArgumentType = argumentType;
            Argument = argument;
        }

        public MapEvent(Actions action, Model.Object o)
        {
            Action = action;
            Target = Targets.Object;
            ID = o.ID;

            if (action == Actions.Spawn)
            {
                Argument = PayloadFactory.Build(o);
            }
            else if (action == Actions.Despawn)
            {
                Argument = null;
            }
            else if (action == Actions.Destroy)
            {
                if (o is Wall)
                {
                    Argument = ((Wall)o).Timer;
                }
                else
                {
                    Argument = null;
                }
            }
            else if (action == Actions.Picked)
            {
                Argument = null;
            }
            else
            {
                throw new InvalidOperationException("Can't build that action");
            }

            ArgumentType = GetArgumentType(Argument);
        }

        public MapEvent(Actions action, Creature c)
        {
            Action = action;
            Target = Targets.Creature;
            ID = c.ID;

            if (action == Actions.Spawn)
            {
                Argument = PayloadFactory.Build(c);
            }
            else if (action == Actions.Despawn)
            {
                Argument = null;
            }
            else if (action == Actions.State)
            {
                Argument = PayloadFactory.Build(c);
            }
            else if (action == Actions.Animation)
            {
                Argument = c.SpriteState.Action;
            }
            else if (action == Actions.Position)
            {
                Argument = new PositionPayload(c.Position, c.Velocity);
            }
            else
            {
                throw new InvalidOperationException("Can't build that action");
            }

            ArgumentType = GetArgumentType(Argument);
        }

        public MapEvent() : this(Actions.Despawn, Targets.Object, 0, ArgumentTypes.None, null) { }
    }
}
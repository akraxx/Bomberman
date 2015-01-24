using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Client
{
    /// <summary>
    /// Client-side message executer.
    /// It executes MessageEvent received from the server.
    /// </summary>
    public class ClientMessageExecuter
    {
        private ClientController controller;

        /// <summary>
        /// Execute a message received from the server.
        /// </summary>
        public void Execute(MessageEvent m)
        {
            Game game = controller.Game;
            Status status = game.Status;
            Map map = game.Map;

            if (m.Type == MessageEvent.Types.Chat)
            {
                controller.NotifyChatMessage((string)m.Payload);
            }
            else if (m.Type == MessageEvent.Types.Options)
            {
                OptionsPayload optionsPayload = (OptionsPayload)m.Payload;
                status.SetMode(optionsPayload.Mode);
                status.Continuable = optionsPayload.Continuable;
            }
            else if(m.Type == MessageEvent.Types.PauseGame)
            {
                status.SetPause((bool)m.Payload);
            }
            else if (m.Type == MessageEvent.Types.Kicked)
            {
                ReasonCodes reason = (ReasonCodes)m.Payload;
                controller.DisconnectReason = reason;
                controller.Disconnect();
            }
            else if(m.Type == MessageEvent.Types.PlayerJoined)
            {
                PlayerPayload playerPayload = (PlayerPayload)m.Payload;
                status.Players[playerPayload.ID].Join(playerPayload.Name, playerPayload.Host, playerPayload.Local);
            }
            else if(m.Type == MessageEvent.Types.PlayerLeft)
            {
                status.Players[(byte)m.Payload].Leave();
            }
            else if (m.Type == MessageEvent.Types.PlayerState)
            {
                PlayerPayload playerPayload = (PlayerPayload)m.Payload;
                Player targetPlayer = status.Players[playerPayload.ID];
                targetPlayer.Waiting = playerPayload.Waiting;
                targetPlayer.GameOver = playerPayload.GameOver;
                targetPlayer.Score = playerPayload.Score;
                targetPlayer.Wins = playerPayload.Wins;
                targetPlayer.Stock = playerPayload.Stock;
            }
            else if(m.Type == MessageEvent.Types.ChangePhase)
            {
                status.SetPhase((Status.Phases)m.Payload);
            }
            else if(m.Type == MessageEvent.Types.SetMap)
            {
                SetMapPayload setMapPayload = (SetMapPayload)m.Payload;
                Level level = Level.GetEmptyLevel(setMapPayload.Number, setMapPayload.Width, setMapPayload.Height, setMapPayload.Theme);
                status.SetRound(level.Number);
                map.Transfer(level.Map);
                foreach (Player p in status.Players)
                {
                    p.Bomberman = null;
                }
            }
            else if(m.Type == MessageEvent.Types.SetTimeLimit)
            {
                status.TimeLimit = (TimeSpan)m.Payload;
            }
            else if (m.Type == MessageEvent.Types.TimeUp)
            {
                controller.NotifyTimeUp();
            }
            else if (m.Type == MessageEvent.Types.MapEvent)
            {
                this.Execute((MapEvent)m.Payload);
            }
            else
            {
                Debug.WriteLine("[CLIENT] Unsupported game message received: " + m.Type.ToString());
#if DEBUG
                Debugger.Break();
#endif
            }
        }

        /// <summary>
        /// Execute a map event received from the server.
        /// </summary>
        public void Execute(MapEvent m)
        {
            Game game = controller.Game;
            Status status = game.Status;
            Map map = game.Map;

            try
            {
                if (m.Action == MapEvent.Actions.Spawn)
                {
                    if (m.Target == MapEvent.Targets.Object)
                    {
                        ObjectPayload objectPayload = (ObjectPayload)m.Argument;
                        map.Add(objectPayload.Build());
                    }
                    else if (m.Target == MapEvent.Targets.Creature)
                    {
                        CreaturePayload creaturePayload = (CreaturePayload)m.Argument;
                        Creature creature = creaturePayload.Build();
                        if (creature is Model.Bomberman)
                        {
                            Model.Bomberman bomberman = (Model.Bomberman)creature;
                            Player player = status.Players.First(p => p.ID == bomberman.Player);
                            player.Bomberman = bomberman;
                        }
                        map.Add(creature);
                    }
                }
                else if (m.Action == MapEvent.Actions.Despawn)
                {
                    if (m.Target == MapEvent.Targets.Object)
                    {
                        Model.Object obj = map.Objects.First(o => o.ID == m.ID);
                        map.Remove(obj);
                    }
                    else if (m.Target == MapEvent.Targets.Creature)
                    {
                        Creature creature = map.SortedCreatures.First(c => c.ID == m.ID);
                        if (creature is Model.Bomberman)
                        {
                            Model.Bomberman bomberman = (Model.Bomberman)creature;
                            Player player = status.Players.First(p => p.ID == bomberman.Player);
                            player.Bomberman = null;
                        }
                        map.Remove(creature);
                    }
                }
                else if (m.Action == MapEvent.Actions.State)
                {
                    if (m.Target == MapEvent.Targets.Creature)
                    {
                        CreaturePayload creaturePayload = (CreaturePayload)m.Argument;
                        Creature creature = map.SortedCreatures.First(c => c.ID == m.ID);
                        int health = creature.Health;
                        creature.SetState(creaturePayload);
                        if (health != creature.Health) controller.NotifyHealth(creature);
                    }
                }
                else if (m.Action == MapEvent.Actions.Destroy)
                {
                    if (m.Target == MapEvent.Targets.Object)
                    {
                        Model.Object obj = map.Objects.First(o => o.ID == m.ID);
                        if (obj is Wall)
                        {
                            ((Wall)obj).Destroy((TimeSpan)m.Argument);
                        }
                        controller.NotifyDestroy(obj);
                    }
                }
                else if (m.Action == MapEvent.Actions.Picked)
                {
                    Model.Object obj = map.Objects.First(o => o.ID == m.ID);
                    controller.NotifyPicked(obj);
                }
                else if (m.Action == MapEvent.Actions.Animation)
                {
                    if (m.Target == MapEvent.Targets.Creature)
                    {
                        Creature creature = map.SortedCreatures.First(c => c.ID == m.ID);
                        creature.SpriteState.Begin((Actions)m.Argument);
                    }
                }
                else if (m.Action == MapEvent.Actions.Position)
                {
                    if (m.Target == MapEvent.Targets.Creature)
                    {
                        PositionPayload positionPayload = (PositionPayload)m.Argument;
                        Creature creature = map.SortedCreatures.First(c => c.ID == m.ID);
                        Model.Bomberman bomberman = status.LocalPlayer.Bomberman;
                        if (creature != bomberman || bomberman.Dead)
                        {
                            creature.Position = positionPayload.DecodePosition();
                            creature.Velocity = positionPayload.DecodeVelocity();
                            creature.UpdateOrientation();
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("[CLIENT] Unsupported map action received: " + m.Action.ToString());
#if DEBUG
                    Debugger.Break();
#endif
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("[CLIENT] Encountered error: " + e);

                controller.Disconnect();
            }
        }

        public ClientMessageExecuter(ClientController controller)
        {
            if (controller != null)
            {
                this.controller = controller;
            }
            else
            {
                throw new ArgumentNullException("controller");
            }
        }
    }
}
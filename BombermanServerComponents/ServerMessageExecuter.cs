using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Bomberman.Model;
using Bomberman.Network;

namespace Bomberman.Server
{
    /// <summary>
    /// Server-side message executer.
    /// It executes MessageEvent received from a client.
    /// </summary>
    public class ServerMessageExecuter
    {
        private ServerController controller;

        /// <summary>
        /// Execute a message received from a client.
        /// </summary>
        public void Execute(int interfaceIndex, MessageEvent m)
        {
            Model.Game game = controller.Game;
            Status status = game.Status;
            Map map = game.Map;
            Player sender = status.Players[interfaceIndex];

            try
            {
                if (m.Type == MessageEvent.Types.Chat)
                {
                    controller.SendChatMessage(sender, (string)m.Payload);
                }
                else if (m.Type == MessageEvent.Types.Options)
                {
                    if (sender.Host)
                    {
                        controller.SetOptions((OptionsPayload)m.Payload);
                    }
                }
                else if (m.Type == MessageEvent.Types.PauseGame)
                {
                    // TODO: piste d'amélioration pour plus tard
                }
                else if (m.Type == MessageEvent.Types.StartGame)
                {
                    if (sender.Host)
                    {
                        controller.StartGame();
                    }
                }
                else if (m.Type == MessageEvent.Types.ContinueGame)
                {
                    controller.ContinueGame(sender.ID, (bool)m.Payload);
                }
                else if (m.Type == MessageEvent.Types.PlayerPosition)
                {
                    controller.Animator.MovePlayer(sender, (PositionPayload)m.Payload);
                }
                else if (m.Type == MessageEvent.Types.PlayerBomb)
                {
                    controller.Animator.PlaceBomb(sender, (Bomb.Types)m.Payload);
                }
                else if (m.Type == MessageEvent.Types.PlayerDetonate)
                {
                    controller.Animator.DetonateBomb(sender, (ushort)m.Payload);
                }
                else
                {
                    Debug.WriteLine("[SERVER] Unsupported game message received: " + m.Type.ToString());
#if DEBUG
                    Debugger.Break();
#endif
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("[SERVER] Encountered error: " + e);
#if DEBUG
                Debugger.Break();
#endif
                controller.Kick(sender, ReasonCodes.CheatKick);
            }
        }

        public ServerMessageExecuter(ServerController controller)
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
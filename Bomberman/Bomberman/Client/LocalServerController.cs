using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Bomberman.Network;

namespace Bomberman.Client
{
    /// <summary>
    /// A decorated ServerController to have a local server-side game controller.
    /// Used when debugging without a real server!
    /// </summary>
    public sealed class LocalServerController : Microsoft.Xna.Framework.GameComponent
    {
        /// <summary>
        /// The contained ServerController.
        /// </summary>
        public Bomberman.Server.ServerController ServerController { get; private set; }

        public void ReplaceInterface(int index, IMessageInterface newInterface)
        {
            ServerController.ReplaceInterface(index, newInterface);
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            ServerController.Update(gameTime.ElapsedGameTime);
        }

        public LocalServerController(Microsoft.Xna.Framework.Game game) : base(game)
        {
            ServerController = new Bomberman.Server.ServerController();

            game.Components.Add(this);
        }
    }
}
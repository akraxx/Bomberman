using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Bomberman.Client;
using Bomberman.Client.View;
using Bomberman.Model;

namespace Bomberman
{
    /// <summary>
    /// Type principal pour votre jeu
    /// </summary>
    public class BombermanGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        ClientController clientController;

        public BombermanGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 400;
            graphics.PreferredBackBufferHeight = 240;
            graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";

            // La fréquence d’image est de 30 i/s pour le Windows Phone.
            TargetElapsedTime = TimeSpan.FromTicks(333333);

            // Augmenter la durée de la batterie sous verrouillage.
            InactiveSleepTime = TimeSpan.FromSeconds(1);

            // Service creation
            this.Services.AddService(typeof(Services.IMusicPlayer), new Services.SimpleMusicPlayer(this));
            this.Services.AddService(typeof(Services.ISoundPlayer), new Services.SimpleSoundPlayer(this));
            this.Services.AddService(typeof(Services.IUserInput), new Services.SimpleUserInput(this));
            this.Services.AddService(typeof(Services.ISpriteDrawer), new Services.SpriteDrawer(this));
        }

        /// <summary>
        /// Permet au jeu d’effectuer l’initialisation nécessaire pour l’exécution.
        /// Il peut faire appel aux services et charger tout contenu
        /// non graphique. Calling base.Initialize énumère les composants
        /// et les initialise.
        /// </summary>
        protected override void Initialize()
        {
            // Scénario 1: LOCAL
            /*
            Network.LocalMessageInterface clientToServer = new Network.LocalMessageInterface();
            Network.LocalMessageInterface serverToClient = new Network.LocalMessageInterface();

            clientController = new ClientController(this) { Enabled = true };
            clientController.ReplaceInterface(clientToServer);

            LocalServerController serverController = new LocalServerController(this);
            serverController.ReplaceInterface(0, serverToClient);

            ViewManager viewManager = new ViewManager(this, clientController) { Enabled = true };
            ClientGameInput clientInput = new ClientGameInput(this, clientController.Game, viewManager.MapView, viewManager.WeaponView);
            clientController.ConnectInput(clientInput);
            Widgets.ScrollingBackground bg = new Widgets.ScrollingBackground(this) { Color = Color.White * 0.5f };

            Player player = new Player(0);
            player.Join("truc", true, false);
            serverController.ServerController.Join(player);
            clientToServer.Connect(serverToClient);
            serverToClient.Connect(clientToServer);
            */

            // Scénario 2 : CLIENT-SERVEUR

            clientController = new ClientController(this) { Enabled = false };
            ViewManager viewManager = new ViewManager(this, clientController) { Enabled = false };
            ClientGameInput clientInput = new ClientGameInput(this, clientController.Game, viewManager.MapView, viewManager.WeaponView);
            clientController.ConnectInput(clientInput);
            Widgets.ScrollingBackground bg = new Widgets.ScrollingBackground(this) { Color = Color.White * 0.5f };
            ClientLoginController clientLoginController = new ClientLoginController(this, clientController, viewManager);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            string[] preload = { "accept", "bomb1", "bomb2", "bomb3", "bossDead", "cleared", "dead", "extraLife", "gameOver", "pickup", "woosh" };
            Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Services.GetService(typeof(Services.ISoundPlayer));
            foreach (string s in preload)
            {
                soundPlayer.Preload(s);
            }
        }

        /// <summary>
        /// Permet au jeu d’exécuter la logique de mise à jour du monde,
        /// de vérifier les collisions, de gérer les entrées et de lire l’audio.
        /// </summary>
        /// <param name="gameTime">Fournit un aperçu des valeurs de temps.</param>
        protected override void Update(GameTime gameTime)
        {
            // Permet au jeu de se fermer
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        /// <summary>
        /// Appelé quand le jeu doit se dessiner.
        /// </summary>
        /// <param name="gameTime">Fournit un aperçu des valeurs de temps.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
        }
    }
}
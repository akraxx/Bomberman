using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// The screen for creating a game.
    /// </summary>
    class CreateGameMenu : Menu
    {
        public Textbox HostName { get; protected set; }
        public Textbox PlayerName { get; protected set; }
        public Textbox AccessToken { get; protected set; }
        public Button Create { get; protected set; }

        public CreateGameMenu(Game game) : base(game)
        {
            titleString = "Create Game";
            
            HostName = new Textbox(game)
            {
                Title = "Server Host",
                Description = "Enter here the host name of the server.",
                Text = "QUENTIN-PC",
                MaxSize = 20,
                Bounds = new Rectangle(48, 96, 128, 16),
            };

            PlayerName = new Textbox(game)
            {
                Title = "Nickname",
                Description = "Enter here your nickname.",
                Text = "Test",
                MaxSize = Network.LoginPayload.MaxNameLength,
                Bounds = new Rectangle(48, 160, 128, 16),
            };

            AccessToken = new Textbox(game)
            {
                Title = "Access Token",
                Description = "Enter here the code you will give to your friends that will be used to identify your game.",
                Text = "CIR3",
                MaxSize = Network.LoginPayload.MaxTokenLength,
                Bounds = new Rectangle(224, 96, 128, 16),
            };

            Create = new Button(game)
            {
                Text = "Connect & Create",
                Bounds = new Rectangle(224, 160, 128, 16),
            };

            Children.Add(HostName);
            Children.Add(PlayerName);
            Children.Add(AccessToken);
            Children.Add(Create);

            DrawOrder = Utilities.Order.StaticLevel;

            game.Components.Add(this);
        }
    }
}
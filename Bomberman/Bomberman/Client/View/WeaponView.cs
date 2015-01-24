using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Model;
using Bomberman.Utilities;

namespace Bomberman.Client.View
{
    public sealed class WeaponView : DrawableGameComponent
    {
        private const int elementPower = 0;
        private const int elementSimultanates = 1;
        private const int elementSplit = 2;
        private const int elementRemote = 3;
        private const int elementSelectNormal = 4;
        private const int elementSelectSplit = 5;
        private const int elementSelectRemote = 6;
        private static readonly Vector2[] elementPositions = { new Vector2(169, 19), new Vector2(202, 19), new Vector2(333, 19), new Vector2(377, 19), new Vector2(71, 8), new Vector2(95, 8), new Vector2(119, 8) };

        private Status status;
        private Bomb.Types equippedBomb;
        private SpriteBatch spriteBatch;
        private Texture2D baseTexture;
        private Texture2D weaponSelectTexture;
        private SpriteFont spriteFontArial9;

        /// <summary>
        /// The origin of this view.
        /// </summary>
        public Vector2 Origin { get; set; }

        private bool isSelectedWeapon(Vector2 mousePosition, Vector2 weaponSelectPosition)
        {
            return (mousePosition.X > (weaponSelectPosition.X + Origin.X) && mousePosition.X < (weaponSelectPosition.X + weaponSelectTexture.Width + Origin.X)
                && mousePosition.Y > (weaponSelectPosition.Y + Origin.Y) && mousePosition.Y < (weaponSelectPosition.Y + weaponSelectTexture.Height + Origin.Y));
        }

        /// <summary>
        /// The currently equipped bomb type in the interface.
        /// </summary>
        public Bomb.Types EquippedBomb
        {
            get { return equippedBomb; }

            set {
                if (value != equippedBomb)
                {
                    equippedBomb = value;
                    if (EquippedBombChanged != null) EquippedBombChanged(this, new EventArgs());
                }
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
            spriteFontArial9 = Game.Content.Load<SpriteFont>("arialBold9");
            
            baseTexture = Game.Content.Load<Texture2D>("gfx\\weapon\\base");
            weaponSelectTexture = Game.Content.Load<Texture2D>("gfx\\weapon\\weaponSelect");
            
        }

        public override void Update(GameTime gameTime)
        {
            Services.IUserInput userInput = (Services.IUserInput)Game.Services.GetService(typeof(Services.IUserInput));
            Player player = status.LocalPlayer;
            if (player != null)
            {
                Model.Bomberman bomberman = player.Bomberman;
                if (bomberman != null)
                {
                    if (userInput.MousePressed)
                    {
                        Vector2 mousePosition = userInput.MousePosition;
                        if(Bomb.Types.Normal != EquippedBomb && isSelectedWeapon(mousePosition, elementPositions[elementSelectNormal]))
                        {
                            EquippedBomb = Bomb.Types.Normal;
                        }
                        else if (Bomb.Types.Split != EquippedBomb && bomberman.GetBombType(Bomb.Types.Split) > 0 && isSelectedWeapon(mousePosition, elementPositions[elementSelectSplit]))
                        {
                            EquippedBomb = Bomb.Types.Split;
                        }
                        else if (Bomb.Types.Remote != EquippedBomb && bomberman.GetBombType(Bomb.Types.Remote) > 0 && isSelectedWeapon(mousePosition, elementPositions[elementSelectRemote]))
                        {
                            EquippedBomb = Bomb.Types.Remote;
                        }
                    }

                    if (bomberman.GetBombType(EquippedBomb) < 1)
                    {
                        EquippedBomb = Bomb.Types.Normal;
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Player player = status.LocalPlayer;
            if (player != null)
            {
                Model.Bomberman bomberman = player.Bomberman;
                if (bomberman != null)
                {
                    string simultaneousBombsText = bomberman.Bombs.ToString();
                    string powerText = bomberman.Power.ToString();
                    string splitStock = bomberman.GetBombType(Bomb.Types.Split).ToString();
                    string remoteStock = bomberman.GetBombType(Bomb.Types.Remote).ToString();

                    spriteBatch.Begin();
                    spriteBatch.Draw(baseTexture, Origin, Color.White);
                    Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, simultaneousBombsText, Origin + elementPositions[elementSimultanates], Color.Black, false);
                    Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, powerText, Origin + elementPositions[elementPower], Color.Black, false);
                    Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, splitStock, Origin + elementPositions[elementSplit], Color.Black, false);
                    Drawing.DrawCenteredText(spriteFontArial9, spriteBatch, remoteStock, Origin + elementPositions[elementRemote], Color.Black, false);
                    
                    if (EquippedBomb == Bomb.Types.Normal)
                    {
                        spriteBatch.Draw(weaponSelectTexture, Origin + elementPositions[elementSelectNormal], Color.White);
                    }
                    else if (EquippedBomb == Bomb.Types.Split)
                    {
                        spriteBatch.Draw(weaponSelectTexture, Origin + elementPositions[elementSelectSplit], Color.White);
                    }
                    else if (EquippedBomb == Bomb.Types.Remote)
                    {
                        spriteBatch.Draw(weaponSelectTexture, Origin + elementPositions[elementSelectRemote], Color.White);
                    }
                    
                    spriteBatch.End();
                }
            }
        }

        public WeaponView(Microsoft.Xna.Framework.Game game, Status status, Vector2 origin) : base(game)
        {
            if (status != null)
            {
                this.status = status;
                equippedBomb = Bomb.Types.Normal;
                Origin = origin;

                DrawOrder = Order.OverlayLevel;

                game.Components.Add(this);
            }
            else
            {
                throw new ArgumentNullException("status");
            }
        }

        // Events

        /// <summary>
        /// Fired when the equipped bomb was changed on the interface.
        /// </summary>
        public event EventHandler<EventArgs> EquippedBombChanged;
    }
}
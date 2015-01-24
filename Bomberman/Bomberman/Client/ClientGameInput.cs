using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Bomberman.Model;
using Bomberman.Client.View;

namespace Bomberman.Client
{
    /// <summary>
    /// Class that collects input from the player.
    /// </summary>
    public sealed class ClientGameInput : GameComponent
    {
        private Map map;
        private Status status;
        private MapView mapView;
        private WeaponView weaponView;
        private Dictionary<Vector2, DateTime> click = new Dictionary<Vector2, DateTime>();
        private Dictionary<int, DateTime> activeMouse = new Dictionary<int,DateTime>();

        private static double delayHold = 120;
        private static double delayTap = 200;
        private static double delayDoubleTap = 300;
        private static int NULL = 0;

        private int MouseWhichMove = NULL;

        /// <summary>
        /// Return true when the player is giving movement orders.
        /// </summary>
        public bool Moving { get; private set; }

        /// <summary>
        /// Return where the player wants to move.
        /// This property should not be used when Moving is false.
        /// </summary>
        public Vector2 Movement { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (TouchPanel.GetCapabilities().IsConnected)
            {
                TouchCollection collection = TouchPanel.GetState();
                foreach (TouchLocation location in collection)
                {
                    int id = location.Id;
                    DateTime now = DateTime.Now;

                    if (activeMouse.ContainsKey(id))
                    {
                        DateTime start = activeMouse[id];
                        if (location.State == TouchLocationState.Moved)
                        {
                            if ((MouseWhichMove == location.Id || MouseWhichMove == NULL) && DateTime.Now.Subtract(start).CompareTo(TimeSpan.FromMilliseconds(delayHold)) >= 0)
                            {
                                MouseWhichMove = location.Id;
                                Moving = true;
                                Movement = mapView.GetGamePosition(location.Position);
                            }
                        }
                        else if (location.State == TouchLocationState.Released)
                        {
                            if (id == MouseWhichMove) { MouseWhichMove = NULL; Moving = false; }
                            activeMouse.Remove(id);
                            if (DateTime.Now.Subtract(start).CompareTo(TimeSpan.FromMilliseconds(delayTap)) <= 0)
                            {
                                if (click.ContainsKey(location.Position))
                                {
                                    DateTime First = click[location.Position];
                                    if (DateTime.Now.Subtract(First).CompareTo(TimeSpan.FromMilliseconds(delayDoubleTap)) < 0)
                                    {
                                        click.Remove(location.Position);
                                        Vector2 position = mapView.GetGamePosition(location.Position);
                                        Point point = Tools.Vector2Point(CreatureMover.AlignPosition(position, true));
                                        Model.Object obj = map.GetObject(point);
                                        if (obj is Bomb)
                                        {
                                            Bomb bomb = (Bomb)obj;
                                            if (bomb.Owner == status.LocalPlayer.Bomberman.ID && bomb.Type == Bomb.Types.Remote)
                                            {
                                                if (RequestDetonate != null) RequestDetonate(this, new EventArgs<Bomb>(bomb));
                                            }
                                        }
                                        else
                                        {
                                            if (RequestBomb != null) RequestBomb(this, new EventArgs<Bomb.Types>(weaponView.EquippedBomb));
                                        }
                                    }
                                    else
                                    {
                                        click[location.Position] = DateTime.Now;
                                    }
                                }
                                else
                                {
                                    click.Add(location.Position, DateTime.Now);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (location.State == TouchLocationState.Pressed)
                        {
                            activeMouse.Add(location.Id, DateTime.Now);
                        }
                    }
                }
            }
        }

        public ClientGameInput(Microsoft.Xna.Framework.Game game, Model.Game g, MapView mapView, WeaponView weaponView) : base(game)
        {
            this.map = g.Map;
            this.status = g.Status;
            this.mapView = mapView;
            this.weaponView = weaponView;

            game.Components.Add(this);
        }

        // Events

        /// <summary>
        /// Fired when the player wants to place a bomb of the specified type.
        /// </summary>
        public event EventHandler<EventArgs<Bomb.Types>> RequestBomb;

        /// <summary>
        /// Fired when the player wants to detonate the specified bomb.
        /// </summary>
        public event EventHandler<EventArgs<Bomb>> RequestDetonate;
    }
}

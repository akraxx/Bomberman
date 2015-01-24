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
    public sealed class MapView : DrawableGameComponent
    {
        private static readonly TimeSpan blinkCycle = TimeSpan.FromSeconds(0.1);
        private static readonly TimeSpan blinkMaxTimer = TimeSpan.FromSeconds(10.0);

        private int tileWidth;
        private int tileHeight;
        private TimeSpan blinkTimer = TimeSpan.Zero;
        private Random random = new Random();

        private SpriteBatch spriteBatch;

        private Map map;

        private Vector2 getScreenPosition(Vector2 v)
        {
            return Origin + new Vector2((v.X + 0.5f) * tileWidth, (v.Y + 0.5f) * tileHeight);
        }

        private Vector2 getScreenPosition(Point p)
        {
            return getScreenPosition(Tools.Point2Vector(p));
        }

        private float GetBlinkAlpha(TimeSpan timeLeft)
        {
            float progression = (float)Math.Min(1.0, timeLeft.TotalSeconds / blinkMaxTimer.TotalSeconds);
            float duty = MathHelper.Lerp(0.5f, 1.0f, progression);
            float value = (float)(blinkTimer.TotalSeconds % blinkCycle.TotalSeconds);
            return value < (blinkCycle.TotalSeconds * duty) ? 1.0f : 0.0f;
        }

        private float GetRenderAlpha(Model.Object o)
        {
            if (o is Bonus || o is PowerUp)
            {
                TimedObject obj = (TimedObject)o;
                return this.GetBlinkAlpha(obj.Timer);
            }
            return 1.0f;
        }

        private float GetRenderAlpha(Creature c)
        {
            return c.Invulnerability > TimeSpan.Zero ? this.GetBlinkAlpha(c.Invulnerability) : 1.0f;
        }

        /// <summary>
        /// The origin of this view.
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// Map an object logical position to screen position.
        /// </summary>
        public Vector2 GetScreenPosition(Model.Object obj)
        {
            if (obj != null)
            {
                return this.getScreenPosition(obj.Position);
            }
            else
            {
                throw new ArgumentNullException("obj");
            }
        }

        /// <summary>
        /// Map a creature logical position to screen position.
        /// </summary>
        public Vector2 GetScreenPosition(Model.Creature creature)
        {
            if (creature != null)
            {
                return this.getScreenPosition(creature.Position);
            }
            else
            {
                throw new ArgumentNullException("creature");
            }
        }

        /// <summary>
        /// Map a screen position to a logical position.
        /// </summary>
        public Vector2 GetGamePosition(Vector2 v)
        {
            if (tileWidth > 0 && tileHeight > 0)
            {
                return (v - Origin) / new Vector2(tileWidth, tileHeight) - new Vector2(0.5f, 0.5f);
            }
            else
            {
                return Vector2.Zero;
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(Game.GraphicsDevice);
        }

        public override void Draw(GameTime gameTime)
        {
            Services.ISpriteDrawer spriteDrawer = (Services.ISpriteDrawer)Game.Services.GetService(typeof(Services.ISpriteDrawer));
            GridGraphics groundGraphics = spriteDrawer.GroundGraphics;
            tileWidth = groundGraphics.TileWidth;
            tileHeight = groundGraphics.TileHeight;
            blinkTimer += gameTime.ElapsedGameTime;

            spriteBatch.Begin();
            // Phase 1: ground
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    spriteDrawer.DrawGround(spriteBatch, map.Theme, this.getScreenPosition(new Point(x, y)), Color.White);
                }
            }
            // Phase 2: objects
            foreach (Wall w in map.Walls)
            {
                // Awesome shadow for walls!
                spriteDrawer.DrawObject(spriteBatch, w, this.GetScreenPosition(w) + new Vector2(2, 2), Color.Black * 0.7f);
            }
            foreach(Model.Object o in map.Objects)
            {
                spriteDrawer.DrawObject(spriteBatch, o, this.GetScreenPosition(o), Color.White * this.GetRenderAlpha(o));
            }
            // Phase 3: creatures
            foreach (Creature c in map.SortedCreatures)
            {
                spriteDrawer.DrawCreature(spriteBatch, c, this.GetScreenPosition(c), Color.White * this.GetRenderAlpha(c), 0.0f, Vector2.One);
            }
            spriteBatch.End();
        }

        public MapView(Microsoft.Xna.Framework.Game game, ClientController controller, Vector2 origin) : base(game)
        {
            if (controller != null)
            {
                this.map = controller.Game.Map;
                Origin = origin;

                DrawOrder = Order.StaticLevel;

                controller.ObjectDestroyed += new EventHandler<EventArgs<Model.Object>>(Controller_ObjectDestroyed);
                controller.ObjectPicked += new EventHandler<EventArgs<Model.Object>>(Controller_ObjectPicked);
                controller.CreatureHealthChanged += new EventHandler<EventArgs<Creature>>(Controller_CreatureHealthChanged);
                controller.TimeUp += new EventHandler<EventArgs>(Controller_TimeUp);

                game.Components.Add(this);
            }
            else
            {
                throw new ArgumentNullException("controller");
            }
        }

        // Event handlers

        private void Controller_ObjectPicked(object sender, EventArgs<Model.Object> e)
        {
            if (Visible)
            {
                Model.Object obj = e.Value;
                string sound = (obj is PowerUp && ((PowerUp)obj).Type == PowerUp.Types.ExtraLife) ? "extraLife" : "pickup";

                Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Game.Services.GetService(typeof(Services.ISoundPlayer));
                soundPlayer.Play(sound, this.GetScreenPosition(obj));
            }
        }

        private void Controller_ObjectDestroyed(object sender, EventArgs<Model.Object> e)
        {
            if (Visible)
            {
                Model.Object obj = e.Value;
                if (obj is Bomb)
                {
                    Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Game.Services.GetService(typeof(Services.ISoundPlayer));
                    soundPlayer.Play("bomb" + random.Next(1, 3), this.GetScreenPosition(obj));
                }
            }
        }

        private void Controller_CreatureHealthChanged(object sender, EventArgs<Creature> e)
        {
            if (Visible)
            {
                Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Game.Services.GetService(typeof(Services.ISoundPlayer));
                Creature creature = e.Value;
                if (creature is Model.Bomberman)
                {
                    soundPlayer.Play("dead");
                }
                else if (creature is Monster)
                {
                    //soundPlayer.Play("bossDead"); // TODO: trouver le boss
                }
            }
        }

        private void Controller_TimeUp(object sender, EventArgs e)
        {
            if (Visible)
            {
                Services.ISoundPlayer soundPlayer = (Services.ISoundPlayer)Game.Services.GetService(typeof(Services.ISoundPlayer));
                soundPlayer.Play("bomb1");
            }
        }
    }
}
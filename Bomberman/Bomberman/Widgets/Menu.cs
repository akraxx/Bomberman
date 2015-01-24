using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Widgets
{
    /// <summary>
    /// Base class of menu components.
    /// </summary>
    abstract class Menu : DrawableGameComponent
    {
        protected string titleString = "My Menu";
        protected Vector2 titlePosition = new Vector2(200, 24);

        protected List<DrawableGameComponent> Children = new List<DrawableGameComponent>();

        protected SpriteBatch spriteBatch;
        protected SpriteFont spriteFont;

        public Menu(Game game) : base(game)
        {
            EnabledChanged += new EventHandler<EventArgs>(Menu_EnabledChanged);
            VisibleChanged += new EventHandler<EventArgs>(Menu_VisibleChanged);
            DrawOrderChanged += new EventHandler<EventArgs>(Menu_DrawOrderChanged);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();
            Utilities.Drawing.DrawShadowedText(spriteFont, spriteBatch, titleString, titlePosition, spriteFont.MeasureString(titleString) / 2, Color.White);
            spriteBatch.End();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Game.Content.Load<SpriteFont>("default");
        }

        private void Menu_EnabledChanged(object sender, EventArgs e)
        {
            foreach (DrawableGameComponent c in Children)
            {
                c.Enabled = Enabled;
            }
        }

        private void Menu_VisibleChanged(object sender, EventArgs e)
        {
            foreach (DrawableGameComponent c in Children)
            {
                c.Visible = Visible;
            }
        }

        private void Menu_DrawOrderChanged(object sender, EventArgs e)
        {
            int count = 0;
            foreach (DrawableGameComponent c in Children)
            {
                count++;
                c.DrawOrder = DrawOrder + count;
            }
        }
    }
}
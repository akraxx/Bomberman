using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman.Model;
using Bomberman.Utilities;

namespace Bomberman.Services
{
    class NullSpriteDrawer : ISpriteDrawer
    {
        private GridGraphics groundGraphics = new GridGraphics("null", 0, 0);

        public GridGraphics GroundGraphics { get { return groundGraphics; } }

        public void DrawGround(SpriteBatch spriteBatch, int theme, Vector2 position, Color color) { }
        public void DrawObject(SpriteBatch spriteBatch, Bomberman.Model.Object obj, Vector2 position, Color color) { }
        public void DrawCreature(SpriteBatch spriteBatch, Creature creature, Vector2 position, Color color, float rotation, Vector2 scale) { }
    }
}
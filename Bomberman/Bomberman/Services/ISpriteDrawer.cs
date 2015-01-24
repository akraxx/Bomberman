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
    interface ISpriteDrawer
    {
        GridGraphics GroundGraphics { get; }
        void DrawGround(SpriteBatch spriteBatch, int theme, Vector2 position, Color color);
        void DrawObject(SpriteBatch spriteBatch, Bomberman.Model.Object obj, Vector2 position, Color color);
        void DrawCreature(SpriteBatch spriteBatch, Creature creature, Vector2 position, Color color, float rotation, Vector2 scale);
    }
}
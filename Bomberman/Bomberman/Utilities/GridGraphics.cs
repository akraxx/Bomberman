using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Describes the arrangement of a grid PNG sprite file.
    /// </summary>
    class GridGraphics
    {
        /// <summary>
        /// Name of the grid graphics.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The width of a tile in the PNG sprite file.
        /// </summary>
        public int TileWidth { get; private set; }

        /// <summary>
        /// The height of a tile in the PNG sprite file.
        /// </summary>
        public int TileHeight { get; private set; }

        /// <summary>
        /// Global origin that should be applied to the sprite.
        /// </summary>
        public Vector2 Origin { get; set; }

        /// <summary>
        /// Get a rectangle appropriate to extract and display the provided tile of the grid.
        /// </summary>
        public Rectangle GetGridRectangle(Point tile)
        {
            return new Rectangle(tile.X * TileWidth, tile.Y * TileHeight, TileWidth, TileHeight);
        }

        /// <summary>
        /// Load the PNG sprite file of this grid graphics.
        /// </summary>
        public virtual Texture2D LoadTexture(ContentManager contentManager)
        {
            return contentManager.Load<Texture2D>("gfx\\tiles\\" + Name);
        }

        public GridGraphics(string name, int tileWidth, int tileHeight)
        {
            if (name != null)
            {
                Name = name;
                TileWidth = tileWidth;
                TileHeight = tileHeight;
                Origin = Vector2.Zero;
            }
            else
            {
                throw new ArgumentNullException("name");
            }
        }
    }
}
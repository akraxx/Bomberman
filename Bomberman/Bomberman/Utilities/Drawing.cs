using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Some commonly used drawing routines.
    /// </summary>
    static class Drawing
    {
        public static void DrawShadowedText(SpriteFont spriteFont, SpriteBatch spriteBatch, string text, Vector2 position, Vector2 origin)
        {
            Drawing.DrawShadowedText(spriteFont, spriteBatch, text, position, origin, Color.White);
        }

        public static void DrawShadowedText(SpriteFont spriteFont, SpriteBatch spriteBatch, string text, Vector2 position, Vector2 origin, Color color)
        {
            spriteBatch.DrawString(spriteFont, text, position + new Vector2(2, 2), new Color(0, 0, 0, color.A), 0.0f, origin, 1.0f, SpriteEffects.None, 0);
            spriteBatch.DrawString(spriteFont, text, position, color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        public static void DrawCenteredText(SpriteFont spriteFont, SpriteBatch spriteBatch, string text, Vector2 position, Color color, bool shadowed)
        {
            Vector2 halfSize = spriteFont.MeasureString(text) / 2;
            if (shadowed)
            {
                Drawing.DrawShadowedText(spriteFont, spriteBatch, text, position, halfSize, color);
            }
            else
            {
                spriteBatch.DrawString(spriteFont, text, position, color, 0.0f, halfSize, 1.0f, SpriteEffects.None, 0.0f);
            }
        }
    }
}

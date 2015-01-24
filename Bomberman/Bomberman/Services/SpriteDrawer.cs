using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Bomberman.Model;
using Bomberman.Utilities;

namespace Bomberman.Services
{
    class SpriteDrawer : DrawableGameComponent, ISpriteDrawer
    {
        private static readonly TimeSpan powerUpBlinkRate = TimeSpan.FromSeconds(0.1);
        private static readonly TimeSpan bombAnimationRate = TimeSpan.FromSeconds(0.3);
        private static readonly int[] blastFrames = { 0, 1, 2, 3, 4, 4, 3, 2, 1, 0 };

        private TimeSpan animationTimer = TimeSpan.Zero;

        private GridGraphics blastGraphics = new GridGraphics("blast", 16, 16) { Origin = new Vector2(8, 8) };
        private GridGraphics bombGraphics = new GridGraphics("bomb", 16, 16) { Origin = new Vector2(8, 8) };
        private GridGraphics bonusGraphics = new GridGraphics("bonus", 32, 32) { Origin = new Vector2(16, 24) };
        private GridGraphics powerUpGraphics = new GridGraphics("powerUp", 16, 16) { Origin = new Vector2(8, 8) };
        private GridGraphics spawnGraphics = new GridGraphics("spawn", 16, 16) { Origin = new Vector2(8, 8) };
        private GridGraphics wallGraphics = new GridGraphics("wall", 16, 16) { Origin = new Vector2(8, 8) };
        private GridGraphics groundGraphics = new GridGraphics("ground", 16, 16) { Origin = new Vector2(8, 8) };

        public GridGraphics GroundGraphics { get { return groundGraphics; } }

        public SpriteDrawer(Microsoft.Xna.Framework.Game game) : base(game)
        {
            CreatureGraphics.Initialize();

            game.Components.Add(this);
        }

        public void DrawGround(SpriteBatch spriteBatch, int theme, Vector2 position, Color color)
        {
            Texture2D texture = groundGraphics.LoadTexture(Game.Content);
            Vector2 origin = groundGraphics.Origin;
            Rectangle rect = groundGraphics.GetGridRectangle(new Point(0, theme));

            spriteBatch.Draw(texture, position, rect, color, 0.0f, origin, Vector2.One, SpriteEffects.None, 1.0f);
        }

        public void DrawObject(SpriteBatch spriteBatch, Bomberman.Model.Object obj, Vector2 position, Color color)
        {
            if (obj is Blast)
            {
                Blast blast = (Blast)obj;
                int col = blastFrames[(int)((1.0f - blast.Progression) * (blastFrames.Length - 1))];
                int row = (int)blast.Type;
                SpriteEffects effects = SpriteEffects.None;
                if (blast.Type == Blast.Types.Edge)
                {
                    row += ((int)blast.Orientation) / 2;
                    if (blast.Orientation == Orientations.Right) effects = SpriteEffects.FlipHorizontally;
                    if (blast.Orientation == Orientations.Top) effects = SpriteEffects.FlipVertically;
                }
                Texture2D texture = blastGraphics.LoadTexture(Game.Content);
                Rectangle rect = blastGraphics.GetGridRectangle(new Point(col, row));

                spriteBatch.Draw(texture, position, rect, color, 0.0f, blastGraphics.Origin, Vector2.One, effects, 1.0f);
            }
            else if (obj is Bomb)
            {
                int col = (int)(animationTimer.TotalSeconds / bombAnimationRate.TotalSeconds) % 4;
                Bomb bomb = (Bomb)obj;
                Texture2D texture = bombGraphics.LoadTexture(Game.Content);
                Rectangle rect = bombGraphics.GetGridRectangle(new Point(col, (int)bomb.Type));

                spriteBatch.Draw(texture, position, rect, color, 0.0f, bombGraphics.Origin, Vector2.One, SpriteEffects.None, 1.0f);
            }
            else if (obj is Bonus)
            {
                Bonus bonus = (Bonus)obj;
                Texture2D texture = bonusGraphics.LoadTexture(Game.Content);
                Rectangle rect = bonusGraphics.GetGridRectangle(new Point(0, bonus.SpriteIndex));

                spriteBatch.Draw(texture, position, rect, color, 0.0f, bonusGraphics.Origin, Vector2.One, SpriteEffects.None, 1.0f);
            }
            else if (obj is PowerUp)
            {
                double mod = animationTimer.TotalSeconds % (2.0f * powerUpBlinkRate.TotalSeconds);
                int col = (mod > powerUpBlinkRate.TotalSeconds) ? 1 : 0;
                PowerUp powerUp = (PowerUp)obj;
                Texture2D texture = powerUpGraphics.LoadTexture(Game.Content);
                Rectangle rect = powerUpGraphics.GetGridRectangle(new Point(col, (int)powerUp.Type));

                spriteBatch.Draw(texture, position, rect, color, 0.0f, powerUpGraphics.Origin, Vector2.One, SpriteEffects.None, 1.0f);
            }
            else if (obj is SpawnPoint)
            {
                Texture2D texture = spawnGraphics.LoadTexture(Game.Content);
                Rectangle rect = spawnGraphics.GetGridRectangle(new Point(0, 0));

                spriteBatch.Draw(texture, position, rect, color, 0.0f, spawnGraphics.Origin, Vector2.One, SpriteEffects.None, 1.0f);
            }
            else if (obj is Wall)
            {
                Wall wall = (Wall)obj;
                int col = wall.Destroying ? (int)MathHelper.Lerp(1, 15, 1.0f - wall.Progression) : 0;
                Texture2D texture = wallGraphics.LoadTexture(Game.Content);
                Rectangle rect = wallGraphics.GetGridRectangle(new Point(col, wall.Destructible ? 0 : 1));

                spriteBatch.Draw(texture, position, rect, color, 0.0f, wallGraphics.Origin, Vector2.One, SpriteEffects.None, 1.0f);
            }
            else
            {
                throw new InvalidOperationException("Unsupported object");
            }
        }

        public void DrawCreature(SpriteBatch spriteBatch, Creature creature, Vector2 position, Color color, float rotation, Vector2 scale)
        {
            string name = creature.Name;
            SpriteState spriteState = creature.SpriteState;

            if (CreatureGraphics.Predefined.ContainsKey(name))
            {
                CreatureGraphics graphics = CreatureGraphics.Predefined[name];
                AnimationData animData = graphics.GetAnimationData(spriteState.Action);

                if (animData != null)
                {
                    Texture2D texture = graphics.LoadTexture(Game.Content);
                    Vector2 origin = graphics.Origin;
                    Point tile = animData.GetTile(spriteState.Frame, spriteState.Orientation);
                    Rectangle rect = graphics.GetGridRectangle(tile);
                    SpriteEffects effects = animData.GetEffects(spriteState.Orientation);

                    spriteBatch.Draw(texture, position, rect, color, rotation, origin, scale, effects, 1.0f);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported animation for this creature");
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported creature");
            }
        }

        protected override void LoadContent()
        {
            // Preload all graphics

            blastGraphics.LoadTexture(Game.Content);
            bombGraphics.LoadTexture(Game.Content);
            bonusGraphics.LoadTexture(Game.Content);
            powerUpGraphics.LoadTexture(Game.Content);
            spawnGraphics.LoadTexture(Game.Content);
            groundGraphics.LoadTexture(Game.Content);
            Texture2D wallTexture = wallGraphics.LoadTexture(Game.Content);

            foreach (KeyValuePair<string, CreatureGraphics> p in CreatureGraphics.Predefined)
            {
                p.Value.LoadTexture(Game.Content);
            }

            // Procedural generation of extra wall graphics

            ProceduralMelter m = new ProceduralMelter(wallTexture.Width / wallGraphics.TileWidth - 1);
            m.Generate(wallTexture, wallGraphics);
        }

        public override void Draw(GameTime gameTime)
        {
            animationTimer += gameTime.ElapsedGameTime;
        }
    }
}
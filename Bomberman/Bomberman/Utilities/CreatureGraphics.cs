using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Bomberman.Model;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Describes the arrangement of a creature PNG sprite file.
    /// </summary>
    class CreatureGraphics : GridGraphics
    {
        private static readonly Vector2 DefaultOrigin = new Vector2(0, 0);

        private Dictionary<Actions, AnimationData> animations;

        /// <summary>
        /// Set the animation data for the specified action.
        /// </summary>
        public void SetAnimationData(Actions action, AnimationData animationData)
        {
            animations[action] = animationData;
        }

        /// <summary>
        /// Get the animation data for the specified action.
        /// </summary>
        public AnimationData GetAnimationData(Actions action)
        {
            return animations[action];
        }

        /// <summary>
        /// Load the PNG sprite file of this creature graphics.
        /// </summary>
        public override Texture2D LoadTexture(ContentManager contentManager)
        {
            return contentManager.Load<Texture2D>("gfx\\creatures\\" + Name);
        }

        public CreatureGraphics(string name, int tileWidth, int tileHeight)
            : base(name, tileWidth, tileHeight)
        {
            animations = new Dictionary<Actions, AnimationData>();
            Origin = DefaultOrigin;
        }

        /// <summary>
        /// In this dictionary are stored available character graphics. Initialize should be called once before using it.
        /// </summary>
        public static readonly Dictionary<string, CreatureGraphics> Predefined = new Dictionary<string, CreatureGraphics>();

        /// <summary>
        /// Initialize the available character graphics. Should be called once at the start of the program.
        /// </summary>
        public static void Initialize()
        {
            if (Predefined.Count > 0) return;

            int[] sequence_2010 = new int[4] { 2, 0, 1, 0 };
            int[] sequence_0121 = new int[4] { 0, 1, 2, 1 };

            // Bomberman
            for (int i = 1; i <= 4; i++)
            {
                CreatureGraphics bomberman = new CreatureGraphics("bomberman" + i, 32, 32) { Origin = new Vector2(16, 26) };
                AnimationData idle = new AnimationData(OrientationSets.FullFlipHorizontal, 1) { StartPoint = new Point(0, 0) };
                AnimationData walk = new AnimationData(OrientationSets.FullFlipHorizontal, sequence_2010) { Loop = true, Rate = 4, StartPoint = new Point(0, 0) };
                AnimationData death = new AnimationData(OrientationSets.BottomOnly, 3) { Loop = false, Rate = 6, StartPoint = new Point(0, 3) };
                bomberman.SetAnimationData(Actions.Idle, idle);
                bomberman.SetAnimationData(Actions.Walk, walk);
                bomberman.SetAnimationData(Actions.Hit, death);
                bomberman.SetAnimationData(Actions.Burn, death);
                Predefined.Add(bomberman.Name, bomberman);
            }

            // Zombie
            {
                CreatureGraphics zombie = new CreatureGraphics("zombie", 32, 32) { Origin = new Vector2(16, 26) };
                AnimationData idle = new AnimationData(OrientationSets.FullFlipHorizontal, 1) { StartPoint = new Point(0, 0) };
                AnimationData walk = new AnimationData(OrientationSets.FullFlipHorizontal, 2) { Loop = true, Rate = 8, StartPoint = new Point(0, 0) };
                AnimationData burn = new AnimationData(OrientationSets.BottomOnly, 4) { Loop = false, Rate = 6, StartPoint = new Point(0, 4) };
                AnimationData special = new AnimationData(OrientationSets.FullFlipHorizontal, 4) { Loop = false, Rate = 6, StartPoint = new Point(0, 3) };
                zombie.SetAnimationData(Actions.Idle, idle);
                zombie.SetAnimationData(Actions.Walk, walk);
                zombie.SetAnimationData(Actions.Hit, burn);
                zombie.SetAnimationData(Actions.Burn, burn);
                zombie.SetAnimationData(Actions.Special, special);
                Predefined.Add(zombie.Name, zombie);
            }

            // Mummy
            {
                CreatureGraphics mummy = new CreatureGraphics("mummy", 32, 32) { Origin = new Vector2(16, 26) };
                AnimationData idle = new AnimationData(OrientationSets.FullFlipHorizontal, 1) { StartPoint = new Point(0, 0) };
                AnimationData walk = new AnimationData(OrientationSets.FullFlipHorizontal, 2) { Loop = true, Rate = 6, StartPoint = new Point(0, 0) };
                AnimationData burn = new AnimationData(OrientationSets.BottomOnly, 1) { StartPoint = new Point(0, 3) };
                mummy.SetAnimationData(Actions.Idle, idle);
                mummy.SetAnimationData(Actions.Walk, walk);
                mummy.SetAnimationData(Actions.Hit, burn);
                mummy.SetAnimationData(Actions.Burn, burn);
                Predefined.Add(mummy.Name, mummy);
            }

            // Chicken of doom
            {
                CreatureGraphics chicken = new CreatureGraphics("chicken", 40, 40) { Origin = new Vector2(19, 34) };
                AnimationData idle = new AnimationData(OrientationSets.FullFlipHorizontal, 1) { StartPoint = new Point(1, 0) };
                AnimationData walk = new AnimationData(OrientationSets.FullFlipHorizontal, sequence_0121) { Loop = true, Rate = 4, StartPoint = new Point(0, 0) };
                AnimationData run = new AnimationData(OrientationSets.FullFlipHorizontal, sequence_0121) { Loop = true, Rate = 2, StartPoint = new Point(3, 0) };
                AnimationData stun = new AnimationData(OrientationSets.BottomOnly, 2) { Loop = true, Rate = 2, StartPoint = new Point(3, 3) };
                AnimationData burn = new AnimationData(OrientationSets.BottomOnly, 2) { Loop = true, Rate = 2, StartPoint = new Point(3, 4) };
                AnimationData special = new AnimationData(OrientationSets.FullFlipHorizontal, 1) { StartPoint = new Point(0, 3) };
                chicken.SetAnimationData(Actions.Idle, idle);
                chicken.SetAnimationData(Actions.Walk, walk);
                chicken.SetAnimationData(Actions.Run, run);
                chicken.SetAnimationData(Actions.Hit, stun);
                chicken.SetAnimationData(Actions.Burn, burn);
                chicken.SetAnimationData(Actions.Stun, stun);
                chicken.SetAnimationData(Actions.Special, special);
                Predefined.Add(chicken.Name, chicken);
            }

            // TODO: piste d'amélioration pour plus tard : ajouter des ennemis
        }
    }
}
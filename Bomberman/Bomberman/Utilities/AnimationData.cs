using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Bomberman.Model;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Describes the arrangement of an animation within a PNG sprite file.
    /// </summary>
    class AnimationData
    {
        private OrientationSets orientations;
        private int[] sequence;

        private Orientations GetEffectiveOrientation(Orientations orientation)
        {
            if (orientation == Orientations.Right)
            {
                if (orientations == OrientationSets.FullFlipBoth || orientations == OrientationSets.FullFlipHorizontal)
                {
                    orientation = Orientations.Left;
                }
            }
            else if (orientation == Orientations.Top)
            {
                if (orientations == OrientationSets.FullFlipBoth)
                {
                    orientation = Orientations.Bottom;
                }
            }
            return this.IsOrientationSupported(orientation) ? orientation : Orientations.Bottom;
        }

        private int GetEffectiveImage(int frame)
        {
            int imageShown = frame / Rate;
            if (Loop)
            {
                imageShown = imageShown % Count;
            }
            else
            {
                int max = Count - 1;
                if (imageShown > max) imageShown = max;
            }
            return sequence[imageShown];
        }

        /// <summary>
        /// The number of images making up the animation sequence.
        /// Some images can be duplicated in the sequence.
        /// </summary>
        public int Count { get { return sequence.Length; } }

        /// <summary>
        /// The number of animation frames before showing the next image.
        /// </summary>
        public int Rate { get; set; }

        /// <summary>
        /// Specify if the animation should be looped when completed once.
        /// </summary>
        public bool Loop { get; set; }

        /// <summary>
        /// The start point of the animation in the grid.
        /// </summary>
        public Point StartPoint { get; set; }

        /// <summary>
        /// Check if the specified orientation is supported in the animation.
        /// </summary>
        public bool IsOrientationSupported(Orientations orientation)
        {
            return !(orientations == OrientationSets.BottomOnly && orientation != Orientations.Bottom);
        }

        /// <summary>
        /// Return the appropriate tile to display from the sprite grid according to the provided parameters.
        /// </summary>
        public Point GetTile(int frame, Orientations orientation)
        {
            int effectiveFrame = this.GetEffectiveImage(frame);
            Orientations effectiveOrientation = this.GetEffectiveOrientation(orientation);
            return new Point(StartPoint.X + effectiveFrame, StartPoint.Y + (int)effectiveOrientation);
        }

        /// <summary>
        /// Get the sprite effects to apply to the provided orientation.
        /// </summary>
        public SpriteEffects GetEffects(Orientations orientation)
        {
            if (orientation == Orientations.Right)
            {
                if (orientations == OrientationSets.FullFlipBoth || orientations == OrientationSets.FullFlipHorizontal)
                {
                    return SpriteEffects.FlipHorizontally;
                }
            }
            else if (orientation == Orientations.Top)
            {
                if (orientations == OrientationSets.FullFlipBoth)
                {
                    return SpriteEffects.FlipVertically;
                }
            }
            return SpriteEffects.None;
        }

        private void _construct(OrientationSets orientationSet, int[] seq)
        {
            orientations = orientationSet;
            sequence = seq;
            Rate = 1;
        }

        public AnimationData(OrientationSets orientationSet, int count)
        {
            int[] sequence = new int[count];
            for (int i = 0; i < count; i++)
            {
                sequence[i] = i;
            }
            _construct(orientationSet, sequence);
        }

        public AnimationData(OrientationSets orientationSet, int[] sequence)
        {
            if (sequence != null)
            {
                _construct(orientationSet, sequence);
            }
            else
            {
                throw new ArgumentNullException("sequence");
            }
        }
    }
}
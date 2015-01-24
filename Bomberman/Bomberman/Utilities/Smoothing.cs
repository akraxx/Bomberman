using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Bomberman.Utilities
{
    static class Smoothing
    {
        /// <summary>
        /// Smoothens with an exponential that is slow toward the end.
        /// </summary>
        /// <param name="progression">Value to smoothen, between 0 and 1.</param>
        public static float IncreasingExponential(float progression)
        {
            progression = 1.0f - (float)Math.Exp(-5.0f * progression);
            if (progression > 0.80f) {
                progression = progression + (1.0f - progression) * (progression - 0.80f) / 0.20f;
            }
            return progression;
        }

        /// <summary>
        /// Smoothens with an exponential that is slow toward the start.
        /// </summary>
        /// <param name="progression">Value to smoothen, between 0 and 1.</param>
        public static float DecreasingExponential(float progression)
        {
            progression = (float)Math.Exp(-5.0f * (1.0f - progression));
            if(progression < 0.20f) {
                progression = progression * progression / 0.20f;
            }
            return progression;
        }

        /// <summary>
        /// Interpolate a Vector2 between start and finish.
        /// </summary>
        public static Vector2 InterpolateVector(Vector2 start, Vector2 finish, float progression)
        {
            return new Vector2(MathHelper.Lerp(start.X, finish.X, progression), MathHelper.Lerp(start.Y, finish.Y, progression));
        }
    }
}
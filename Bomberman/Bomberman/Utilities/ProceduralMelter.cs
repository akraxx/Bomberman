using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Bomberman.Model;

namespace Bomberman.Utilities
{
    /// <summary>
    /// Generates extra version of a texture with bright orange and progressive disintegration.
    /// </summary>
    class ProceduralMelter
    {
        private int iterations;

        /// <summary>
        /// Construct a new instance of ProceduralMelter.
        /// </summary>
        /// <param name="numberOfIterations">The number of graphics to generate.</param>
        public ProceduralMelter(int numberOfIterations)
        {
            iterations = numberOfIterations;
        }

        /// <summary>
        /// Generate extra graphics for the provided grid texture.
        /// The reference graphics should be placed in the first column of the grid.
        /// </summary>
        /// <param name="t">The texture that will receive the extra graphics.</param>
        /// <param name="g">The GridGraphics that describes the grid in the texture.</param>
        public void Generate(Texture2D t, GridGraphics g)
        {
            // @Caron: dans t je t'envoie le fichier wall.png et dans g je dis que le fichier utilise une grille de 16x16

            int w = g.TileWidth; // taille d'une case horizontale
            int h = g.TileHeight; // taille d'une case verticale
            int nbCases = t.Height / h;
            Color[] colors = new Color[16*16];


            // Pour chaque Mur
            for (int y = 0; y < nbCases; y++)
            {
                t.GetData(0, new Rectangle(0, h * y, w, h), colors, 0, w * h);

                //Pour chaque case à faire
                for (int i = 0; i < iterations; i++)
                {
                    t.SetData(0, new Rectangle((i+1) * w, y * h, w, h), desintegrer(voiler(colors, Color.Orange), Math.Pow(i / (double) iterations, 2)), 0, w * h);
                }
            }
        }

        private Color[] voiler(Color[] colors, Color voil)
        {
            Color[] result = new Color[colors.Length];

            for (int i = 0; i < colors.Length;i++)
            {
                Color color = colors[i];
                result[i] = new Color((color.R + voil.R) / 2, (color.G + voil.G) / 2, (color.B + voil.B) / 2);
            }

            return result;
        }

        private Color[] desintegrer(Color[] colors, double percent)
        {
            Random rand = new Random();
            Color[] result = new Color[colors.Length];

            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];
                bool visible = rand.NextDouble() > percent;
                result[i] = visible ? new Color(color.R, color.G, color.B) : Color.Transparent;
            }

            return result;
        }
    }
}
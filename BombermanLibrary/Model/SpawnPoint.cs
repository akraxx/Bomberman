using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Bomberman.Model
{
    /// <summary>
    /// Represents a map spawn point.
    /// </summary>
    public class SpawnPoint : Object
    {
        public SpawnPoint(Point position) : base(position) { }
    }
}
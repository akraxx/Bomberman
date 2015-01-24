using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Bomberman.Services
{
    class NullUserInput : IUserInput
    {
        public bool IsKeyDown(Keys key)
        {
            return false;
        }

        public bool IsKeyUp(Keys key)
        {
            return true;
        }

        public bool IsKeyPressed(Keys key)
        {
            return false;
        }

        public bool MouseDown
        {
            get { return false; }
        }

        public bool MousePressed
        {
            get { return false; }
        }

        public Vector2 MousePosition
        {
            get { return Vector2.Zero; }
        }
    }
}

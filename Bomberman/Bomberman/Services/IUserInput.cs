using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Bomberman.Services
{
    /// <summary>
    /// Provide methods to read user input.
    /// </summary>
    interface IUserInput
    {
        bool IsKeyDown(Keys key); // True if key is currently down.
        bool IsKeyUp(Keys key); // True if key is currently up.
        bool IsKeyPressed(Keys key); // True if key has been pressed since last update.
        bool MouseDown { get; } // True if the mouse/touchscreen is currently down.
        bool MousePressed { get; } // True if the mouse/touchscreen has been pressed since last update.
        Vector2 MousePosition { get; } // Get the position of the mouse/touch area.
    }
}
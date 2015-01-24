using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Bomberman.Services
{
    class SimpleUserInput : GameComponent, IUserInput
    {
        private KeyboardState oldKeyboardState;
        private KeyboardState currentKeyboardState;
        private MouseState oldMouseState;
        private MouseState currentMouseState;

        public SimpleUserInput(Game game) : base(game)
        {
            oldKeyboardState = currentKeyboardState = Keyboard.GetState();
            oldMouseState = currentMouseState = Mouse.GetState();

            game.Components.Add(this);
        }

        public bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return currentKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !oldKeyboardState.IsKeyDown(key);
        }

        public bool MouseDown
        {
            get { return currentMouseState.LeftButton == ButtonState.Pressed; }
        }

        public bool MousePressed
        {
            get { return currentMouseState.LeftButton == ButtonState.Pressed && oldMouseState.LeftButton == ButtonState.Released;  }
        }

        public Vector2 MousePosition
        {
            get { return new Vector2(currentMouseState.X, currentMouseState.Y); }
        }

        public override void Update(GameTime gameTime)
        {
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            oldMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }
    }
}
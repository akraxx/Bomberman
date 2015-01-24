using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace Bomberman.Services
{
    class NullSoundPlayer : ISoundPlayer
    {
        public int ActiveSounds { get { return 0; } }
        public void Preload(string name) { }
        public void Clear(bool unload) { }
        public void Play(String name) { }
        public void Play(String name, Vector2 position) { }
        public void Stop(String name) { }
    }
}
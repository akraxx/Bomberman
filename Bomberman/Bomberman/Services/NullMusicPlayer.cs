using System;

namespace Bomberman.Services
{
    class NullMusicPlayer : IMusicPlayer
    {
        public string Name { get { return null; } }
        public bool IsPlaying { get { return false; } }
        public bool IsFading { get { return false; } }
        public void Load(String name) { }
        public void Play() { }
        public void Stop() { }
        public void Stop(TimeSpan fadeDuration) { }
    }
}
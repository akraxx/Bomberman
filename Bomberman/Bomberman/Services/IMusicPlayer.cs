using System;

namespace Bomberman.Services
{
    /// <summary>
    /// Provide methods to play music.
    /// </summary>
    interface IMusicPlayer
    {
        string Name { get; } // Return the name of the loaded music.
        bool IsPlaying { get; } // True if a music is playing.
        bool IsFading { get; } // True if the current music is fading out.
        void Load(string name); // Load a music.
        void Play(); // Play the loaded music.
        void Stop(); // Stop immediately the music that is currently playing.
        void Stop(TimeSpan fadeDuration); // Stop the music that is currently playing with a fade out.
    }
}
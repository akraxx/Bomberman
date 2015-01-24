using System;

using Microsoft.Xna.Framework;

namespace Bomberman.Services
{
    /// <summary>
    /// Provide methods to play sounds.
    /// </summary>
    interface ISoundPlayer
    {
        int ActiveSounds { get; } // Get the number of sound effects currently playing.
        void Preload(string name); // Preload a sound effect.
        void Clear(bool unload); // Stop all sounds. Can also unload sound data from memory.
        void Play(String name); // Play a sound effect.
        void Play(String name, Vector2 position); // Play a sound effect at a position of the screen.
        void Stop(String name); // Stop a sound effect.
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace Bomberman.Services
{
    class SimpleSoundPlayer : GameComponent, ISoundPlayer
    {
        private ContentManager content;
        private Dictionary<string, SoundEffectInstance> activeSounds = new Dictionary<string, SoundEffectInstance>();
        private List<string> markedForDeletion = new List<string>();

        public int ActiveSounds { get { return activeSounds.Count; } }

        public SimpleSoundPlayer(Game game) : base(game)
        {
            content = new ContentManager(Game.Services, "Content\\sound");
            game.Components.Add(this);
        }

        public void Preload(string name)
        {
            content.Load<SoundEffect>(name);
        }

        public void Clear(bool unload)
        {
            foreach (var s in activeSounds)
            {
                s.Value.Stop();
                s.Value.Dispose();
            }
            activeSounds.Clear();

            if (unload)
            {
                content.Unload();
            }
        }

        public void Play(String name)
        {
            this.Play(name, 1.0f);
        }

        public void Play(String name, Vector2 position)
        {
            this.Play(name, position.X / Game.GraphicsDevice.Viewport.Width);
        }

        public void Stop(String name)
        {
            SoundEffectInstance s;
            if (activeSounds.TryGetValue(name, out s))
            {
                activeSounds.Remove(name);
                s.Stop();
                s.Dispose();
            }
        }

        public override void Update(GameTime gameTime)
        {
            markedForDeletion.Clear();
            foreach (var s in activeSounds)
            {
                if (s.Value.State == SoundState.Stopped)
                {
                    markedForDeletion.Add(s.Key);
                }
            }
            foreach (var k in markedForDeletion)
            {
                this.Stop(k);
            }
        }

        private void Play(String name, float pan)
        {
            this.Stop(name);

            SoundEffect s = content.Load<SoundEffect>(name);
            SoundEffectInstance i = s.CreateInstance();
            i.Pan = pan;
            i.Play();
            activeSounds[name] = i;
        }
    }
}
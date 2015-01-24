using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace Bomberman.Services
{
    class SimpleMusicPlayer : GameComponent, IMusicPlayer
    {
        private bool fading;
        private TimeSpan fadeTimer;
        private TimeSpan fadeTimerMax;

        private ContentManager content;
        private Song musicData;

        public string Name { get { return musicData != null ? musicData.Name : null; } }
        public bool IsPlaying { get { return MediaPlayer.State == MediaState.Playing || MediaPlayer.State == MediaState.Paused; } }
        public bool IsFading { get { return fading; } }

        public SimpleMusicPlayer(Game game) : base(game)
        {
            content = new ContentManager(Game.Services, "Content\\music");
            game.Components.Add(this);
        }

        public void Load(String name)
        {
            this.Stop();

            if (musicData != null && musicData.Name != name)
            {
                content.Unload();
            }
            musicData = content.Load<Song>(name);
        }

        public void Play()
        {
            if (musicData != null)
            {
                this.Stop();
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Volume = 1.0f;
                MediaPlayer.Play(musicData);
            }
        }

        public void Stop()
        {
            this.Stop(TimeSpan.Zero);
        }

        public void Stop(TimeSpan fadeDuration)
        {
            if (IsPlaying)
            {
                if (fadeDuration.Ticks > 0 && MediaPlayer.State == MediaState.Paused)
                {
                    if (!fading)
                    {
                        fadeTimer = TimeSpan.FromSeconds(0.0);
                        fadeTimerMax = fadeDuration;
                        fading = true;
                    }
                }
                else
                {
                    MediaPlayer.Stop();
                    fading = false;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(fading) {
                fadeTimer += gameTime.ElapsedGameTime;

                if (fadeTimer > fadeTimerMax)
                {
                    this.Stop();
                }
                else
                {
                    MediaPlayer.Volume = 1.0f - (float)(fadeTimer.TotalSeconds / fadeTimerMax.TotalSeconds);
                }
            }
        }
    }
}
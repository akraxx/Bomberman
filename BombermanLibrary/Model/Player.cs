using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Model
{
    public class Player
    {
        /// <summary>
        /// The ID of the player.
        /// </summary>
        public byte ID { get; private set; }

        /// <summary>
        /// Set to true when player is currently used.
        /// </summary>
        public bool Playing { get; private set; }

        /// <summary>
        /// Set to true when player is in waiting situation.
        /// A player should be waiting when he joins a game in progress or when he loses his last life.
        /// Should be reset to false once the player takes part in the game.
        /// </summary>
        public bool Waiting { get; set; }

        /// <summary>
        /// Set to true when player is in game over situation.
        /// A player is set in game over situation when he has 0 stock after his bomberman death.
        /// </summary>
        public bool GameOver { get; set; }

        /// <summary>
        /// Set to true if the player is the game host.
        /// </summary>
        public bool Host { get; private set; }

        /// <summary>
        /// Set to true if the player is the local player.
        /// </summary>
        public bool Local { get; private set; }

        /// <summary>
        /// Name of the player.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Score of the player.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Wins of the player.
        /// </summary>
        public int Wins { get; set; }

        /// <summary>
        /// The number of characters the player can deploy before being "game over".
        /// </summary>
        public int Stock { get; set; }

        /// <summary>
        /// Bomberman currently controlled by the player. Can be null.
        /// </summary>
        public Bomberman Bomberman { get; set; }

        public Player(byte id)
        {
            ID = id;
            this.Leave();
        }

        /// <summary>
        /// Should be called when a player joins the game.
        /// </summary>
        public void Join(string name, bool isHost, bool isLocal)
        {
            bool wasPlaying = Playing;

            Playing = true;
            Waiting = true;
            GameOver = false;
            Host = isHost;
            Local = isLocal;
            Name = name;
            Score = 0;
            Wins = 0;
            Stock = 3;
            Bomberman = null;

            if (!wasPlaying)
            {
                if (Joined != null) Joined(this, new EventArgs());
            }
        }

        /// <summary>
        /// Should be called when a player is removed from the game.
        /// </summary>
        public void Leave()
        {
            if (Playing)
            {
                if (Left != null) Left(this, new EventArgs());
            }

            Playing = false;
            Waiting = false;
            GameOver = false;
            Host = false;
            Local = false;
            Name = "";
            Score = 0;
            Wins = 0;
            Stock = 0;
            Bomberman = null;
        }

        /// <summary>
        /// Should be called when a player continues the game.
        /// </summary>
        public void Continue()
        {
            Waiting = true;
            GameOver = false;
            Score = 0;
            Stock = 3;
        }

        // Events

        /// <summary>
        /// Fired when the player joins the game.
        /// Core player data is available when this event is fired.
        /// </summary>
        public event EventHandler<EventArgs> Joined;

        /// <summary>
        /// Fired when the player leaves the game.
        /// It is the last chance of querying player data before it is reset.
        /// </summary>
        public event EventHandler<EventArgs> Left;
    }
}

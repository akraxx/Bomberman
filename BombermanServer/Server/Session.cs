using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Server
{
    /// <summary>
    /// Represents a server-side session.
    /// </summary>
    class Session
    {
        private static readonly TimeSpan updateRate = TimeSpan.FromTicks(333333);
        private TimeSpan update;

        /// <summary>
        /// The underlying game handled by the session.
        /// </summary>
        public ServerController Controller { get; protected set; }

        /// <summary>
        /// Return true if the session has ended.
        /// </summary>
        public bool Ended { get { return Controller.Ended; } }

        /// <summary>
        /// Tick the update timer, updating the controller when appropriate.
        /// </summary>
        public void Tick(TimeSpan elapsed)
        {
            update -= elapsed;
            while (update <= TimeSpan.Zero)
            {
                update += updateRate;
                Controller.Update(updateRate);
            }
        }

        public Session()
        {
            update = updateRate;
            Controller = new ServerController();
        }
    }
}
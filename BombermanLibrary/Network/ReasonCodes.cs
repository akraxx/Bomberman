using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Network
{
    /// <summary>
    /// The various reason codes used in the protocol.
    /// </summary>
    public enum ReasonCodes
    {
        /// <summary>
        /// Disconnection because of a technical problem.
        /// </summary>
        Disconnected,

        // ***** Denied game reasons *****

        /// <summary>
        /// The protocol version doesn't match.
        /// </summary>
        VersionMismatch,

        /// <summary>
        /// The token or username is illegal.
        /// </summary>
        IllegalLogin,

        /// <summary>
        /// No game found with the provided token.
        /// </summary>
        GameNotFound,

        /// <summary>
        /// The requested game doesn't allow joining at this point.
        /// </summary>
        GameNotJoinable,

        /// <summary>
        /// The requested game is already full.
        /// </summary>
        GameFull,

        // ***** Kicked reasons *****

        /// <summary>
        /// The player was kicked by the host.
        /// </summary>
        HostKick,

        /// <summary>
        /// The player was kicked by the server (suspected cheat).
        /// </summary>
        CheatKick,

        /// <summary>
        /// The player was kicked by the server (game ended normally).
        /// </summary>
        EndKick,

        /// <summary>
        /// The game was aborted (the host left in lobby or all players have left the game).
        /// </summary>
        AbortedKick,
    }
}
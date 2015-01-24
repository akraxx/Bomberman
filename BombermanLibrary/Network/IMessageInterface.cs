using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Network
{
    /// <summary>
    /// Interface to send/receive MessageEvent instances to a remote endpoint.
    /// </summary>
    public interface IMessageInterface
    {
        /// <summary>
        /// True if the remote endpoint is connected.
        /// </summary>
        bool Up { get; }

        /// <summary>
        /// True if the reception queue is empty.
        /// </summary>
        bool Empty { get; }

        /// <summary>
        /// Pull the next message from the reception queue.
        /// </summary>
        MessageEvent Pull();

        /// <summary>
        /// Send a message to the remote endpoint.
        /// </summary>
        /// <param name="m"></param>
        void Send(MessageEvent m);

        /// <summary>
        /// Close the remote endpoint.
        /// </summary>
        void Close();

        /// <summary>
        /// Should be fired when communication with the endpoint was opened.
        /// </summary>
        event EventHandler EndpointUp;

        /// <summary>
        /// Should be fired when communication with the endpoint was closed.
        /// </summary>
        event EventHandler EndpointDown;
    }
}
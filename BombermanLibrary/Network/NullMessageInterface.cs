using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bomberman.Network
{
    /// <summary>
    /// Message interface that does no message exchanging at all.
    /// </summary>
    public sealed class NullMessageInterface : IMessageInterface
    {
        public bool Up { get { return false; } }

        public bool Empty { get { return true; } }

        public MessageEvent Pull()
        {
            throw new InvalidOperationException("NullMessageInterface doesn't do anything");
        }

        public void Send(MessageEvent m) { }

        public void Close()
        {
            throw new InvalidOperationException("NullMessageInterface can't be closed");
        }

        // Events

#pragma warning disable 67
        public event EventHandler EndpointUp;

        public event EventHandler EndpointDown;
#pragma warning restore 67
    }
}
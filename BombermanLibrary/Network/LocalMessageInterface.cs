using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bomberman.Network
{
    /// <summary>
    /// Message interface that does message exchange between a client and server situated over the local computer.
    /// </summary>
    public sealed class LocalMessageInterface : IMessageInterface
    {
        private Queue<MessageEvent> pending;

        private LocalMessageInterface endpoint;

        public bool Up { get { return endpoint != null; } }

        public bool Empty { get { return pending.Count == 0; } }

        public MessageEvent Pull()
        {
            if (!Empty)
            {
                return pending.Dequeue();
            }
            else
            {
                throw new InvalidOperationException("No MessageEvent in queue");
            }
        }

        public void Send(MessageEvent m)
        {
            if (endpoint != null)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                m.Write(writer);
                writer.Close();

                endpoint.Receive(stream.ToArray());
            }
        }

        private void Receive(byte[] data)
        {
            if (Up && data != null)
            {
                MemoryStream stream = new MemoryStream(data);
                BinaryReader reader = new BinaryReader(stream);
                MessageEvent m = MessageEvent.Read(reader);
                reader.Close();

                pending.Enqueue(m);
            }
        }

        public void Close()
        {
            if (endpoint != null)
            {
                this.Connect(null);
            }
            else
            {
                throw new InvalidOperationException("LocalMessageInterface is not connected");
            }
        }

        /// <summary>
        /// Connect the current LocalMessageInterface endpoint to the specified LocalMessageInterface.
        /// Will fire appropriate EndpointDown/EndpointUp events.
        /// </summary>
        public void Connect(LocalMessageInterface other)
        {
            if (endpoint != null)
            {
                endpoint = null;

                if (EndpointDown != null) EndpointDown(this, new EventArgs());
            }

            endpoint = other;

            if (endpoint != null)
            {
                if (EndpointUp != null) EndpointUp(this, new EventArgs());
            }
        }

        public LocalMessageInterface()
        {
            pending = new Queue<MessageEvent>();

            endpoint = null;
        }

        // Events

        public event EventHandler EndpointUp;

        public event EventHandler EndpointDown;
    }
}
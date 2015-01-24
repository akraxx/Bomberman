using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using Bomberman.Network;

namespace Bomberman.Server
{
    public class SocketMessageInterface : IMessageInterface
    {
        private static readonly object _sendLock = new object();
        private Socket socket = null;
        private Queue<MessageEvent> pending = new Queue<MessageEvent>();

        private bool sending = false;
        private int sendCursor = 0;
        private byte[] send = new byte[1024];
        private Queue<byte[]> sendQueue = new Queue<byte[]>();

        private int receptionCursor = 0;
        private byte[] reception = new byte[1024];

        public SocketMessageInterface()
        {
            Up = false;
        }

        private void BeginReceive()
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.SetBuffer(reception, receptionCursor, reception.Length - receptionCursor);
            e.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            if (!socket.ReceiveAsync(e))
            {
                Receive_Completed(socket, e);
            }
        }

        private void TrySendData()
        {
            if (sendQueue.Count > 0)
            {
                byte[] data = sendQueue.Peek();
                int remaining = send.Length - sendCursor;
                if (remaining >= data.Length)
                {
                    sendQueue.Dequeue();
                    data.CopyTo(send, sendCursor);
                    sendCursor += data.Length;
                }
            }
            this.BeginSend();
        }

        private void BeginSend()
        {
            if (!sending && sendCursor > 0)
            {
                sending = true;

                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(send, 0, sendCursor);
                e.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);
                if (!socket.SendAsync(e))
                {
                    Send_Completed(socket, e);
                }
            }
        }

        /// <summary>
        /// Connect the message interface to the provided connected socket.
        /// </summary>
        public void Start(Socket connectSocket)
        {
            if (connectSocket != null)
            {
                if (connectSocket.Connected)
                {
                    if (!Up)
                    {
                        Up = true;
                        socket = connectSocket;
                        if (EndpointUp != null) EndpointUp(this, new EventArgs());

                        this.BeginReceive();
                    }
                    else
                    {
                        throw new InvalidOperationException("Interface is already connected");
                    }
                }
                else
                {
                    throw new ArgumentException("Socket is not connected");
                }
            }
            else
            {
                throw new ArgumentNullException("connectSocket");
            }
        }

        /// <summary>
        /// Temporarily suspend the socket interface, allowing it to be transfered to another user.
        /// </summary>
        public void Suspend()
        {
            if (Up)
            {
                Up = false;
                if (EndpointDown != null) EndpointDown(this, new EventArgs());
            }
        }

        /// <summary>
        /// Resume a suspended socket interface.
        /// </summary>
        public void Resume()
        {
            if (socket != null)
            {
                if (!Up)
                {
                    Up = true;
                    if (EndpointUp != null) EndpointUp(this, new EventArgs());
                }
            }
            else
            {
                throw new InvalidOperationException("Can't unfreeze: no underlying socket");
            }
        }

        // Event handlers

        void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (socket != null)
            {
                if (e.SocketError == SocketError.Success)
                {
                    receptionCursor += e.BytesTransferred;

                    bool tryNextMessage = true;
                    while (tryNextMessage)
                    {
                        tryNextMessage = false;
                        if (receptionCursor >= sizeof(int))
                        {
                            MemoryStream stream = new MemoryStream(reception);
                            BinaryReader reader = new BinaryReader(stream);
                            int messageSize = reader.ReadInt32();
                            int available = (int)(receptionCursor - stream.Position);
                            if (available >= messageSize)
                            {
                                MessageEvent m = MessageEvent.Read(reader);
                                receptionCursor -= (int)stream.Position;
                                stream.Read(reception, 0, receptionCursor); // Le reste des données est replacé au début du buffer...
                                reader.Close();

                                pending.Enqueue(m);
                                tryNextMessage = true;
                            }
                        }
                    }

                    if (Up)
                    {
                        this.BeginReceive();
                    }
                }
                else
                {
                    this.Close();
                }
            }
        }

        void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (socket != null)
            {
                if (e.SocketError == SocketError.Success)
                {
                    lock (_sendLock)
                    {
                        sendCursor -= e.BytesTransferred;
                        for (int i = 0; i < sendCursor; i++) send[i] = send[i + e.BytesTransferred]; // Le reste des données est replacé au début du buffer...

                        sending = false;

                        if (Up)
                        {
                            this.TrySendData();
                        }
                    }
                }
                else
                {
                    this.Close();
                }
            }
        }

        // Interface

        public bool Up { get; private set; }

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
            if (Up)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(0);
                int begin = (int)stream.Position;
                m.Write(writer);
                int size = (int)(stream.Position) - begin;
                stream.Seek(0, SeekOrigin.Begin);
                writer.Write(size);
                writer.Close();

                byte[] myPrecious = stream.ToArray();
                
                lock (_sendLock)
                {
                    sendQueue.Enqueue(myPrecious);

                    this.TrySendData();
                }
            }
        }

        public void Close()
        {
            if (socket != null)
            {
                Up = false;
                try
                {
                    socket.Close(1);
                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { }
                socket = null;
                if (EndpointDown != null) EndpointDown(this, new EventArgs());
            }
        }

        // Events

        public event EventHandler EndpointUp;

        public event EventHandler EndpointDown;
    }
}
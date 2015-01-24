using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Bomberman.Network;

namespace Bomberman.Client
{
    public class ClientConnection
    {
        private Socket socket;

        /// <summary>
        /// Socket state of the connection.
        /// </summary>
        public SocketError Result { get; protected set; }

        public ClientConnection()
        {
            Result = SocketError.NotConnected;
        }

        public void Connect(string host)
        {
            if (socket == null)
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.RemoteEndPoint = new DnsEndPoint(host, Protocol.Port);
                e.Completed += new EventHandler<SocketAsyncEventArgs>(Connect_Completed);
                if (!socket.ConnectAsync(e))
                {
                    Connect_Completed(socket, e);
                }
            }
            else
            {
                throw new InvalidOperationException("Connection already used");
            }
        }

        // Events handlers

        void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            Result = e.SocketError;
            SocketMessageInterface messageInterface = null;
            if (e.SocketError == SocketError.Success)
            {
                messageInterface = new SocketMessageInterface();
                messageInterface.Start(e.ConnectSocket);
            }
            if (Completed != null) Completed(this, new EventArgs<SocketMessageInterface>(messageInterface));
        }

        // Events
        
        public event EventHandler<EventArgs<SocketMessageInterface>> Completed;
    }
}
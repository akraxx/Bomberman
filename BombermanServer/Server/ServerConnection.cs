using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace Bomberman.Server
{
    class ServerConnection
    {
        public ServerConnection() { }

        public void Accept(Socket socket)
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            socket.AcceptAsync(e);
        }

        // Events handlers

        void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            SocketMessageInterface messageInterface = null;
            if (e.SocketError == SocketError.Success)
            {
                messageInterface = new SocketMessageInterface();
                messageInterface.Start(e.AcceptSocket);
            }
            if (Completed != null) Completed(this, new EventArgs<SocketMessageInterface>(messageInterface));
        }

        // Events

        public event EventHandler<EventArgs<SocketMessageInterface>> Completed;
    }
}
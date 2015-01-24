using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;

using Bomberman.Server;

namespace Bomberman
{
    public partial class ServerControl : Form
    {
        private readonly object _lock = new object();

        private GameServer server;

        private DateTime lastUpdate;

        public ServerControl()
        {
            server = new GameServer();

            InitializeComponent();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                TimeSpan elapsed = DateTime.Now - lastUpdate;
                lastUpdate = DateTime.Now;
                //elapsed = TimeSpan.FromSeconds(10); // Rien de tel pour débugger les sockets que d'overclocker le serveur...
                server.Tick(elapsed);
            }
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            GameServer.States state = server.State;
            StatusText.Text = string.Format("{0} | {1} pending logins | {2} games", state.ToString(), server.Logins.Count, server.Sessions.Count);

            if (state == GameServer.States.Started || state == GameServer.States.Stopped || state == GameServer.States.Error)
            {
                RestartButton.Enabled = true;
                if (state == GameServer.States.Started)
                {
                    RestartButton.Text = "Stop";
                }
                else
                {
                    RestartButton.Text = "Start";
                }
            }
            else
            {
                RestartButton.Enabled = false;
            }
        }

        private void ServerControl_Load(object sender, EventArgs e)
        {
            RefreshTimer.Enabled = true;

            server.Start();

            // START THE CLOCK!!
            lastUpdate = DateTime.Now;
            System.Timers.Timer timer = new System.Timers.Timer(10);
            timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            timer.Start();
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            if (server.State == GameServer.States.Started)
            {
                server.Stop();
            }
            else
            {
                server.Start();
            }
        }
    }
}
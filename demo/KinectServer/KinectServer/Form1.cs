using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenNI;

namespace KinectServer
{
    public partial class Form1 : Form
    {
        private WebService service = new WebService();
        private List<int> players = new List<int>();
        private int currentPlayer = -1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Instance_OnPlayerLost(object sender, PlayerDetectedEventArgs e)
        {
            if (this.players.Any(p => p == e.PlayerId))
            {
                this.players.Remove(e.PlayerId);
            }

            UpdatePlayerList();
        }

        private void Instance_OnNewPlayer(object sender, PlayerDetectedEventArgs e)
        {
            if (!this.players.Any(p => p == e.PlayerId))
            {
                this.players.Add(e.PlayerId);
            }
            Console.WriteLine("3333333333");
            UpdatePlayerList();
        }

        private void Instance_OnPlayerDetected(object sender, PlayerDetectedEventArgs e)
        {
            this.currentPlayer = e.PlayerId;
            this.UpdatePlayerList();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Stop();
        }

        private void UpdatePlayerList()
        {
            listView1.BeginInvoke((MethodInvoker)delegate()
            {
                listView1.Items.Clear();
                foreach (var player in this.players)
                {
                    listView1.Items.Add("Player-" + player.ToString());
                    if (player == this.currentPlayer)
                    {
                        listView1.Items[listView1.Items.Count - 1].Selected = true;
                    }
                }
            });
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            KinectManager.Instance.OnPlayerDetected += Instance_OnPlayerDetected;
            KinectManager.Instance.OnNewPlayer += Instance_OnNewPlayer;
            KinectManager.Instance.OnPlayerLost += Instance_OnPlayerLost;
            KinectManager.Instance.Start();
            this.service.Start();
            this.btnStart.Enabled = false;
            this.btnStop.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.Stop();
        }

        private void Stop()
        {
            KinectManager.Instance.Stop();
            this.service.Stop();

            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }
    }
}

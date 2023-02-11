using ShadeClient.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShadeClient
{
    public partial class ShatClient : Form
    {

        private Client client;

        public ShatClient()
        {
            InitializeComponent();
        }

        private void ShatClient_Load(object sender, EventArgs e)
        {
            client = new Client("192.168.100.42", 9090);
            client.Connect();
            //client.SendAudio();
            //client.ReceiveAudio();
            client.ReceiveMessage();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            client.SendMessage(1, chatText.Text);
        }
    }
}

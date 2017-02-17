using DSS.UareU.Web.Api.Client.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSS.A2F.Fingerprint.ReaderClient
{
    public partial class Main : Form
    {
        string urlConnectionString;
        ReaderWebSocketClientService client;
        Color tempColor;

        public Main()
        {
            InitializeComponent();

            urlConnectionString = ConfigurationManager.AppSettings["auth2factor.Websocket"];
            client = new ReaderWebSocketClientService(urlConnectionString);

        }

        private void startButton_Click(object sender, EventArgs e)
        {
            tempColor = startButton.BackColor;
            client.Start(this.idLabel.Text);
            startButton.BackColor = Color.Green;
            startButton.Enabled = false;
            startButton.Text = "Conectado";
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            startButton.BackColor = tempColor;
            startButton.Enabled = true;
            client.Close(this.idLabel.Text);
            startButton.Text = "Conectar";
        }

        private void Main_Load(object sender, EventArgs e)
        {
            var id = NUlid.Ulid.NewUlid();
            var uid = id.ToString().Substring(18, 8);

            idLabel.Text = uid.Insert(4, "-");

        }

        private void copyClipboardButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.idLabel.Text);
        }
    }
}

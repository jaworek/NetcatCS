using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Net;

namespace Netcat
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);

        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text = this.textBox1.Text + text + Environment.NewLine;
            }
        }

        abstract class Tcp
        {
            public TcpClient client;
            public StreamReader sr;
            public StreamWriter sw;
            public Button b;
            public TextBox t1, t2;
            public SetTextCallback f;

            public void tx(Object sender, EventArgs e)
            {
                String toClient;
                toClient = t2.Text;
                if (toClient != null) sw.WriteLine(toClient);
                f("Me: " + toClient);
                t2.Text = "";
            }

            public void rx()
            {
                String fromServer;
                do
                {
                    fromServer = sr.ReadLine();
                    if (fromServer != null) f("Client: " + fromServer);
                }
                while (fromServer != null);

                System.Environment.Exit(0);
            }
        }

        class WncTcpServer : Tcp
        {
            TcpListener listener;

            public WncTcpServer(ref Button bt, ref TextBox tb1, ref TextBox tb2, String port, SetTextCallback ff) : base()
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), int.Parse(port));
                listener.Start();

                client = listener.AcceptTcpClient();

                sr = new StreamReader(client.GetStream());
                sw = new StreamWriter(client.GetStream());
                sw.AutoFlush = true;

                b = bt;
                t1 = tb1;
                t2 = tb2;
                b.Click += tx;

                f = ff;

                Thread th = new Thread(rx);
                th.Start();
            }
        }

        class WncTcpClient : Tcp
        {
            public WncTcpClient(ref Button bt, ref TextBox tb1, ref TextBox tb2, String rip, String rport, SetTextCallback ff) : base()
            {
                client = new TcpClient(rip, int.Parse(rport));

                sr = new StreamReader(client.GetStream());
                sw = new StreamWriter(client.GetStream());
                sw.AutoFlush = true;

                b = bt;
                t1 = tb1;
                t2 = tb2;
                b.Click += tx;

                f = ff;

                Thread th = new Thread(rx);
                th.Start();
            }
        }

        abstract class Udp
        {
            public UdpClient client;
            public String ip = null;
            public int port;
            public Button b;
            public TextBox t1, t2;
            public SetTextCallback f;

            public void tx(Object sender, EventArgs e)
            {
                String toClient;
                toClient = t2.Text;
                if (toClient != null) client.Send(Encoding.ASCII.GetBytes(toClient), toClient.Length, ip, port);
                f("Me: " + toClient);
                t2.Text = "";
            }
        }

        class WncUdpServer : Udp
        {
            public WncUdpServer(ref Button bt, ref TextBox tb1, ref TextBox tb2, String lport, SetTextCallback ff) : base()
            {
                port = int.Parse(lport);
                client = new UdpClient(port);

                b = bt;
                t1 = tb1;
                t2 = tb2;
                b.Click += tx;

                f = ff;

                Thread th = new Thread(rx);
                th.Start();
            }

            void rx()
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                String fromServer;
                do
                {
                    fromServer = Encoding.ASCII.GetString(client.Receive(ref RemoteIpEndPoint));
                    ip = RemoteIpEndPoint.Address + "";
                    port = RemoteIpEndPoint.Port;
                    if (fromServer != null) f("Client: " + fromServer);
                }
                while (fromServer != null);

                System.Environment.Exit(0);
            }
        }

        class WncUdpClient : Udp
        {
            public WncUdpClient(ref Button bt, ref TextBox tb1, ref TextBox tb2, String rip, String rport, SetTextCallback ff) : base()
            {
                client = new UdpClient();

                ip = rip;
                port = int.Parse(rport);

                b = bt;
                t1 = tb1;
                t2 = tb2;
                b.Click += tx;

                f = ff;

                Thread th = new Thread(rx);
                th.Start();
            }

            void rx()
            {
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                client.Client.Bind(RemoteIpEndPoint); // NOTE THIS LINE. THIS IS FOR THE UDP CLIENT ONLY

                String fromServer;
                do
                {
                    fromServer = Encoding.ASCII.GetString(client.Receive(ref RemoteIpEndPoint));
                    ip = RemoteIpEndPoint.Address + "";
                    port = RemoteIpEndPoint.Port;
                    if (fromServer != null) f("Client: " + fromServer);
                }
                while (fromServer != null);

                System.Environment.Exit(0);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox3.Enabled = false;
            textBox4.Enabled = false;
            textBox5.Enabled = false;
            listBox1.Enabled = false;
            button1.Enabled = false;

            if (listBox1.SelectedIndex == 0) new WncTcpServer(ref button2, ref textBox1, ref textBox2, textBox5.Text, SetText);
            if (listBox1.SelectedIndex == 1) new WncTcpClient(ref button2, ref textBox1, ref textBox2, textBox3.Text, textBox4.Text, SetText);
            if (listBox1.SelectedIndex == 2) new WncUdpServer(ref button2, ref textBox1, ref textBox2, textBox5.Text, SetText);
            if (listBox1.SelectedIndex == 3) new WncUdpClient(ref button2, ref textBox1, ref textBox2, textBox3.Text, textBox4.Text, SetText);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //textBox1.Text = "Hello";
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                MessageBox.Show("Enter key pressed");
            }
        }
    }
}

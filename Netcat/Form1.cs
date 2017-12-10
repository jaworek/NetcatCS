using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        class WncTcpServer
        {
            TcpListener listener;
            TcpClient client;
            StreamReader sr;
            StreamWriter sw;
            Button b;
            TextBox t1, t2;
            SetTextCallback f;

            public WncTcpServer(ref Button bt, ref TextBox tb1, ref TextBox tb2, String port, SetTextCallback ff)
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

            public void tx(Object sender, EventArgs e)
            {
                String toClient;
                toClient = t2.Text;
                if (toClient != null) sw.WriteLine(toClient);
                f("Me: " + toClient);
                t2.Text = "";
            }

            void rx()
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

        class WncTcpClient
        {
            TcpClient client;
            StreamReader sr;
            StreamWriter sw;
            Button b;
            TextBox t1, t2;
            SetTextCallback f;

            public WncTcpClient(ref Button bt, ref TextBox tb1, ref TextBox tb2, String rip, String rport, SetTextCallback ff)
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

            public void tx(Object sender, EventArgs e)
            {
                String toClient;
                toClient = t2.Text;
                if (toClient != null) sw.WriteLine(toClient);
                f("Me: " + toClient);
                t2.Text = "";
            }

            void rx()
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

        class WncUdpServer
        {
            UdpClient client;
            String ip = null;
            int port;
            Button b;
            TextBox t1, t2;
            SetTextCallback f;

            public WncUdpServer(ref Button bt, ref TextBox tb1, ref TextBox tb2, String lport, SetTextCallback ff)
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

            public void tx(Object sender, EventArgs e)
            {
                String toClient;
                toClient = t2.Text;
                if (toClient != null) client.Send(Encoding.ASCII.GetBytes(toClient), toClient.Length, ip, port);
                f("Me: " + toClient);
                t2.Text = "";
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

        class WncUdpClient
        {
            UdpClient client;
            String ip = null;
            int port;
            Button b;
            TextBox t1, t2;
            SetTextCallback f;

            public WncUdpClient(ref Button bt, ref TextBox tb1, ref TextBox tb2, String rip, String rport, SetTextCallback ff)
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

            public void tx(Object sender, EventArgs e)
            {
                String toClient;
                toClient = t2.Text;
                if (toClient != null) client.Send(Encoding.ASCII.GetBytes(toClient), toClient.Length, ip, port);
                f("Me: " + toClient);
                t2.Text = "";
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

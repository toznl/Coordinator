using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Threading;

using System.Runtime.InteropServices;


namespace Coordinator
{
    public struct CoOrd
    {
        public float x;
        public float y;
        public float z;

        public CoOrd(float x1, float y1, float z1)
        {
            x = x1;
            y = y1;
            z = z1;
        }
    }


    public partial class Client : UserControl
    {
        private Socket socket = null;
        private Thread waitMsg = null;
        private Grid root;
        CoOrd co = new CoOrd();

        bool flag = true;
        public Client(Grid root)
        {
            InitializeComponent();
            this.root = root;
            Connect();
            //btSend.Click += new RoutedEventHandler(btSend_Click);
        }

        //void btSend_Click(object sender, RoutedEventArgs e)
        //{
        //    //send 버튼 클릭
        //    SendMsg();

        //}

        private void SendMsg()
        {
            CoOrd co = new CoOrd();
            try
            {
                
            }
            catch (Exception) { MessageBox.Show("메세지 센드에서 익셉션."); }
        }
        private void Connect()
        {
            try
            {
                IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(iep);
                socket.Send(Encoding.Default.GetBytes("접속."));

                waitMsg = new Thread(new ThreadStart(wait));
                waitMsg.Start();
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
        }

        delegate void MyDelegate();

        private void wait()
        {

            while (flag)
            {
                try
                {
                    byte[] data = new byte[1024];
                    string msg;
                    socket.Receive(data, data.Length, SocketFlags.None);
                    msg = Encoding.Default.GetString(data);
                    msg = msg.TrimEnd('\0');
                    MyDelegate del = delegate ()
                    {
                    };
                    root.Dispatcher.Invoke(DispatcherPriority.Normal, del);


                }
                catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            }
        }
        ~Client()
        {
            flag = false;
        }
    }
}

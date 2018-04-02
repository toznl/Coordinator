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
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct CoOrd
    {
        [MarshalAs(UnmanagedType.R4)]
        public float x;

        [MarshalAs(UnmanagedType.R4)]
        public float y;

        [MarshalAs(UnmanagedType.R4)]
        public float z;

        public CoOrd(float x1, float y1, float z1)
        {
            x = x1;
            y = y1;
            z = z1;
        }

        // Calling this method will return a byte array with the contents
        // of the struct ready to be sent via the tcp socket.
        public byte[] Serialize()
        {
            // allocate a byte array for the struct data
            var buffer = new byte[Marshal.SizeOf(typeof(CoOrd))];

            // Allocate a GCHandle and get the array pointer
            var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var pBuffer = gch.AddrOfPinnedObject();

            // copy data from struct to array and unpin the gc pointer
            Marshal.StructureToPtr(this, pBuffer, false);
            gch.Free();

            return buffer;
        }

        // this method will deserialize a byte array into the struct.
        public void Deserialize(ref byte[] data)
        {
            var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
            this = (CoOrd)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(CoOrd));
            gch.Free();
        }

        public void SendCoord(CoOrd co)
        {
            if (co.x != null && co.y != null && co.z != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    byte[] buffer = co.Serialize();

                    TcpClient client = new TcpClient();
                    client.Connect("127.0.0.1", 8080);
                    Console.WriteLine("connected...");

                    NetworkStream stream = client.GetStream();

                    stream.Write(buffer, 0, buffer.Length);
                    Console.WriteLine("{0} data sent", buffer.Length);
                }

            }
        }
    }

    public partial class Client : UserControl
    {
        private Socket socket = null;
        private Thread waitMsg = null;
        private Grid root;

        bool flag = true;
        public Client(Grid root)
        {
            InitializeComponent();
            this.root = root;
            Connect();
        }

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

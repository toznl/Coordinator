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
using Microsoft.Kinect;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Security.Cryptography;


namespace Coordinator
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Serializable]
    public struct CoOrd
    {
        [MarshalAs(UnmanagedType.R4)]
        public float x;

        [MarshalAs(UnmanagedType.R4)]
        public float y;

        [MarshalAs(UnmanagedType.R4)]
        public float z;

        [MarshalAs(UnmanagedType.IUnknown)]
        public object markerType;

        [MarshalAs(UnmanagedType.IUnknown)]
        public object markerTrackingState;

        public CoOrd(float x1, float y1, float z1, object m1, object m2)
        {
            x = x1;
            y = y1;
            z = z1;
            markerType = m1;
            markerTrackingState = m2;

        }


        // Calling this method will return a byte array with the contents
        // of the struct ready to be sent via the tcp socket.
        //public byte[] Serialize()
        //{
        //    // allocate a byte array for the struct data
        //    var buffer = new byte[Marshal.SizeOf(typeof(CoOrd))];

        //    // Allocate a GCHandle and get the array pointer
        //    var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        //    var pBuffer = gch.AddrOfPinnedObject();

        //    // copy data from struct to array and unpin the gc pointer
        //    Marshal.StructureToPtr(this, pBuffer, false);
        //    gch.Free();

        //    return buffer;
        //}

        //// this method will deserialize a byte array into the struct.
        //public void Deserialize(ref byte[] data)
        //{
        //    var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
        //    this = (CoOrd)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(CoOrd));
        //    gch.Free();
        //}
    }

    public partial class Client : UserControl
    {
        private Grid root;

        ////For using console windows
        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();

        //bool flag = true;
        public Client(Grid root)
        {
            InitializeComponent();
            this.root = root;
        }
    }
}

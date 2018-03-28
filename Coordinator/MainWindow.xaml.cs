using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;

namespace Coordinator
{
    public partial class MainWindow : Window

    {
        //Variables
        KinectSensor sensor; 
        DepthFrameReader depthReader;
        BodyFrameReader bodyReader;
        IList<Body> bodies;


        //For using console windows
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        

        //Main windows Initialize
        public MainWindow()
        {

            AllocConsole();
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        //Depth Image Process Frame
        private ImageSource ProcessFrame(DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;
            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;
            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];
            frame.CopyFrameDataToArray(pixelData);
            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];
                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);
                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red
                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;
            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        //Kinect sensor open & Depthreader, Bodyreader open
        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.sensor = KinectSensor.GetDefault();
            this.sensor.Open();

            this.depthReader = this.sensor.DepthFrameSource.OpenReader();
            this.depthReader.FrameArrived += OnDepthFrameArrived;

            this.bodyReader = this.sensor.BodyFrameSource.OpenReader();
            this.bodyReader.FrameArrived += OnBodyFrameArrived;
            
        }

        //Close all readers
        void CloseReader(object sender, RoutedEventArgs e)
        {
            this.depthReader.FrameArrived -= OnDepthFrameArrived;
            this.depthReader.Dispose();
            this.depthReader = null;

            this.bodyReader.FrameArrived -= OnBodyFrameArrived;
            this.bodyReader.Dispose();
            this.bodyReader = null;


        }
        //release Kinect v2 Sensor
        void ReleaseSensor()
        {
            this.sensor.Close();
            this.sensor = null;
        }

        //Body Frame
        void OnBodyFrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            var frame = e.FrameReference.AcquireFrame();
            
            if (frame != null)
                {
                    bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(bodies);

                    foreach (var body in bodies)
                    {
                        if (bodies != null)
                        {
                            if (body.IsTracked)
                            {
                                Joint head = body.Joints[JointType.Head];

                                float x = head.Position.X;
                                float y = head.Position.Y;
                                float z = head.Position.Z;

                            // HeadPosition (x,y,z) to console
                            System.Console.WriteLine("HeadPosition x : "+x);
                            System.Console.WriteLine("HeadPosition y : "+y);
                            System.Console.WriteLine("HeadPosition z : "+z);
                             }
                        }
                    }
                }
            

        }

        //Depth Frame
        void OnDepthFrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    depthCamera.Source = ProcessFrame(frame);
                }
            }
        }

        static Brush[] brushes =
        {
            Brushes.Green,
            Brushes.Blue,
            Brushes.Red,
            Brushes.Orange,
            Brushes.Purple,
            Brushes.Yellow

        };

    }

    
        
}


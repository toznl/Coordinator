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

using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Security.Cryptography;

using CooOrdStructure;

namespace Coordinator
{
    public partial class MainWindow : Window

    {
        //Variables
        KinectSensor sensor;
        DepthFrameReader depthReader;
        BodyFrameReader bodyReader;
        IList<Body> bodies;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        //Main windows Initialize
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;

            AllocConsole();
            this.Closing += delegate
            {
                Environment.Exit(1);
            };
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

        void SendBuffer(ref byte[] buffer)
        {
            try
            {
                TcpClient tc = new TcpClient("192.168.0.191", 5000);
                NetworkStream stream = tc.GetStream();

                stream.Write(buffer, 0, buffer.Length);

                stream.Close();
                tc.Close();
                Console.WriteLine("Finish");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception:{0}", e.ToString());
            }
        }
         

        public byte[] Serialize(object param)
        {
            byte[] encMsg = null;
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter br = new BinaryFormatter();
                br.Serialize(ms, param);
                encMsg = ms.ToArray();
            }

            return encMsg;
        }

        public T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
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
                canvas.Children.Clear();
                bodies = new Body[frame.BodyFrameSource.BodyCount];
                frame.GetAndRefreshBodyData(bodies);

                foreach (var body in bodies)
                {
                    if (bodies != null)
                    {
                        if (body.IsTracked)
                        {
                            //Head
                            Joint head = body.Joints[JointType.Head];
                            DepthSpacePoint realJointHead = sensor.CoordinateMapper.MapCameraPointToDepthSpace(head.Position);
                            float headX = realJointHead.X;
                            float headY = realJointHead.Y;
                            float headZ = head.Position.Z;

                            CoOrd coHead = new CoOrd(headX, headY, headZ, head.JointType, head.TrackingState, 1);
                            byte[] buffer = Serialize(coHead);
                            //SendBuffer(ref buffer);

                            //Neck
                            Joint neck = body.Joints[JointType.Neck];
                            DepthSpacePoint realJointNeck = sensor.CoordinateMapper.MapCameraPointToDepthSpace(neck.Position);
                            float neckX = realJointNeck.X;
                            float neckY = realJointNeck.Y;
                            float neckZ = neck.Position.Z;

                            CoOrd coNeck = new CoOrd(neckX, neckY, neckZ, neck.JointType, neck.TrackingState, 1);
                            buffer = Serialize(coNeck);
                            //SendBuffer(ref buffer);

                            //LeftShoulder
                            Joint leftShoulder = body.Joints[JointType.ShoulderLeft];
                            DepthSpacePoint realJointLeftShoulder = sensor.CoordinateMapper.MapCameraPointToDepthSpace(leftShoulder.Position);
                            float leftShoulderX = realJointLeftShoulder.X;
                            float leftShoulderY = realJointLeftShoulder.Y;
                            float leftShoulderZ = leftShoulder.Position.Z;

                            CoOrd coLeftShoulder = new CoOrd(leftShoulderX, leftShoulderY, leftShoulderZ, leftShoulder.JointType, leftShoulder.TrackingState, 1);
                            buffer = Serialize(coLeftShoulder);
                            //SendBuffer(ref buffer);

                            //ElbowLeft
                            Joint elbowLeft = body.Joints[JointType.ElbowLeft];
                            DepthSpacePoint realJointElbowLeft = sensor.CoordinateMapper.MapCameraPointToDepthSpace(elbowLeft.Position);
                            float elbowLeftX = realJointElbowLeft.X;
                            float elbowLeftY = realJointElbowLeft.Y;
                            float elbowLeftZ = elbowLeft.Position.Z;

                            CoOrd coElbowLeft = new CoOrd(elbowLeftX, elbowLeftY, elbowLeftZ, elbowLeft.JointType, elbowLeft.TrackingState, 1);
                            buffer = Serialize(coElbowLeft);
                            //SendBuffer(ref buffer);

                            //WristLeft
                            Joint wristLeft = body.Joints[JointType.WristLeft];
                            DepthSpacePoint realJointWristLeft = sensor.CoordinateMapper.MapCameraPointToDepthSpace(wristLeft.Position);
                            float wristLeftX = realJointWristLeft.X;
                            float wristLeftY = realJointWristLeft.Y;
                            float wristLeftZ = wristLeft.Position.Z;

                            CoOrd coWristLeft = new CoOrd(wristLeftX, wristLeftY, wristLeftZ, wristLeft.JointType, wristLeft.TrackingState, 1);
                            buffer = Serialize(coWristLeft);
                            //SendBuffer(ref buffer);

                            //RightShoulder
                            Joint rightShoulder = body.Joints[JointType.ShoulderRight];
                            DepthSpacePoint realJointRightShoulder = sensor.CoordinateMapper.MapCameraPointToDepthSpace(rightShoulder.Position);
                            float rightShoulderX = realJointRightShoulder.X;
                            float rightShoulderY = realJointRightShoulder.Y;
                            float rightShoulderZ = rightShoulder.Position.Z;

                            CoOrd coRightShoulder = new CoOrd(rightShoulderX, rightShoulderY, rightShoulderZ, rightShoulder.JointType, rightShoulder.TrackingState, 1);
                            buffer = Serialize(coRightShoulder);
                            //SendBuffer(ref buffer);

                            //ElbowRight
                            Joint elbowRight = body.Joints[JointType.ElbowRight];
                            DepthSpacePoint realJointElbowRight = sensor.CoordinateMapper.MapCameraPointToDepthSpace(elbowRight.Position);
                            float elbowRightX = realJointElbowRight.X;
                            float elbowRightY = realJointElbowRight.Y;
                            float elbowRightZ = elbowRight.Position.Z;

                            CoOrd coElbowRight = new CoOrd(elbowRightX, elbowRightY, elbowRightZ, elbowRight.JointType, elbowRight.TrackingState, 1);
                            buffer = Serialize(coElbowRight);
                            //SendBuffer(ref buffer);

                            //WristRight
                            Joint wristRight = body.Joints[JointType.WristRight];
                            DepthSpacePoint realJointWristRight = sensor.CoordinateMapper.MapCameraPointToDepthSpace(wristRight.Position);
                            float wristRightX = realJointWristRight.X;
                            float wristRightY = realJointWristRight.Y;
                            float wristRightZ = wristRight.Position.Z;

                            CoOrd coWristRight = new CoOrd(wristRightX, wristRightY, wristRightZ, wristRight.JointType, wristRight.TrackingState, 1);
                            buffer = Serialize(coWristRight);
                            //SendBuffer(ref buffer);

                            //SpineBase
                            Joint spineBase = body.Joints[JointType.SpineBase];
                            DepthSpacePoint realJointSpineBase = sensor.CoordinateMapper.MapCameraPointToDepthSpace(spineBase.Position);
                            float spineBaseX = realJointSpineBase.X;
                            float spineBaseY = realJointSpineBase.Y;
                            float spineBaseZ = spineBase.Position.Z;

                            CoOrd coSpineBase = new CoOrd(spineBaseX, spineBaseY, spineBaseZ, spineBase.JointType, spineBase.TrackingState, 1);
                            buffer = Serialize(coSpineBase);
                            //SendBuffer(ref buffer);

                            //SpineMid
                            Joint spineMid = body.Joints[JointType.SpineMid];
                            DepthSpacePoint realJointSpineMid = sensor.CoordinateMapper.MapCameraPointToDepthSpace(spineMid.Position);
                            float spineMidX = realJointSpineMid.X;
                            float spineMidY = realJointSpineMid.Y;
                            float spineMidZ = spineMid.Position.Z;

                            CoOrd coSpineMid = new CoOrd(spineMidX, spineMidY, spineMidZ, spineMid.JointType, spineMid.TrackingState, 1);
                            buffer = Serialize(coSpineMid);
                            //SendBuffer(ref buffer);

                            //string savePath = @"c:\test\test.txt";
                            //string textValue;
                            //textValue = System.String.Format("{0}, {1}, {2}, {3}, {4}, {5} \r\n", leftShoulderX, leftShoulderY, leftShoulderZ, rightShoulderX, rightShoulderY, rightShoulderZ);
                            //System.IO.File.AppendAllText(savePath, textValue, Encoding.Default);


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


    }

}


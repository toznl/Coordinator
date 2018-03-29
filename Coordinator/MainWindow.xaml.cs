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

        //Winsock client 
        void client (string[] args)
        {
            byte[] bytes = new byte[1024];

            // connect to a Remote device
            try
            {
                // Establish the remote end point for the socket
                IPHostEntry ipHost = Dns.Resolve("127.0.0.1");
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 8080);

                Socket sender = new Socket(AddressFamily.InterNetwork,
                                           SocketType.Stream, ProtocolType.Tcp);



                // Connect the socket to the remote endpoint. Catch any errors

                sender.Connect(ipEndPoint);

                Console.WriteLine("Socket connected to {0}",
                                  sender.RemoteEndPoint.ToString());
                //string theMessage=Console.ReadLine();
                string theMessage;

                if (args.Length == 0)
                    theMessage = "This is a test";
                else
                    theMessage = args[0];

                byte[] msg = Encoding.ASCII.GetBytes(theMessage + "<TheEnd>");

                // Send the data through the socket
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device            
                int bytesRec = sender.Receive(bytes);

                Console.WriteLine("The Server says : {0}",
                                  Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();


            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
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

                            Ellipse drawHead = new Ellipse
                            {
                                Fill = Brushes.Red,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawHead, headX - drawHead.Width / 2);
                            Canvas.SetTop(drawHead, headY - drawHead.Height / 2);
                            canvas.Children.Add(drawHead);

                            //Neck

                            Joint neck = body.Joints[JointType.Neck];
                            DepthSpacePoint realJointNeck = sensor.CoordinateMapper.MapCameraPointToDepthSpace(neck.Position);
                            float neckX = realJointNeck.X;
                            float neckY = realJointNeck.Y;
                            float neckZ = neck.Position.Z;

                            Ellipse drawNeck = new Ellipse
                            {
                                Fill = Brushes.Orange,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawNeck, neckX - drawNeck.Width / 2);
                            Canvas.SetTop(drawNeck, neckY - drawNeck.Height / 2);
                            canvas.Children.Add(drawNeck);

                            //LeftShoulder

                            Joint leftShoulder = body.Joints[JointType.ShoulderLeft];
                            DepthSpacePoint realJointLeftShoulder = sensor.CoordinateMapper.MapCameraPointToDepthSpace(leftShoulder.Position);
                            float leftShoulderX = realJointLeftShoulder.X;
                            float leftShoulderY = realJointLeftShoulder.Y;
                            float leftShoulderZ = leftShoulder.Position.Z;

                            Ellipse drawLeftShoulder = new Ellipse
                            {
                                Fill = Brushes.Yellow,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawLeftShoulder, leftShoulderX - drawLeftShoulder.Width / 2);
                            Canvas.SetTop(drawLeftShoulder, leftShoulderY - drawLeftShoulder.Height / 2);
                            canvas.Children.Add(drawLeftShoulder);

                            //ElbowLeft

                            Joint elbowLeft = body.Joints[JointType.ElbowLeft];
                            DepthSpacePoint realJointElbowLeft = sensor.CoordinateMapper.MapCameraPointToDepthSpace(elbowLeft.Position);
                            float elbowLeftX = realJointElbowLeft.X;
                            float elbowLeftY = realJointElbowLeft.Y;
                            float elbowLeftZ = elbowLeft.Position.Z;

                            Ellipse drawElbowLeft = new Ellipse
                            {
                                Fill = Brushes.Yellow,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawElbowLeft, elbowLeftX - drawElbowLeft.Width / 2);
                            Canvas.SetTop(drawElbowLeft, elbowLeftY - drawElbowLeft.Height / 2);
                            canvas.Children.Add(drawElbowLeft);

                            //WristLeft

                            Joint wristLeft = body.Joints[JointType.WristLeft];
                            DepthSpacePoint realJointWristLeft = sensor.CoordinateMapper.MapCameraPointToDepthSpace(wristLeft.Position);
                            float wristLeftX = realJointWristLeft.X;
                            float wristLeftY = realJointWristLeft.Y;
                            float wristLeftZ = wristLeft.Position.Z;

                            Ellipse drawWristLeft = new Ellipse
                            {
                                Fill = Brushes.Yellow,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawWristLeft, wristLeftX - drawWristLeft.Width / 2);
                            Canvas.SetTop(drawWristLeft, wristLeftY - drawWristLeft.Height / 2);
                            canvas.Children.Add(drawWristLeft);

                            //RightShoulder

                            Joint rightShoulder = body.Joints[JointType.ShoulderRight];
                            DepthSpacePoint realJointRightShoulder = sensor.CoordinateMapper.MapCameraPointToDepthSpace(rightShoulder.Position);
                            float rightShoulderX = realJointRightShoulder.X;
                            float rightShoulderY = realJointRightShoulder.Y;
                            float rightShoulderZ = rightShoulder.Position.Z;

                            Ellipse drawRightShoulder = new Ellipse
                            {
                                Fill = Brushes.Green,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawRightShoulder, rightShoulderX - drawRightShoulder.Width / 2);
                            Canvas.SetTop(drawRightShoulder, rightShoulderY - drawRightShoulder.Height / 2);
                            canvas.Children.Add(drawRightShoulder);

                            //ElbowRight

                            Joint elbowRight = body.Joints[JointType.ElbowRight];
                            DepthSpacePoint realJointElbowRight = sensor.CoordinateMapper.MapCameraPointToDepthSpace(elbowRight.Position);
                            float elbowRightX = realJointElbowRight.X;
                            float elbowRightY = realJointElbowRight.Y;
                            float elbowRightZ = elbowRight.Position.Z;

                            Ellipse drawElbowRight = new Ellipse
                            {
                                Fill = Brushes.Green,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawElbowRight, elbowRightX - drawElbowRight.Width / 2);
                            Canvas.SetTop(drawElbowRight, elbowRightY - drawElbowRight.Height / 2);
                            canvas.Children.Add(drawElbowRight);

                            //WristRight

                            Joint wristRight = body.Joints[JointType.WristRight];
                            DepthSpacePoint realJointWristRight = sensor.CoordinateMapper.MapCameraPointToDepthSpace(wristRight.Position);
                            float wristRightX = realJointWristRight.X;
                            float wristRightY = realJointWristRight.Y;
                            float wristRightZ = wristRight.Position.Z;

                            Ellipse drawWristRight = new Ellipse
                            {
                                Fill = Brushes.Green,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawWristRight, wristRightX - drawWristRight.Width / 2);
                            Canvas.SetTop(drawWristRight, wristRightY - drawWristRight.Height / 2);
                            canvas.Children.Add(drawWristRight);

                            //SpineBase

                            Joint spineBase = body.Joints[JointType.SpineBase];
                            DepthSpacePoint realJointSpineBase = sensor.CoordinateMapper.MapCameraPointToDepthSpace(spineBase.Position);
                            float spineBaseX = realJointSpineBase.X;
                            float spineBaseY = realJointSpineBase.Y;
                            float spineBaseZ = spineBase.Position.Z;

                            Ellipse drawSpineBase = new Ellipse
                            {
                                Fill = Brushes.Orange,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawSpineBase, spineBaseX - drawSpineBase.Width / 2);
                            Canvas.SetTop(drawSpineBase, spineBaseY - drawSpineBase.Height / 2);
                            canvas.Children.Add(drawSpineBase);

                            //SpineMid

                            Joint spineMid = body.Joints[JointType.SpineMid];
                            DepthSpacePoint realJointSpineMid = sensor.CoordinateMapper.MapCameraPointToDepthSpace(spineMid.Position);
                            float spineMidX = realJointSpineMid.X;
                            float spineMidY = realJointSpineMid.Y;
                            float spineMidZ = spineMid.Position.Z;

                            Ellipse drawSpineMid = new Ellipse
                            {
                                Fill = Brushes.Orange,
                                Width = 20,
                                Height = 20
                            };

                            Canvas.SetLeft(drawSpineMid, spineMidX - drawSpineMid.Width / 2);
                            Canvas.SetTop(drawSpineMid, spineMidY - drawSpineMid.Height / 2);
                            canvas.Children.Add(drawSpineMid);



                            //Drawing Skeleton
                            //Head to Neck
                            Line lineHeadToNeck = new Line();
                            lineHeadToNeck.Stroke = Brushes.LightSteelBlue;
                            lineHeadToNeck.X1 = headX;
                            lineHeadToNeck.Y1 = headY;
                            lineHeadToNeck.X2 = neckX;
                            lineHeadToNeck.Y2 = neckY;
                            lineHeadToNeck.StrokeThickness = 2;
                            //Neck to LeftShoulder
                            Line lineNeckToLeftShoulder = new Line();
                            lineNeckToLeftShoulder.Stroke = Brushes.LightSteelBlue;
                            lineNeckToLeftShoulder.X1 = leftShoulderX;
                            lineNeckToLeftShoulder.Y1 = leftShoulderY;
                            lineNeckToLeftShoulder.X2 = neckX;
                            lineNeckToLeftShoulder.Y2 = neckY;
                            lineNeckToLeftShoulder.StrokeThickness = 2;
                            //LeftShoulder to LeftElbow
                            Line lineLeftShoulderToElbowLeft = new Line();
                            lineLeftShoulderToElbowLeft.Stroke = Brushes.LightSteelBlue;
                            lineLeftShoulderToElbowLeft.X1 = leftShoulderX;
                            lineLeftShoulderToElbowLeft.Y1 = leftShoulderY;
                            lineLeftShoulderToElbowLeft.X2 = elbowLeftX;
                            lineLeftShoulderToElbowLeft.Y2 = elbowLeftY;
                            lineLeftShoulderToElbowLeft.StrokeThickness = 2;
                            //LeftElbow to LeftWrist
                            Line lineElbowLeftToWristLeft = new Line();
                            lineElbowLeftToWristLeft.Stroke = Brushes.LightSteelBlue;
                            lineElbowLeftToWristLeft.X1 = elbowLeftX;
                            lineElbowLeftToWristLeft.Y1 = elbowLeftY;
                            lineElbowLeftToWristLeft.X2 = wristLeftX;
                            lineElbowLeftToWristLeft.Y2 = wristLeftY;
                            lineElbowLeftToWristLeft.StrokeThickness = 2;

                            //Neck to RightShoulder
                            Line lineNeckToRightShoulder = new Line();
                            lineNeckToRightShoulder.Stroke = Brushes.LightSteelBlue;
                            lineNeckToRightShoulder.X1 = rightShoulderX;
                            lineNeckToRightShoulder.Y1 = rightShoulderY;
                            lineNeckToRightShoulder.X2 = neckX;
                            lineNeckToRightShoulder.Y2 = neckY;
                            lineNeckToRightShoulder.StrokeThickness = 2;
                            //RightShoulder to RightElbow
                            Line lineRightShoulderToElbowRight = new Line();
                            lineRightShoulderToElbowRight.Stroke = Brushes.LightSteelBlue;
                            lineRightShoulderToElbowRight.X1 = rightShoulderX;
                            lineRightShoulderToElbowRight.Y1 = rightShoulderY;
                            lineRightShoulderToElbowRight.X2 = elbowRightX;
                            lineRightShoulderToElbowRight.Y2 = elbowRightY;
                            lineRightShoulderToElbowRight.StrokeThickness = 2;
                            //RightElbow to RightWrist
                            Line lineElbowRightToWristRight = new Line();
                            lineElbowRightToWristRight.Stroke = Brushes.LightSteelBlue;
                            lineElbowRightToWristRight.X1 = elbowRightX;
                            lineElbowRightToWristRight.Y1 = elbowRightY;
                            lineElbowRightToWristRight.X2 = wristRightX;
                            lineElbowRightToWristRight.Y2 = wristRightY;
                            lineElbowRightToWristRight.StrokeThickness = 2;

                            //Neck to SpineMid
                            Line lineNeckToSpineMid = new Line();
                            lineNeckToSpineMid.Stroke = Brushes.LightSteelBlue;
                            lineNeckToSpineMid.X1 = neckX;
                            lineNeckToSpineMid.Y1 = neckY;
                            lineNeckToSpineMid.X2 = spineMidX;
                            lineNeckToSpineMid.Y2 = spineMidY;
                            lineNeckToSpineMid.StrokeThickness = 2;

                            //SpineMid to SpineBase
                            Line lineSpineMidToSpineBase = new Line();
                            lineSpineMidToSpineBase.Stroke = Brushes.LightSteelBlue;
                            lineSpineMidToSpineBase.X1 = spineMidX;
                            lineSpineMidToSpineBase.Y1 = spineMidY;
                            lineSpineMidToSpineBase.X2 = spineBaseX;
                            lineSpineMidToSpineBase.Y2 = spineBaseY;
                            lineSpineMidToSpineBase.StrokeThickness = 2;


                            canvas.Children.Add(lineHeadToNeck);
                            canvas.Children.Add(lineNeckToLeftShoulder);
                            canvas.Children.Add(lineNeckToRightShoulder);
                            canvas.Children.Add(lineLeftShoulderToElbowLeft);
                            canvas.Children.Add(lineRightShoulderToElbowRight);
                            canvas.Children.Add(lineElbowRightToWristRight);
                            canvas.Children.Add(lineElbowLeftToWristLeft);
                            canvas.Children.Add(lineNeckToSpineMid);
                            canvas.Children.Add(lineSpineMidToSpineBase);
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


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

namespace CooOrdStructure

{
    //Main structure of joints with serializable
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    [Serializable]
    //Theta and Translation Valiables for Error function and initialized
    public struct ErrorVar
    {
        public double thetaX;
        public double thetaY;
        public double thetaZ;

        public double transX;
        public double transY;
        public double transZ;



        public ErrorVar(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            thetaX = x1;
            thetaY = y1;
            thetaZ = z1;

            transX = x2;
            transY = y2;
            transZ = z2;

        }
    }

    public struct CoOrdXYZ
    {
        public float x;
        public float y;
        public float z;

        public CoOrdXYZ(float x1, float y1, float z1)
        {
            x = x1;
            y = y1;
            z = z1;
        }
    }

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

        [MarshalAs(UnmanagedType.I4)]
        public int markerPC;

        [MarshalAs(UnmanagedType.IUnknown)]
        public Microsoft.Kinect.JointType markerType;

        [MarshalAs(UnmanagedType.IUnknown)]
        public object markerTrackingState;

        public CoOrd(float x1, float y1, float z1, Microsoft.Kinect.JointType m1, object m2, int mPc)
        {
            x = x1;
            y = y1;
            z = z1;
            markerType = m1;
            markerTrackingState = m2;
            markerPC = mPc;

        }
    }
}
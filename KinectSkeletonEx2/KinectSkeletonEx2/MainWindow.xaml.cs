using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Coding4Fun.Kinect.Wpf;

namespace KinectSkeletonEx2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectDevice;
        private Skeleton[] skeletonData;

        Polyline headPolyline = new Polyline();
        Polyline twistleftPolyline = new Polyline();
        Polyline twistrightPolyline = new Polyline();
        Polyline forearmleftPolyline = new Polyline();
        Polyline forearmrightPolyline = new Polyline();
        Polyline upperarmleftPolyline = new Polyline();
        Polyline upperarmrightPolyline = new Polyline();
        Polyline shoulderleftPolyline = new Polyline();
        Polyline shoulderrightPolyline = new Polyline();

        public KinectSensor KinectDevice
        {
            get { return this.kinectDevice; }
            set
            {
                if (this.kinectDevice != value)
                {
                    //Uninitialize
                    if (this.kinectDevice != null)
                    {
                        this.kinectDevice.Stop();
                        this.kinectDevice.SkeletonFrameReady -= kinectDevice_SkeletonFrameReady;
                        this.kinectDevice.ColorFrameReady -= kinectDevice_ColorFrameReady;
                        this.kinectDevice.SkeletonStream.Disable();
                        this.kinectDevice.ColorStream.Disable();
                        this.skeletonData = null;
                    }

                    this.kinectDevice = value;

                    //Initialize
                    if (this.kinectDevice != null)
                    {
                        if (this.kinectDevice.Status == KinectStatus.Connected)
                        {
                            this.kinectDevice.SkeletonStream.Enable();
                            this.kinectDevice.ColorStream.Enable();
                            this.skeletonData = new Skeleton[this.kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                            this.kinectDevice.SkeletonFrameReady += kinectDevice_SkeletonFrameReady;
                            this.kinectDevice.ColorFrameReady += kinectDevice_ColorFrameReady;
                            this.kinectDevice.Start();
                        }
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        private void kinectDevice_SkeletonFrameReady(object sender,SkeletonFrameReadyEventArgs e)
        {
            using(SkeletonFrame skeletonframe = e.OpenSkeletonFrame())
            {
                if (skeletonframe != null)
                {
                    skeletonData = new Skeleton[kinectDevice.SkeletonStream.FrameSkeletonArrayLength];
                    skeletonframe.CopySkeletonDataTo(skeletonData);
                    Skeleton skeleton = (from s in skeletonData
                                         where s.TrackingState == SkeletonTrackingState.Tracked
                                         select s).FirstOrDefault();
                    if (skeleton != null)
                    {
                        SetAllPointPosition(skeleton);
                    }
                }
            }
        }

        private void SetAllPointPosition(Skeleton ske)
        {
            SkeletonCanvas.Children.Clear();
            headPolyline.Points.Clear();
            twistleftPolyline.Points.Clear();
            twistrightPolyline.Points.Clear();
            forearmleftPolyline.Points.Clear();
            forearmrightPolyline.Points.Clear();
            upperarmrightPolyline.Points.Clear();
            upperarmleftPolyline.Points.Clear();
            shoulderleftPolyline.Points.Clear();
            shoulderrightPolyline.Points.Clear();

            foreach(Joint joint in ske.Joints)
            {
                Point jointPoint = GetPointPosition(joint);
                switch(joint.JointType)
                {
                    case JointType.Head:
                        SetPointPosition(headPoint,joint);
                        headPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ShoulderCenter:
                        SetPointPosition(shouldercenterPoint, joint);
                        headPolyline.Points.Add(jointPoint);
                        shoulderleftPolyline.Points.Add(jointPoint);
                        shoulderrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ShoulderLeft:
                        SetPointPosition(shoulderleftPoint, joint);
                        upperarmleftPolyline.Points.Add(jointPoint);
                        shoulderleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ShoulderRight:
                        SetPointPosition(shoulderrightPoint, joint);
                        upperarmrightPolyline.Points.Add(jointPoint);
                        shoulderrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ElbowLeft:
                        SetPointPosition(elbowleftPoint, joint);
                        upperarmleftPolyline.Points.Add(jointPoint);
                        forearmleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ElbowRight:
                        SetPointPosition(elbowrightPoint, joint);
                        upperarmrightPolyline.Points.Add(jointPoint);
                        forearmrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.WristLeft:
                        SetPointPosition(twistleftPoint, joint);
                        twistleftPolyline.Points.Add(jointPoint);
                        forearmleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.WristRight:
                        SetPointPosition(twistrightPoint, joint);
                        twistrightPolyline.Points.Add(jointPoint);
                        forearmrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.HandLeft:
                        SetPointPosition(handleftPoint, joint);
                        twistleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.HandRight:
                        SetPointPosition(handrightPoint, joint);
                        twistrightPolyline.Points.Add(jointPoint);
                        break;
                }
            }

            headPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            headPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(headPolyline);
            twistleftPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            twistleftPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(twistleftPolyline);
            twistrightPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            twistrightPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(twistrightPolyline);
            forearmleftPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            forearmleftPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(forearmleftPolyline);
            forearmrightPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            forearmrightPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(forearmrightPolyline);
            upperarmleftPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            upperarmleftPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(upperarmleftPolyline);
            upperarmrightPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            upperarmrightPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(upperarmrightPolyline);
            shoulderleftPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            shoulderleftPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(shoulderleftPolyline);
            shoulderrightPolyline.Stroke = new SolidColorBrush(Colors.Blue);
            shoulderrightPolyline.StrokeThickness = 5;
            SkeletonCanvas.Children.Add(shoulderrightPolyline);
        }

        private void SetPointPosition(FrameworkElement ellipse, Joint joint)
        {
            var scaledJoint = joint.ScaleTo(640, 480);
            Canvas.SetLeft(ellipse, scaledJoint.Position.X);
            Canvas.SetTop(ellipse, scaledJoint.Position.Y);
            SkeletonCanvas.Children.Add(ellipse);
        }

        private Point GetPointPosition(Joint joint)
        {
            var scaledJoint = joint.ScaleTo(640, 480);
            return new Point(scaledJoint.Position.X, scaledJoint.Position.Y);
        }

        private void kinectDevice_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    this.ColorImageElement.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96,
                                                                 PixelFormats.Bgr32, null, pixelData,
                                                                 frame.Width * frame.BytesPerPixel);

                }
            }
        }
    }
}

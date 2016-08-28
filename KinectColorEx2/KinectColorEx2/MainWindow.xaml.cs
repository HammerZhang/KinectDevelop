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
using System.Timers;
using System.IO;
using Microsoft.Kinect;
using Coding4Fun.Kinect.Wpf;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32;

namespace KinectColorEx2
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor kinectDevice;
        private Skeleton[] skeletonData;
        private bool initPos = true;
        FileStream fs;
        StreamWriter sw;

        Polyline headPolyline = new Polyline();
        Polyline twistleftPolyline = new Polyline();
        Polyline twistrightPolyline = new Polyline();
        Polyline forearmleftPolyline = new Polyline();
        Polyline forearmrightPolyline = new Polyline();
        Polyline upperarmleftPolyline = new Polyline();
        Polyline upperarmrightPolyline = new Polyline();
        Polyline shoulderleftPolyline = new Polyline();
        Polyline shoulderrightPolyline = new Polyline();

        struct HandPos3D
        {
            public double x;
            public double y;
            public double z;
        }

        HandPos3D rightHandPos;
        HandPos3D initRightHandPos;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_loaded(object sender, RoutedEventArgs e)
        {
            kinectDevice = (from sensor in KinectSensor.KinectSensors
                           where sensor.Status == KinectStatus.Connected
                           select sensor).FirstOrDefault();
            kinectDevice.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectDevice.SkeletonStream.Enable();
            kinectDevice.Start();
            kinectDevice.ColorFrameReady += kinectDevice_ColorFrameReady;
            kinectDevice.SkeletonFrameReady += new EventHandler < SkeletonFrameReadyEventArgs > (kinectDevice_SkeletonFrameReady);
        
        }

        private void kinectDevice_ColorFrameReady(object sender,ColorImageFrameReadyEventArgs e)
        {
            using(ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    byte[] pixelData = new byte[imageFrame.PixelDataLength];
                    imageFrame.CopyPixelDataTo(pixelData);
                    this.ColorImage.Source = BitmapSource.Create(imageFrame.Width, imageFrame.Height, 96, 96,
                                                                 PixelFormats.Bgr32, null, pixelData,
                                                                 imageFrame.Width * imageFrame.BytesPerPixel);
                }
            }
        }

        private void Window_Closed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            kinectDevice.Stop();
        }

        private void kinectDevice_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonframe = e.OpenSkeletonFrame())
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

            foreach (Joint joint in ske.Joints)
            {
                Point jointPoint = GetPointPosition(joint);
                switch (joint.JointType)
                {
                    case JointType.Head:
                        //SetPointPosition(headPoint, joint);
                        SetPointPositionAlt(headPoint, jointPoint);
                        headPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ShoulderCenter:
                        //SetPointPosition(shouldercenterPoint, joint);
                        SetPointPositionAlt(shouldercenterPoint, jointPoint);
                        headPolyline.Points.Add(jointPoint);
                        shoulderleftPolyline.Points.Add(jointPoint);
                        shoulderrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ShoulderLeft:
                        //SetPointPosition(shoulderleftPoint, joint);
                        SetPointPositionAlt(shoulderleftPoint, jointPoint);
                        upperarmleftPolyline.Points.Add(jointPoint);
                        shoulderleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ShoulderRight:
                        //SetPointPosition(shoulderrightPoint, joint);
                        SetPointPositionAlt(shoulderrightPoint, jointPoint);
                        upperarmrightPolyline.Points.Add(jointPoint);
                        shoulderrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ElbowLeft:
                        //SetPointPosition(elbowleftPoint, joint);
                        SetPointPositionAlt(elbowleftPoint, jointPoint);
                        upperarmleftPolyline.Points.Add(jointPoint);
                        forearmleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.ElbowRight:
                        //SetPointPosition(elbowrightPoint, joint);
                        SetPointPositionAlt(elbowrightPoint, jointPoint);
                        upperarmrightPolyline.Points.Add(jointPoint);
                        forearmrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.WristLeft:
                        //SetPointPosition(twistleftPoint, joint);
                        SetPointPositionAlt(twistleftPoint, jointPoint);
                        twistleftPolyline.Points.Add(jointPoint);
                        forearmleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.WristRight:
                        //SetPointPosition(twistrightPoint, joint);
                        SetPointPositionAlt(twistrightPoint, jointPoint);
                        twistrightPolyline.Points.Add(jointPoint);
                        forearmrightPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.HandLeft:
                        //SetPointPosition(handleftPoint, joint);
                        SetPointPositionAlt(handleftPoint, jointPoint);
                        twistleftPolyline.Points.Add(jointPoint);
                        break;
                    case JointType.HandRight:
                        //SetPointPosition(handrightPoint, joint);
                        SetPointPositionAlt(handrightPoint, jointPoint);
                        twistrightPolyline.Points.Add(jointPoint);
                        rightHandPos = Get3DHandPostion(joint);                // 记录右手三维位置
                        break;
                }
            }

            // 绘制骨骼
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
            DepthImagePoint point = kinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(
                joint.Position, kinectDevice.DepthStream.Format);
            Canvas.SetLeft(ellipse, point.X);
            Canvas.SetTop(ellipse, point.Y);
            SkeletonCanvas.Children.Add(ellipse);
        }

        private void SetPointPositionAlt(FrameworkElement ellipse, Point jointPoint)
        {
            Canvas.SetLeft(ellipse, jointPoint.X);
            Canvas.SetTop(ellipse, jointPoint.Y);
            SkeletonCanvas.Children.Add(ellipse);
        }

        private Point GetPointPosition(Joint joint)
        {
            //var scaledJoint = joint.ScaleTo(640, 480);
            DepthImagePoint point = kinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(
                joint.Position, kinectDevice.DepthStream.Format);
            //point.X *= (int)SkeletonCanvas.ActualWidth / kinectDevice.DepthStream.FrameWidth;
            //point.Y *= (int)SkeletonCanvas.ActualHeight / kinectDevice.DepthStream.FrameHeight;
            return new Point(point.X, point.Y);
        }

        private HandPos3D Get3DHandPostion(Joint joint)
        {
            HandPos3D handPos;
            DepthImagePoint point = kinectDevice.CoordinateMapper.MapSkeletonPointToDepthPoint(
                joint.Position, kinectDevice.DepthStream.Format);
            handPos.x = point.X;
            handPos.y = point.Y;
            handPos.z = point.Depth;

            return handPos;
        }
        private void MyTimer_Elapsed(object sender, EventArgs e)
        {
            if (initPos)
            {
                initRightHandPos = rightHandPos;
                initPos = false;
            }
            else
            {
                if (serialPort.SerialState == true)
                {
                    if (rightHandPos.x - initRightHandPos.x > 200)
                    {
                        //发送电机控制指令
                        serialPort.SetData = "000001";
                        string str1 = "000001";
                        sw.WriteLine(str1);
                    }
                    else if (rightHandPos.x - initRightHandPos.x < -200)
                    {
                        // 发送电机控制指令
                        serialPort.SetData = "000002";
                        string str1 = "000002";
                        sw.WriteLine(str1);
                    }
                    else if (rightHandPos.y - initRightHandPos.y > 200)
                    {
                        // 发送电机控制指令
                        serialPort.SetData = "000003";
                        string str1 = "000003";
                        sw.WriteLine(str1);
                    }
                    else if (rightHandPos.y - initRightHandPos.y < -200)
                    {
                        // 发送电机控制指令
                        serialPort.SetData = "000004";
                        string str1 = "000004";
                        sw.WriteLine(str1);
                    }
                    else if (rightHandPos.z - initRightHandPos.z > 200)
                    {
                        // 发送电机控制指令
                        serialPort.SetData = "000005";
                        string str1 = "000005";
                        sw.WriteLine(str1);
                    }
                    else if (rightHandPos.z - initRightHandPos.z < -200)
                    {
                        // 发送电机控制指令
                        serialPort.SetData = "000006";
                        string str1 = "000006";
                        sw.WriteLine(str1);
                    }
                }                
            }

            // 写入文本用作测试
            string str = "    " + rightHandPos.x.ToString() + "   " + rightHandPos.y.ToString() + "   " + rightHandPos.z.ToString() + "\n";
            sw.WriteLine(str);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // 开启定时器
            System.Timers.Timer myTimer = new System.Timers.Timer(50);
            myTimer.Elapsed += MyTimer_Elapsed;
            myTimer.Enabled = true;
            myTimer.AutoReset = true;

            // 开启文件读写流
            try
            {
                fs = new FileStream("RightHandPos.txt", FileMode.Create, FileAccess.Write);
                sw = new StreamWriter(fs);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }  
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sw.Close();
            fs.Close();
        }
    }
}

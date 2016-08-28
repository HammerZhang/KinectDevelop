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

namespace KinectMotorControl
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor kinectsensor;
        private readonly Brush[] skeletonBrushes;
        private Skeleton[] frameSkeleton;

        public KinectSensor kinectSensor
        {
            get { return this.kinectsensor; }
            set
            {
                if (this.kinectsensor != value)
                {
                    //如果当前传感器对象部位NULL
                    if (this.kinectsensor != null)
                    {
                        //uninitailize当前对象
                        //UninitializeKinectSensor(this.kinectsensor);
                        this.kinectsensor.Stop();
                        this.kinectsensor.SkeletonFrameReady -= kinectSensor_skeletionFrameReady;
                        this.kinectsensor.SkeletonStream.Disable();
                        this.frameSkeleton = null;
                        this.kinectsensor = null;
                    }
                    //如果传入的对象不为空，且状态为连接状态
                    if (value != null && value.Status == KinectStatus.Connected)
                    {
                        this.kinectsensor = value;
                        this.kinectsensor.SkeletonStream.Enable();
                        this.frameSkeleton = new Skeleton[this.kinectsensor.SkeletonStream.FrameSkeletonArrayLength];
                        this.kinectsensor.SkeletonFrameReady += kinectSensor_skeletionFrameReady;
                        this.kinectsensor.Start();
                        //InitializeKinectSensor(this.kinectsensor);
                    }
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            skeletonBrushes = new Brush[] { Brushes.Black, Brushes.Crimson, Brushes.Indigo, Brushes.DodgerBlue, 
                Brushes.Purple, Brushes.Pink };
            //this.Loaded += (s, e) => DiscoverKinectSensor();
            //this.Unloaded += (s, e) => this.kinectsensor = null;
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            this.kinectSensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch(e.Status)
            {
                case KinectStatus.Connected:
                    if (this.kinectsensor == null)
                        this.kinectsensor = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    if (this.kinectsensor == e.Sensor)
                    {
                        this.kinectsensor = null;
                        this.kinectsensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
                        if (this.kinectsensor == null)
                        {
                            //TODO:通知用于Kinect已拔出
                        }
                    }
                    break;
            }
        }

        private void InitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.ColorStream.Enable();
                //sensor.SkeletonStream.Enable();
                sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
                //this.frameSkeleton = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
                //sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_skeletionFrameReady);
                sensor.Start();
            }
        }

        private void UninitializeKinectSensor(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.ColorFrameReady -= new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);
                //sensor.SkeletonFrameReady -= new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_skeletionFrameReady);
                //this.frameSkeleton = null;
            }
        }

        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame frame = e.OpenColorImageFrame())
            {
                if (frame != null)
                {
                    byte[] pixelData = new byte[frame.PixelDataLength];
                    frame.CopyPixelDataTo(pixelData);
                    /*ColorAndSkeleton.Source = BitmapImage.Create(frame.Width, frame.Height, 96, 96,
                                                                 PixelFormats.Bgr32, null, pixelData,
                                                                 frame.Width * frame.BytesPerPixel);*/

                }
            }
        }

        void kinectSensor_skeletionFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using(SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Polyline figure;
                    Brush userBrush;
                    Skeleton skeleton;

                    LayoutRoot.Children.Clear();
                    frame.CopySkeletonDataTo(this.frameSkeleton);
                    for (int i = 0; i < this.frameSkeleton.Length; i++ )
                    {
                        skeleton = this.frameSkeleton[i];

                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            userBrush = this.skeletonBrushes[i % this.skeletonBrushes.Length];
                            // 绘制躯干和头
                            figure = CreateFigure(skeleton, userBrush, new[] {JointType.Head,JointType.ShoulderCenter,
                            JointType.ShoulderLeft,JointType.Spine,JointType.ShoulderRight,JointType.ShoulderCenter,
                            JointType.HipCenter});
                            LayoutRoot.Children.Add(figure);

                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipLeft, JointType.HipRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制左腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipLeft, 
                                JointType.KneeLeft, JointType.AnkleLeft, JointType.FootLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右腿
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.HipCenter, JointType.HipRight, 
                                JointType.KneeRight, JointType.AnkleRight, JointType.FootRight });
                            LayoutRoot.Children.Add(figure);

                            //绘制左臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderLeft, JointType.ElbowLeft,
                                JointType.WristLeft, JointType.HandLeft });
                            LayoutRoot.Children.Add(figure);

                            //绘制右臂
                            figure = CreateFigure(skeleton, userBrush, new[] { JointType.ShoulderRight, JointType.ElbowRight,
                                JointType.WristRight, JointType.HandRight });
                            LayoutRoot.Children.Add(figure);
                        }
                    }
                }
            }
        }

        private Polyline CreateFigure(Skeleton skeleton, Brush brushes, JointType[] joints)
        {
            Polyline figure = new Polyline();

            figure.StrokeThickness = 8;
            figure.Stroke = brushes;

            for (int i = 0; i < joints.Length; i++ )
            {
                figure.Points.Add(GetJointPoint(skeleton.Joints[joints[i]]));
            }

            return figure;
        }

        private Point GetJointPoint(Joint joint)
        {
            DepthImagePoint point = this.kinectsensor.MapSkeletonPointToDepth(joint.Position, this.kinectsensor.DepthStream.Format);

            point.X *= (int)this.LayoutRoot.ActualWidth / kinectsensor.DepthStream.FrameWidth;
            point.Y *= (int)this.LayoutRoot.ActualHeight / kinectsensor.DepthStream.FrameHeight;

            return new Point(point.X, point.Y);
        }
    }
}
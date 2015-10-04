using Microsoft.Kinect;
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

namespace KinectCoordinateMapping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        string projectPath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));


        CameraMode _mode = CameraMode.Color;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        private void zoomImage(float distance)
        {
            double zoom = distance * 1.5;
            double ScaleX = zoom;
            double ScaleY = zoom;
            ScaleTransform scale = new ScaleTransform(ScaleX, ScaleY);
            this.RenderTransform = scale;
        }
        private double toDegrees(double rads) {
            double degrees = (180 / Math.PI) * rads;
            Console.WriteLine("Degrees: " + degrees + "RADS: " + rads);
            return degrees;
        }

        private void rotateImage(double angleDeg, double pointX, double pointY)
        {
            var angle = angleDeg - 180;
            RotateTransform rotate = new RotateTransform(angle, pointX, pointY);

            
            
            // so I am passing 3 perfectly ok doubles and the function returns 
                                                                                //{System.Windows.Media.RotateTransform} object, Whichis what we want 
                                                                                // keeps throwing InvalidOperationException was unhandled
                                                                                // Stackoverflow suggests that Path is null, so it needs to const update
                                                                                // Dunno how to do it! Too tired! Will be back at 11am! :*+-


            //this.RenderTransform = rotate;
        }

        private double getJointAngle(Joint joint1, Joint joint2)
        {
            float [] point1 = {joint1.Position.X, joint1.Position.Y};
            float [] point2 = {joint2.Position.X, joint2.Position.Y};
            double d = toDegrees(Math.Atan2(point1[1]-point2[1], point1[0]-point2[0]));
            if (d != double.NaN ) // ret if x or y are +ve or -ve Infinity (1,1)!!!!!!!!
                { return d; }
            return 0;
            
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            this.Background = new ImageBrush(new BitmapImage(new System.Uri(projectPath + "/Assets/bg.jpg")));

            

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    Dictionary <ulong, string> bodyMapping = new Dictionary<ulong,string>();

                    string[] characters = {"bear", "robot", "mouse"};

                    int i = 0;

                    foreach (var body in _bodies)
                    {  
                        if (!bodyMapping.ContainsKey(body.TrackingId))
                        bodyMapping.Add(body.TrackingId, characters[i]);
                        i = (i + 1) % 3;
                    }

                    foreach (var body in _bodies)
                    {

                        if (body.IsTracked)
                        {

                            // COORDINATE MAPPING
                            foreach (Joint joint in body.Joints.Values)
                            {
                                if (joint.TrackingState == TrackingState.Tracked)
                                {
                                    // 3D space point
                                    CameraSpacePoint jointPosition = joint.Position;

                                    // 2D space point
                                    Point point = new Point();

                                    if (_mode == CameraMode.Color)
                                    {
                                        ColorSpacePoint colorPoint = _sensor.CoordinateMapper.MapCameraPointToColorSpace(jointPosition);

                                        point.X = float.IsInfinity(colorPoint.X) ? 0 : colorPoint.X;
                                        point.Y = float.IsInfinity(colorPoint.Y) ? 0 : colorPoint.Y;
                                    }
                                    else if (_mode == CameraMode.Depth || _mode == CameraMode.Infrared) // Change the Image and Canvas dimensions to 512x424
                                    {
                                        DepthSpacePoint depthPoint = _sensor.CoordinateMapper.MapCameraPointToDepthSpace(jointPosition);

                                        point.X = float.IsInfinity(depthPoint.X) ? 0 : depthPoint.X;
                                        point.Y = float.IsInfinity(depthPoint.Y) ? 0 : depthPoint.Y;
                                    }

                                    // Draw

                                    string character = bodyMapping[body.TrackingId];    

                                    switch (joint.JointType.ToString())
                                    {
                                        case "Head":
                                            Image face = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/face.png")),
                                                Width = 200,
                                                Height = 200
                                            };

                                            //double angle = getJointAngle(joint, body.Joints.Values.ElementAt(2));

                                            //rotateImage(angle, joint.Position.X, joint.Position.Y);

                                            Canvas.SetLeft(face, point.X - face.Width / 2);
                                            Canvas.SetTop(face, point.Y - face.Width / 2);
                                          

                                            canvas.Children.Add(face);

                                            break;

                                        case "SpineMid":
                                            Image belly = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/belly.png")),
                                                Width = 300,
                                                Height = 520,
                                                Stretch = Stretch.Fill
                                            };

                                            Canvas.SetLeft(belly, point.X - belly.Width / 2.2);
                                            Canvas.SetTop(belly, point.Y - belly.Height/ 3.2);

                                            canvas.Children.Add(belly);

                                            break;

                                        case "ShoulderRight":
                                            Image rightUpperArm = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/rightUpperArm.png")),
                                                Width = 200,
                                                Height = 200
                                            };

                                            Canvas.SetLeft(rightUpperArm, point.X);
                                            Canvas.SetTop(rightUpperArm, point.Y);

                                            canvas.Children.Add(rightUpperArm);

                                            break;

                                        case "ElbowRight":
                                            Image rightLowerArm = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/rightLowerArm.png")),
                                                Width = 200,
                                                Height = 200
                                            };

                                            Canvas.SetLeft(rightLowerArm, point.X);
                                            Canvas.SetTop(rightLowerArm, point.Y);

                                            canvas.Children.Add(rightLowerArm);

                                            break;

                                        case "HandRight":
                                            Image rightHand = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/rightHand.png")),
                                                Width = 150,
                                                Height = 150
                                            };

                                            Canvas.SetLeft(rightHand, point.X - rightHand.Width / 2);
                                            Canvas.SetTop(rightHand, point.Y - rightHand.Height / 2);

                                            canvas.Children.Add(rightHand);

                                            break;

                                        case "ShoulderLeft":
                                            Image leftUpperArm = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/leftUpperArm.png")),
                                                Width = 200,
                                                Height = 200
                                            };

                                            Canvas.SetLeft(leftUpperArm, point.X - leftUpperArm.Width);
                                            Canvas.SetTop(leftUpperArm, point.Y);

                                            canvas.Children.Add(leftUpperArm);

                                            break;
                                        case "ElbowLeft":
                                            Image leftLowerArm = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/leftLowerArm.png")),
                                                Width = 200,
                                                Height = 200
                                            };

                                            Canvas.SetLeft(leftLowerArm, point.X - leftLowerArm.Width);
                                            Canvas.SetTop(leftLowerArm, point.Y);

                                            canvas.Children.Add(leftLowerArm);

                                            break;

                                        case "HandLeft":
                                            Image leftHand = new Image
                                            {
                                                Source = new BitmapImage(new System.Uri(projectPath + "/Assets/" + character + "/leftHand.png")),
                                                Width = 150,
                                                Height = 150
                                            };

                                            Canvas.SetLeft(leftHand, point.X - leftHand.Width / 2);
                                            Canvas.SetTop(leftHand, point.Y - leftHand.Height / 2);

                                            canvas.Children.Add(leftHand);

                                            break;

                                        default:

                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    enum CameraMode
    {
        Color,
        Depth,
        Infrared
    }
}

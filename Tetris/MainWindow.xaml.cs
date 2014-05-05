using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Tetris.TetrisModule;
using Tetris.Pages;

namespace Tetris
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        private MainPage mainpage;

        private readonly KinectSensorChooser sensorChooser;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            // Initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

            mainpage = new MainPage();
            restoreStart();
        }
        public void changeFrame(IMainPage page)
        {
            this.mainFrame.Navigate(page);
        }
        public void restoreStart()
        {
            changeSensorPosition(HorizontalAlignment.Left, VerticalAlignment.Top);
            this.mainFrame.Navigate(mainpage);
        }
        public void popPage(IPopup page)
        {
            this.popupBackground.Visibility = Visibility.Visible;
            this.popupFrame.Navigate(page);
            this.popupFrame.Visibility = Visibility.Visible;
        }
        public void exitPopup()
        {
            this.popupFrame.Visibility = Visibility.Collapsed;
            this.popupBackground.Visibility = Visibility.Collapsed;
        }

        public void changeSensorPosition(HorizontalAlignment h, VerticalAlignment v)
        {
            sensorViewer.HorizontalAlignment = h;
            sensorViewer.VerticalAlignment = v;

            sensorChooserUi.HorizontalAlignment = h;
            sensorChooserUi.VerticalAlignment = v;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TetrisM.getInstance().saveHighscores();
        }
        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.ColorStream.Disable();
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                        // Dont need lower joints
                        args.NewSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    }
                    catch (InvalidOperationException)
                    {
                        error = true;
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
            if (!error)
            {
                kinectRegion.KinectSensor = args.NewSensor;
            }
        }
    }
}

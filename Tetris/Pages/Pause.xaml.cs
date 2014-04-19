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
using System.Windows.Shapes;
using Tetris.TetrisModule;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect;

namespace Tetris.Pages
{
    public partial class Pause : Page, IMainPage
    {
        private KinectSensorChooser sensorChooser;
        public Pause()
        {
            InitializeComponent();
        }

        private void resumeButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisM.getInstance().pausePlay();
            MainWindow.Instance.restoreStart();
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisM.getInstance().startGame();
            MainWindow.Instance.restoreStart();
        }

        public void bindSensor(KinectSensorChooser sensorChooser)
        {
            this.sensorChooser = sensorChooser;
            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
        }
    }
}

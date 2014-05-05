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
        public Pause()
        {
            MainWindow.Instance.changeSensorPosition(HorizontalAlignment.Center, VerticalAlignment.Top);
            InitializeComponent();
            changeButtonStatus(TetrisM.getInstance().getGBlockStatus());
        }

        private void resumeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.restoreStart();
            TetrisM.getInstance().pausePlay();
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow.Instance.restoreStart();
            TetrisM.getInstance().startGame();
        }

        private void ghostBlockButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisM t = TetrisM.getInstance();
            t.changeGBlockStatus();
            changeButtonStatus(t.getGBlockStatus());
        }

        private void changeButtonStatus(bool activate) {
            if (activate)
            {
                ghostBlockButtonLabel.Content = "Deactivate";
                ghostBlockButtonEllipse.Fill = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                ghostBlockButtonLabel.Content = "Activate";
                ghostBlockButtonEllipse.Fill = new SolidColorBrush(Colors.White);
            }
        }
    }
}

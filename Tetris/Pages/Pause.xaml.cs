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
using System.Windows.Threading;

namespace Tetris.Pages
{
    public partial class Pause : Page, IMainPage
    {
        DispatcherTimer delaySubmit = new DispatcherTimer();
        public Pause()
        {
            MainWindow.Instance.changeSensorPosition(HorizontalAlignment.Center, VerticalAlignment.Top);
            InitializeComponent();
            changeButtonStatus(TetrisM.getInstance().getGBlockStatus());

            restartButton.IsEnabled = false;
            resumeButton.IsEnabled = false;

            delaySubmit.Tick += new EventHandler(delaySubmitEvent);
            delaySubmit.Interval = TimeSpan.FromMilliseconds(1000);     /* Delay submit for 3 seconds */
            delaySubmit.Start();
        }
        public void delaySubmitEvent(object sender, EventArgs e)
        {
            delaySubmit.Stop();
            restartButton.IsEnabled = true;
            resumeButton.IsEnabled = true;
        }
        private void resumeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.restoreStart();
            MainWindow.Instance.getMainPage().resumeGame();
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.restoreStart();
            MainWindow.Instance.getMainPage().startGame();
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
                ghostBlockButtonLabel.Content = "Desactivar";
                ghostBlockButtonEllipse.Fill = new SolidColorBrush(Colors.Gray);
            }
            else
            {
                ghostBlockButtonLabel.Content = "Activar";
                ghostBlockButtonEllipse.Fill = new SolidColorBrush(Colors.White);
            }
        }
    }
}

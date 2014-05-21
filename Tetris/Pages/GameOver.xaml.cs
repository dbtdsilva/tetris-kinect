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
using System.Windows.Threading;

namespace Tetris.Pages
{
    /// <summary>
    /// Interaction logic for gameover.xaml
    /// </summary>
    public partial class GameOver : Page, IPopup
    {
        DispatcherTimer delayPress = new DispatcherTimer();
        public GameOver(int finalscore)
        {
            InitializeComponent();
            score.Content = finalscore;

            RestartButton.IsEnabled = false;

            delayPress.Tick += new EventHandler(delayPressEvent);
            delayPress.Interval = TimeSpan.FromMilliseconds(1000);     /* Delay submit for 3 seconds */
            delayPress.Start();
        }
        public void delayPressEvent(object sender, EventArgs e)
        {
            delayPress.Stop();
            RestartButton.IsEnabled = true;
        }
        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.exitPopup();
            MainWindow.Instance.getMainPage().startGame();
        }
    }
}

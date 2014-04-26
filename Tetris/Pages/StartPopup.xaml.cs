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
using Tetris.TetrisModule;

namespace Tetris.Pages
{
    /// <summary>
    /// Interaction logic for StartPopup.xaml
    /// </summary>
    public partial class StartPopup : Page, IPopup
    {
        public StartPopup()
        {
            InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisM.getInstance().startGame();
            MainWindow.Instance.exitPopup();
        }

        private void KinectCircleButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

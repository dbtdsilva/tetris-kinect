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
        public MainPage mainpage;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            mainpage = new MainPage();
            this.mainFrame.Navigate(mainpage);
        }
        public void PopupWindow(Page page)
        {
            this.popupBackground.Visibility = Visibility.Visible;
            this.popupFrame.Navigate(page);
            this.popupFrame.Visibility = Visibility.Visible;
        }
        public void ExitPopup()
        {
            this.popupFrame.Visibility = Visibility.Collapsed;
            this.popupBackground.Visibility = Visibility.Collapsed;
        }
    }
}

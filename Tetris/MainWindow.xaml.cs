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
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;

            mainpage = new MainPage();
            restoreStart();
        }
        public void changeFrame(IMainPage page)
        {
            this.mainFrame.Navigate(page);
        }
        public void restoreStart()
        {
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
            var window = MainWindow.GetWindow(this);
            this.popupFrame.Visibility = Visibility.Collapsed;
            this.popupBackground.Visibility = Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TetrisM.getInstance().saveHighscores();
        }
    }
}

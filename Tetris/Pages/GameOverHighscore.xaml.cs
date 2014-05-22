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
using Microsoft.Kinect.Toolkit.Controls;
using System.Windows.Threading;

namespace Tetris.Pages
{
    /// <summary>
    /// Interaction logic for GameOverHighscore.xaml
    /// </summary>
    public partial class GameOverHighscore : Page, IPopup
    {
        private int finalscore;
        private int activeIndex;

        private Key keyLetter = Key.None;
        private Key keyBlock = Key.None;
        
        DispatcherTimer kinectSmoothMovement = new DispatcherTimer();
        DispatcherTimer delaySubmit = new DispatcherTimer();
        
        public GameOverHighscore(int finalscore)
        {
            InitializeComponent();

            Submit.IsEnabled = false;
            CloseButton.IsEnabled = false;
            delaySubmit.Tick += new EventHandler(delaySubmitEvent);
            delaySubmit.Interval = TimeSpan.FromMilliseconds(3000);     /* Delay submit for 3 seconds */
            delaySubmit.Start();

            kinectSmoothMovement.Tick += new EventHandler(onTimedEvent);
            kinectSmoothMovement.Interval = TimeSpan.FromMilliseconds(500);
            kinectSmoothMovement.Start();
            score.Content = finalscore;
            this.finalscore = finalscore;

            changeLetter(0);
            MainWindow.Instance.KeyDown += new KeyEventHandler(Key_Down);
        }
        public void delaySubmitEvent(object sender, EventArgs e)
        {
            delaySubmit.Stop();
            CloseButton.IsEnabled = true;
            Submit.IsEnabled = true;
        }
        private void Kinect_HandPointerGrip(object sender, HandPointerEventArgs e)
        {
            //onSubmit(sender, e);
        }
        private void onClose(object sender, RoutedEventArgs e)
        {
            StartPopup page = new StartPopup();
            MainWindow.Instance.popPage(page);
            MainWindow.Instance.KeyDown -= new KeyEventHandler(Key_Down);
            e.Handled = true;
        }
        private void onSubmit(object sender, RoutedEventArgs e)
        {
            if (delaySubmit.IsEnabled)
                return;
            string name = "";
            int childrenCount = VisualTreeHelper.GetChildrenCount(lettersGrid);
            for (int i = 0; i < childrenCount; i++)
            {
                TextBlock child = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, i);
                name += child.Text;
            }
            if (!TetrisM.getInstance().submitScore(finalscore, name))
                /* This is never supposed to return false since it is controlled by MainPage */
                throw new NotImplementedException();

            StartPopup page = new StartPopup();
            MainWindow.Instance.popPage(page);
            MainWindow.Instance.KeyDown -= new KeyEventHandler(Key_Down);
            e.Handled = true;
        }
        private void changeLetter(int new_index) {
            TextBlock letter = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, activeIndex);
            letter.Foreground = new SolidColorBrush(Colors.DimGray);
            letter.Background = new SolidColorBrush(Colors.WhiteSmoke);

            activeIndex = new_index;
            letter = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, new_index);
            letter.Foreground = new SolidColorBrush(Colors.Black);
            letter.Background = new SolidColorBrush(Colors.DarkGray);
        }
        private void Key_Down(object sender, KeyEventArgs e)
        {   
            if (changeLetter(e.Key))
                e.Handled = true;
        }
        private bool changeLetter(Key e)
        {
            char c;
            TextBlock letter;
            switch (e)
            {
                case Key.Down:
                    letter = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, activeIndex);
                    c = letter.Text.ToCharArray()[0];
                    if (c == 'Z')
                        c = 'A';
                    else
                        c++;
                    letter.Text = c.ToString();
                    break;
                case Key.Up:
                    letter = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, activeIndex);
                    c = letter.Text.ToCharArray()[0];
                    if (c == 'A')
                        c = 'Z';
                    else
                        c--;
                    letter.Text = c.ToString();
                    break;
                case Key.Right:
                    if (activeIndex < 2)
                        changeLetter(activeIndex + 1);
                    break;
                case Key.Left:
                    if (activeIndex > 0)
                        changeLetter(activeIndex - 1);
                    break;
                default:
                    return false;
            }
            return true;
        }
        private void Kinect_HandPointerMove(object sender, HandPointerEventArgs e)
        {
            Point pos = e.HandPointer.GetPosition(this);

                if (pos.X > (0.75 * ActualWidth))
                    keyBlock = Key.Right;
                else if (pos.X < (0.25 * ActualWidth))
                    keyBlock = Key.Left;
                else
                    keyBlock = Key.None;

                if (pos.Y > (0.85 * ActualHeight))
                    keyLetter = Key.Down;
                else if (pos.Y < (0.20 * ActualHeight))
                    keyLetter = Key.Up;
                else
                    keyLetter = Key.None;
        }
        public void onTimedEvent(object sender, EventArgs e)
        {
            if (keyBlock != Key.None)
                changeLetter(keyBlock);

            if (keyLetter != Key.None)
                changeLetter(keyLetter);
        }
    }
}

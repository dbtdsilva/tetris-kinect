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
    /// Interaction logic for GameOverHighscore.xaml
    /// </summary>
    public partial class GameOverHighscore : Page, IPopup
    {
        private int finalscore;
        private int activeIndex;
        public GameOverHighscore(int finalscore)
        {
            InitializeComponent();
            score.Content = finalscore;
            this.finalscore = finalscore;
            changeLetter(0);
        }

        private void onSubmit(object sender, RoutedEventArgs e)
        {
            string name= "";

            int childrenCount = VisualTreeHelper.GetChildrenCount(lettersGrid);
            for (int i = 0; i < childrenCount; i++)
            {
                TextBlock child = (TextBlock) VisualTreeHelper.GetChild(lettersGrid, i);
                name += child.Text;
            }
            if (!TetrisM.getInstance().submitScore(finalscore, name))
               /* This is never supposed to return false since it is controlled by MainPage */
               throw new NotImplementedException();

            StartPopup page = new StartPopup();
            MainWindow.Instance.popPage(page);
            var window = MainWindow.GetWindow(this);
            window.KeyDown -= new KeyEventHandler(Key_Down);
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var window = MainWindow.GetWindow(this);
            window.KeyDown += new KeyEventHandler(Key_Down);
        }
        private void changeLetter(int new_index) {
            TextBlock letter = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, activeIndex);
            letter.Foreground = new SolidColorBrush(Colors.Black);

            activeIndex = new_index;
            letter = (TextBlock)VisualTreeHelper.GetChild(lettersGrid, new_index);
            letter.Foreground = new SolidColorBrush(Colors.White);
        }
        private void Key_Down(object sender, KeyEventArgs e)
        {   
            char c;
            TextBlock letter;
            switch (e.Key)
            {
                case Key.Down:
                    letter = (TextBlock) VisualTreeHelper.GetChild(lettersGrid, activeIndex);
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
                    return;
            }
            e.Handled = true;
        }
    }
}

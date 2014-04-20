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
        public GameOverHighscore(int finalscore)
        {
            InitializeComponent();
            score.Content = finalscore;
            this.finalscore = finalscore;
        }

        private void onSubmit(object sender, RoutedEventArgs e)
        {
            if (submit_name.Text.Length != 0)
            {
                if (!TetrisM.getInstance().submitScore(finalscore, submit_name.Text))
                    /* This is never supposed to return false since it is controlled by MainPage */
                    throw new NotImplementedException();
                StartPopup page = new StartPopup();
                MainWindow.Instance.popPage(page);
            }
        }
    }
}

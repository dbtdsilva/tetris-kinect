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

namespace Tetris.Pages
{
    public partial class Pause : Window
    {
        public Pause()
        {
            InitializeComponent();
        }

        private void onClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TetrisM.getInstance().pausePlay();
        }

        private void resumeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            TetrisM.getInstance().startGame();
            this.Close();
        }
    }
}

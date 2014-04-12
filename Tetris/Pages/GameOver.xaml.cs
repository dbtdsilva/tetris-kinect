﻿using System;
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
    /// <summary>
    /// Interaction logic for gameover.xaml
    /// </summary>
    public partial class GameOver : Page
    {
        public GameOver()
        {
            InitializeComponent();
        }

        private void onClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TetrisM.getInstance().startGame();
        }
    }
}

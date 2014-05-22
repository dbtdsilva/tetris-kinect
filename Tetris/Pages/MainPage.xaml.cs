using System;
using System.Collections.Generic;
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
using Tetris.TetrisModule;
using Tetris.TetrisModule.BlockModule;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using Microsoft.Kinect;
using System.Data;
using System.Collections;
using System.Windows.Threading;

namespace Tetris.Pages
{
    public partial class MainPage : Page, IMainPage
    {
        private Rectangle[,] tetrisTable;
        private Rectangle[,] nextBlockTable;
        private TetrisM tetris = TetrisM.getInstance();
        private bool tickedEnd = true;
        private bool moveDown = false;
        private static double kinectArrowsOpacity = 0.8;
        DispatcherTimer kinectSmoothLeftRight = new DispatcherTimer();
        DispatcherTimer kinectSmoothDown = new DispatcherTimer();
        DispatcherTimer startTimer = new DispatcherTimer();
        private int timeLeftToStart;

        TetrisM.Actions action;
        public MainPage()
        {
            InitializeComponent();

            kinectSmoothLeftRight.Tick += new EventHandler(timerTriggerLeftRight);
            kinectSmoothLeftRight.Interval = TimeSpan.FromMilliseconds(500);
            kinectSmoothLeftRight.Start();

            kinectSmoothDown.Tick += new EventHandler(timerTriggerDown);
            kinectSmoothDown.Interval = TimeSpan.FromMilliseconds(350);
            kinectSmoothDown.Start();

            startTimer.Tick += new EventHandler(startTick);
            startTimer.Interval = TimeSpan.FromMilliseconds(1000);

            // Events related with Tetris Module
            tetris.blockPaint += new TetrisM.BlockRepaintEventHandler(blockPaintEvent);
            tetris.blockMoved += new TetrisM.BlockMovedEventHandler(blockMovedEvent);
            tetris.tableChanged += new TetrisM.TableChangedEventHandler(tableChanged);
            tetris.rowComplete += new TetrisM.RowCompleteEventHandler(rowCompleteEvent);
            tetris.clockTick += new TetrisM.ClockTickEventHandler(clockTick);
            tetris.gameEnd += new TetrisM.GameEndEventHandler(gameEnded);
            tetris.nextBlockChanged += new TetrisM.NextBlockChangedEventHandler(nextBlockChanged);
            tetris.scoreChanged += new TetrisM.ScoreChangedEventHandler(scoreChanged);
            tetris.highscoreChanged += new TetrisM.HighscoresChangedEventHandler(highscoreChanged);
            tetris.pauseStatus += new TetrisM.PauseStatusEventHandler(pauseStatusChanged);

            createGrid();
            if (tetris.loadHighscores())
                highscoreChanged();

            StartPopup page = new StartPopup();
            MainWindow.Instance.popPage(page);
        }
        public void startGame()
        {
            paintGrid(true);
            timeLeftToStart = 3;
            ReadyTimeout.Content = timeLeftToStart;
            ReadyTimeout.Visibility = System.Windows.Visibility.Visible;
            startTimer.Start();

        }
        public void startTick(object sender, EventArgs e)
        {
            if (--timeLeftToStart == 0)
            {
                tetris.startGame();
                startTimer.Stop();
                ReadyTimeout.Visibility = System.Windows.Visibility.Hidden;
            }
            ReadyTimeout.Content = timeLeftToStart;
        }
        private void gridHighscores_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString() + ".";
        }
        private void highscoreChanged()
        {
            ArrayList list = new ArrayList();
            Score[] arr = tetris.getHighscores().getArray();

            for (int i = 0; i < tetris.getHighscores().Size; i++)
                list.Add(arr[i]);
            gridHighscores.ItemsSource = list;
        }
        public void clockTick(TimeSpan cTime)
        {
            TimeLeft.Content = String.Format("{0:m\\:ss}", cTime);
        }
        public void blockMovedEvent(Block currentBlock)
        {
            Point2D[] list = currentBlock.getList();
            Point2D pos = currentBlock.getPosition();
            for (int i = 0; i < list.Length; i++)
            {
                if (TetrisM.validPos(pos.X + list[i].X, pos.Y + list[i].Y))
                    tetrisTable[pos.X + list[i].X, pos.Y + list[i].Y].Fill = 
                        new SolidColorBrush(TetrisM.emptyBlock);
            }
        }
        public void blockPaintEvent(Block currentBlock)
        {
            Point2D[] list = currentBlock.getList();
            Point2D pos = currentBlock.getPosition();
            for (int i = 0; i < list.Length; i++)
            {
                if (TetrisM.validPos(pos.X + list[i].X, pos.Y + list[i].Y))
                    tetrisTable[pos.X + list[i].X, pos.Y + list[i].Y].Fill = 
                        new SolidColorBrush(currentBlock.getColor());
            }
        }
        private void createGrid()
        {
            MainGrid.Background = new SolidColorBrush(Colors.Black);

            tetrisTable = new Rectangle[TetrisM.NC, TetrisM.NR];
            TetrisGrid.Columns = TetrisM.NC;
            TetrisGrid.Rows = TetrisM.NR;

            TetrisGridBack.Columns = TetrisGrid.Columns;
            TetrisGridBack.Rows = TetrisGrid.Rows;

            for (int i = 0; i < TetrisM.NR; i++)
            {
                for (int j = 0; j < TetrisM.NC; j++)
                {
                    tetrisTable[j, i] = new Rectangle();
                    tetrisTable[j, i].Margin = new Thickness(0.5);
                    TetrisGrid.Children.Add(tetrisTable[j, i]);

                    TetrisGridBack.Children.Add(new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.WhiteSmoke),
                        Margin = new Thickness(0.5)
                    });
                }
            }

            /* Grid related with next block */
            nextBlockTable = new Rectangle[4, 2];
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    nextBlockTable[col, row] = new Rectangle();
                    nextBlockTable[col, row].StrokeThickness = 1;
                    Grid.SetRow(nextBlockTable[col, row], row);
                    Grid.SetColumn(nextBlockTable[col, row], col);
                    GridNextBlock.Children.Add(nextBlockTable[col, row]);
                }
            }
        }
        private void paintGrid(bool clearGrid = false)
        {
            Color[,] table = tetris.getTable();
            for (int i = 0; i < TetrisM.NC; i++)
            {
                for (int j = 0; j < TetrisM.NR; j++)
                {
                    tetrisTable[i, j].Fill = clearGrid ? new SolidColorBrush(TetrisM.emptyBlock) : new SolidColorBrush(table[i, j]);
                }
            }
        }
        private void scoreChanged(int score) {
            this.score.Content = score;
        }
        public void onKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    tetris.moveCurrentBlock(TetrisM.Actions.DOWN);  break;
                case Key.Left:
                    tetris.moveCurrentBlock(TetrisM.Actions.LEFT); break;
                case Key.Right:
                    tetris.moveCurrentBlock(TetrisM.Actions.RIGHT); break;
                case Key.Up:
                    tetris.moveCurrentBlock(TetrisM.Actions.ROTATE); break;
                case Key.Space:
                    tetris.moveCurrentBlock(TetrisM.Actions.F_DOWN); break;
                default:
                    return;
            }
            e.Handled = true;
        }
        public void rowCompleteEvent(int row) {
            paintGrid();
        }
        public void tableChanged() {
            paintGrid();
        }
        private void nextBlockChanged(Block nextBlock)
        {
            GridNextBlock.ColumnDefinitions.Clear();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    nextBlockTable[i, j].Fill = new SolidColorBrush(Colors.Transparent);
                    nextBlockTable[i, j].Stroke = new SolidColorBrush(Colors.Transparent);
                }
            }
            Point2D[] list = nextBlock.getList();
            Color c = nextBlock.getColor();
            int min = Int32.MaxValue;
            int max = Int32.MinValue;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].X < min)
                    min = list[i].X;
                if (list[i].X > max)
                    max = list[i].X;
            }
            int cols = max - min + 1;
            GridNextBlock.Width = ((MainGridNextBlock.ActualWidth - 5) / 4) * cols;
            while (cols < GridNextBlock.ColumnDefinitions.Count)
                GridNextBlock.ColumnDefinitions.RemoveAt(GridNextBlock.ColumnDefinitions.Count - 1);
            while (cols > GridNextBlock.ColumnDefinitions.Count)
                GridNextBlock.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < list.Length; i++)
            {
                nextBlockTable[list[i].X - min, list[i].Y + 1].Fill = new SolidColorBrush(c);
                nextBlockTable[list[i].X - min, list[i].Y + 1].Stroke = new SolidColorBrush(Colors.Black);
            }
        }
        public void pauseStatusChanged(bool pause)
        {
            if (pause)
            {
                MainWindow.Instance.KeyDown -= new KeyEventHandler(onKeyDown);
            }
            else
            {
                MainWindow.Instance.KeyDown += new KeyEventHandler(onKeyDown);
            }
        }
        public void gameEnded(int finalscore)
        {
            if (tetris.getHighscores().isHighscore(finalscore) && KinectSensor.KinectSensors.Count != 0)
            {
                GameOverHighscore submitPanel = new GameOverHighscore(finalscore);
                MainWindow.Instance.popPage(submitPanel);
            }
            else
            {
                GameOver gameover = new GameOver(finalscore);
                MainWindow.Instance.popPage(gameover);
            }
        }
        public void resumeGame()
        {
            if (timeLeftToStart != 0)
                startTimer.Start();
            else
                tetris.pausePlay();
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!startTimer.IsEnabled)
                tetris.pausePlay();
            else
                startTimer.Stop();
            Pause pause = new Pause();
            MainWindow.Instance.mainFrame.Navigate(pause);
        }
        private void HandPointerGrip(object sender, HandPointerEventArgs args)
        {
            tetris.moveCurrentBlock(TetrisM.Actions.ROTATE);
        }
        private void HandPointerMove(object sender, HandPointerEventArgs e)
        {
            /* 3 X Positions for Kinect Horizontal Movement */
            Point pos = e.HandPointer.GetPosition(this);
            if (pos.X > ActualWidth * 0.65)
                action = TetrisM.Actions.RIGHT;
            else if (pos.X < ActualWidth * 0.35)
                action = TetrisM.Actions.LEFT;
            else
                action = TetrisM.Actions.NONE;
            
            if ((pos.Y > (0.90 * ActualHeight)) && !tickedEnd)
            {
                tickedEnd = true;
                tetris.moveCurrentBlock(TetrisM.Actions.F_DOWN);
                moveDown = false;
            }
            else if (pos.Y > (0.65 * ActualHeight)) {
                moveDown = true;
            } 
            else
            {
                moveDown = false;
                tickedEnd = false;
            }
            drawMovement(action, moveDown);
        }
        private void drawMovement(TetrisM.Actions rightLeft, bool movingDown)
        {
            if (rightLeft == TetrisM.Actions.LEFT) {
                ArrowLeft.Opacity = kinectArrowsOpacity;
                ArrowRight.Opacity = 0;
            } else if (rightLeft == TetrisM.Actions.RIGHT) {
                ArrowLeft.Opacity = 0;
                ArrowRight.Opacity = kinectArrowsOpacity;
            } else {
                ArrowLeft.Opacity = 0;
                ArrowRight.Opacity = 0;
            }

            ArrowDown.Opacity = moveDown ? kinectArrowsOpacity : 0;
        }
        public void timerTriggerLeftRight(object sender, EventArgs e)
        {
            if (e != null)
                tetris.moveCurrentBlock(action);
        }
        public void timerTriggerDown(object sender, EventArgs e)
        {
            if (moveDown)
                tetris.moveCurrentBlock(TetrisM.Actions.DOWN);
        }
    }
}

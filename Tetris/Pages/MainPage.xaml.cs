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

namespace Tetris.Pages
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, IMainPage
    {
        private Rectangle[,] tetrisTable;
        private Rectangle[,] nextBlockTable;
        private TetrisM tetris = TetrisM.getInstance();
        private bool tickedEnd = true;
        
        private readonly KinectSensorChooser sensorChooser;
        
        public MainPage()
        {
            InitializeComponent();
            // Initialize the sensor chooser and UI
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();

            // Bind the sensor chooser's current sensor to the KinectRegion
            var regionSensorBinding = new Binding("Kinect") { Source = this.sensorChooser };
            BindingOperations.SetBinding(this.kinectRegion, KinectRegion.KinectSensorProperty, regionSensorBinding);

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

            createGrid();
            if (tetris.loadHighscores())
                highscoreChanged();
            tetris.startGame();

        }
        private void gridHighscores_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString() + ".";
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var window = MainWindow.GetWindow(this);
            window.KeyDown += new KeyEventHandler(onKeyDown);
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
            nextBlockTable = new Rectangle[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    nextBlockTable[j, i] = new Rectangle();
                    NextBlockGrid.Children.Add(nextBlockTable[j, i]);

                }
            }
        }
        private void paintGrid()
        {
            Color[,] table = tetris.getTable();
            for (int i = 0; i < TetrisM.NC; i++)
            {
                for (int j = 0; j < TetrisM.NR; j++)
                {
                    tetrisTable[i, j].Fill = new SolidColorBrush(table[i, j]);
                }
            }
        }
        private void scoreChanged(int score) {
            this.score.Content = score;
        }
        private void onKeyDown(object sender, KeyEventArgs e)
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
            Point2D[] list = nextBlock.getList();
            Color c = nextBlock.getColor();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    nextBlockTable[i, j].Fill = new SolidColorBrush(Colors.Transparent);
                }
            }
            for (int i = 0; i < list.Length; i++)
            {
                nextBlockTable[list[i].X + 1, list[i].Y + 2].Fill = new SolidColorBrush(c);
            }
        }

        public void gameEnded(int finalscore)
        {
            if (tetris.getHighscores().isHighscore(finalscore))
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
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            tetris.pausePlay();
            Pause pause = new Pause();
            pause.bindSensor(sensorChooser);
            MainWindow.Instance.mainFrame.Navigate(pause);
        }
        private void HandPointerGrip(object sender, HandPointerEventArgs args)
        {
            tetris.moveCurrentBlock(TetrisM.Actions.ROTATE);
        }
        private void HandPointerMove(object sender, HandPointerEventArgs e)
        {
            Point pos = e.HandPointer.GetPosition(this);
            
            int newpos = (int) (pos.X / (ActualWidth / TetrisM.NC));
            int currentPos = tetris.getCurrentBlock().getPosition().X;
            if (newpos != currentPos) {
                if (newpos > currentPos)
                {
                    for (int i = 0; i < (newpos - currentPos); i++)
                    {
                        tetris.moveCurrentBlock(TetrisM.Actions.RIGHT);
                    }
                }
                else
                {
                    for (int i = 0; i < (currentPos - newpos); i++)
                    {
                        tetris.moveCurrentBlock(TetrisM.Actions.LEFT);
                    }
                }
                currentPos = newpos;
            }
            if (pos.Y > 700 && !tickedEnd)
            {
                tickedEnd = true;
                tetris.moveCurrentBlock(TetrisM.Actions.F_DOWN);
            }
            if (pos.Y < 400)
                tickedEnd = false;
        }
        private static void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    try
                    {
                        args.NewSensor.DepthStream.Range = DepthRange.Near;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Non Kinect for Windows devices do not support Near mode, so reset back to default mode.
                        args.NewSensor.DepthStream.Range = DepthRange.Default;
                        args.NewSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }
        }
    }
}

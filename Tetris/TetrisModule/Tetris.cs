using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.TetrisModule.BlockModule;
using Tetris.TetrisModule.BlockModule.SingularBlocks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.Runtime.Serialization.Formatters.Binary;

/********************************************
 *                                          *
 *      Author: Diogo Silva (60337)         *
 *      Tetris developed for Kinect         *
 *                                          *
 ********************************************/
namespace Tetris.TetrisModule
{
    class TetrisM       /* Implementing Tetris class as a singleton */
    {
        /*** Events related with game actions/changes ***/
        public delegate void TableChangedEventHandler();
        public event TableChangedEventHandler tableChanged;
        public delegate void BlockMovedEventHandler(Block currentBlock);
        public event BlockMovedEventHandler blockMoved;
        public delegate void BlockRepaintEventHandler(Block currentBlock);
        public event BlockRepaintEventHandler blockPaint;
        public delegate void NextBlockChangedEventHandler(Block nextBlock);
        public event NextBlockChangedEventHandler nextBlockChanged;
        public delegate void RowCompleteEventHandler(int row);
        public event RowCompleteEventHandler rowComplete;
        public delegate void ClockTickEventHandler(TimeSpan currentTime);
        public event ClockTickEventHandler clockTick;
        public delegate void GameEndEventHandler(int finalscore);
        public event GameEndEventHandler gameEnd;
        public delegate void ScoreChangedEventHandler(int score);
        public event ScoreChangedEventHandler scoreChanged;
        public delegate void HighscoresChangedEventHandler();
        public event HighscoresChangedEventHandler highscoreChanged;

        /*** Public stats related with the game */
        public static int NR = 20;                                  /* Number of rows */
        public static int NC = 10;                                  /* Number of columns */
        public static Color emptyBlock = Colors.Transparent;        /* Default color */
        public enum Actions { LEFT, DOWN, F_DOWN, RIGHT, ROTATE };     /* Available actions */
        
        private static TetrisM instance;        /* Private and static instance -> Singleton implementation */

        private DispatcherTimer slideTimer;     /* Timer to make block go down after a certain amount of time */
        private DispatcherTimer timeOut;        /* Timer to control a Tetris game (2 minutes default) */
        private TimeSpan secondsLeft;           /* Temporary TimeSpan to control the time left */
        private Block nextBlock;                /* Next block that will appear */
        private Block currentBlock;             /* Current block on the table */
        private GhostBlock gblock;              /* Ghost block */
        private bool gblock_act = true;         /* Gblock status */
        private Color[,] table;                 /* Tetris table */
        private bool paused = true;             /* Flag to show when game is paused or not */

        private const string FileName = @"highscores.bin";  /* Filename for highscores serialization */

        private HighScore highscoreTable;       /* Highscores table being saved with serialization */
        private int currentScore;               /* Temporary variable to store game score */

        /****** SINGLETON IMPLEMENTATION ******/
        private TetrisM()
        {
            slideTimer = new DispatcherTimer();
            slideTimer.Tick += new EventHandler(onTimedEvent);
            slideTimer.Interval = TimeSpan.FromMilliseconds(750);

            timeOut = new DispatcherTimer();
            timeOut.Tick += new EventHandler(onTimeOut);
            timeOut.Interval = TimeSpan.FromSeconds(1);
            /* Time out function with a interval of 1 second to update visual effects */
            
            table = new Color[NC, NR];
            highscoreTable = new HighScore(10);
        }
        /****** PUBLIC FUNCTIONS ******/
        public static TetrisM getInstance()
        {
            if (instance == null)
            {
                instance = new TetrisM();
            }
            return instance;
        }
        public Color[,] getTable()
        {
            return table;
        }
        public void startGame()
        {
            currentScore = 0;
            if (scoreChanged != null) scoreChanged(currentScore);
            paused = false;

            currentBlock = BlockFactory.generateBlock();
            nextBlock = BlockFactory.generateBlock();

            if (nextBlockChanged != null) nextBlockChanged(nextBlock);

            clearTable();
            if (tableChanged != null) tableChanged();
            if (blockPaint != null)
                blockPaint(currentBlock);
            checkGhostBlock();
            
            secondsLeft = new TimeSpan(0, 2, 0);
            if (clockTick != null) 
                clockTick(secondsLeft);
            slideTimer.Start();
            timeOut.Start();
        }
        public void saveHighscores()
        {
            Stream FileStream = File.Create(FileName);
            BinaryFormatter serializer = new BinaryFormatter();
            serializer.Serialize(FileStream, highscoreTable);
            FileStream.Close();
        }
        public bool loadHighscores()
        {
            if (File.Exists(FileName))
            {
                Stream FileStream = File.OpenRead(FileName);
                BinaryFormatter deserializer = new BinaryFormatter();
                highscoreTable = (HighScore)deserializer.Deserialize(FileStream);
                FileStream.Close();
                return true;
            }
            return false;
        }
        public HighScore getHighscores() {
            return highscoreTable;
        }
        public bool isHighscore(int score)
        {
            return highscoreTable.checkScore(score);
        }
        public bool submitScore(int score, string name)
        {
            if (!highscoreTable.addScore(score, name))
                return false;
            if (highscoreChanged != null)
                highscoreChanged();
            return true;
        }
        public void pausePlay()
        {
            paused = !paused;
            if (paused)
            {
                timeOut.Stop();
                slideTimer.Stop();
            }
            else
            {
                timeOut.Start();
                slideTimer.Start();
            }
        }
        public void moveCurrentBlock(Actions e, bool forced = false)
        {
            if (paused) return;                                 /* Doesn't allow any movements when paused */

            if (checkCollisions(e, currentBlock))               /* Check for any colisions */
            {
                if (e == Actions.DOWN)                          /* If there's colisions on bottom */
                    newBlock();                                 /* Create a new block */
                return;                                         /* Doesn't allow any movements when there's colisions */
            }

            if (blockMoved != null) blockMoved(currentBlock);   /* If there's no colisions, means that block will move */

            if (e == Actions.F_DOWN)                            /* F_DOWN action throws block to the end of the table */
            {
                while (!checkCollisions(Actions.DOWN, currentBlock))    /* Move block down till find any colision */
                {
                    changeScore(currentScore + 1);                      /* Score is double than normal */
                    moveCurrentBlock(Actions.DOWN);                     /* Move current block */
                }
                newBlock();                                     /* And ask for a new block, no need to wait */
            } 
            else if (e == Actions.DOWN)                         /* DOWN action, moves block down */
            {
                if (!forced)                                    /* If this action is not forced by the system */
                    changeScore(currentScore + 1);              /* Then increase score */
                currentBlock.moveDown();                    
                slideTimer.Stop();                              /* Refresh slider timer to prevent double down block */
                slideTimer.Start();                             /* One forced and other not-forced */
            }
            else if (e == Actions.LEFT)                         /* LEFT action, moves block to the left */
                currentBlock.moveLeft();
            else if (e == Actions.RIGHT)                        /* RIGHT action, moves block to the right */
                currentBlock.moveRight();
            else                                                /* ROTATE action, rotates block clockwise */
                currentBlock.rotate();

            if (e == Actions.LEFT || e == Actions.RIGHT || e == Actions.ROTATE)
                checkGhostBlock();
            if (blockPaint != null) blockPaint(currentBlock);         /* Current block needs to be painted */
        }
        public Block getCurrentBlock() {
            return currentBlock;
        }
        public static bool validPos(int x, int y)
        {
            if (x >= NC || x < 0 || y >= NR || y < 0)
                return false;
            return true;
        }
        public int getCurrentScore()
        {
            return currentScore;
        }
        /****** PRIVATE FUNCTIONS ******/
        private void newBlock()
        {
            fillTableWithBlock();

            int row;
            int linesNumber = 0;
            /* Check for lines complete */
            while ((row = checkForLinesComplete()) != -1)
            {
                linesNumber += 1;
                deleteRow(row);
                rowComplete(row);
            }
            /* If there is lines complete, increase current score */
            switch (linesNumber)
            {
                case 1: changeScore(currentScore + 100); break;
                case 2: changeScore(currentScore + 300); break;
                case 3: changeScore(currentScore + 500); break;
                case 4: changeScore(currentScore + 800); break;
            }

            currentBlock = nextBlock;
            /* Check if there's collision right before block appears, it means a game over */
            Point2D[] list = currentBlock.getList();
            Point2D pos = currentBlock.getPosition();
            for (int i = 0; i < list.Length; i++)
            {
                if (validPos(list[i].X + pos.X, list[i].Y + pos.Y) &&
                    table[list[i].X + pos.X, list[i].Y + pos.Y] != emptyBlock)
                {
                    gameFinished();
                    return;
                }
            }
            gblock = new GhostBlock(currentBlock.getList());
            checkGhostBlock();
            nextBlock = BlockFactory.generateBlock();
            if (nextBlockChanged != null) nextBlockChanged(nextBlock);
            if (blockPaint != null) blockPaint(currentBlock);
        }

        private int checkForLinesComplete()
        {
            bool complete;
            for (int row = 0; row < NR; row++)
            {
                complete = true;
                for (int col = 0; col < NC; col++)
                {
                    if (table[col, row] == emptyBlock)
                        complete = false;
                }
                if (complete)
                    return row;
            }
            return -1;
        }
        private void clearTable()
        {
            for (int i = 0; i < NC; i++)
            {
                for (int j = 0; j < NR; j++)
                {
                    table[i, j] = emptyBlock;
                }
            }
            if (tableChanged != null) tableChanged();
        }
        private void fillTableWithBlock()
        {
            Point2D[] list = currentBlock.getList();
            Point2D pos = currentBlock.getPosition();
            for (int i = 0; i < list.Length; i++)
            {
                if (pos.Y + list[i].Y >= 0)
                    table[pos.X + list[i].X, pos.Y + list[i].Y] = currentBlock.getColor();
            }
        }
        private bool checkCollisions(Actions direction, Block block)
        {
            Point2D[] list = block.getList();
            Point2D pos = block.getPosition();
            int x, y;
            switch (direction)
            {
                case Actions.DOWN:
                    for (int i = 0; i < list.Length; i++)
                    {
                        x = list[i].X + pos.X;
                        y = list[i].Y + pos.Y + 1;
                        if (y >= NR)
                            return true;
                        if (validPos(x, y) && table[x, y] != emptyBlock)
                            return true;
                    }
                    break;

                case Actions.LEFT:
                    for (int i = 0; i < list.Length; i++)
                    {
                        x = list[i].X + pos.X - 1;
                        y = list[i].Y + pos.Y;

                        if (x < 0)
                            return true;
                        if (validPos(x, y) && table[x, y] != emptyBlock)
                            return true;
                    }
                    break;

                case Actions.RIGHT:
                    for (int i = 0; i < list.Length; i++)
                    {
                        x = list[i].X + pos.X + 1;
                        y = list[i].Y + pos.Y;
                        if (x >= NC)
                            return true;
                        if (validPos(x, y) && table[x, y] != emptyBlock)
                            return true;
                    }
                    break;
                case Actions.ROTATE:
                    Point2D[] preview = block.rotatePreviewList();
                    int collidePos = 0;
                    for (int i = 0; i < preview.Length; i++)
                    {
                        x = preview[i].X + pos.X;
                        y = preview[i].Y + pos.Y;
                        if (x >= NC && (NC - x - 1) < collidePos)
                            collidePos = NC - x - 1;
                        else if (x < 0 && (-x) > collidePos)
                            collidePos = -x;
                        else if (y >= NR)
                            return true;
                    }
                    for (int i = 0; i < preview.Length; i++)
                    {
                        x = preview[i].X + pos.X + collidePos;
                        y = preview[i].Y + pos.Y;
                        if (validPos(x, y) && table[x, y] != emptyBlock)
                            return true;
                    }

                    while (collidePos != 0)
                    {
                        if (collidePos > 0) {
                            collidePos--;
                            moveCurrentBlock(Actions.RIGHT);
                        } else {
                            collidePos++;
                            moveCurrentBlock(Actions.LEFT);
                        }
                    }  
                    break;
            }
            return false;
        }
        private void checkGhostBlock()
        {
            if (!gblock_act)
                return;
            if (blockMoved != null && gblock != null) blockMoved(gblock);

            gblock = new GhostBlock(currentBlock.getList());
            gblock.setPosition(currentBlock.getPosition());

            while (!checkCollisions(Actions.DOWN, gblock))
                gblock.moveDown();

            if (blockPaint != null) blockPaint(gblock);
        }
        private void deleteRow(int rowClear)
        {
            for (int row = rowClear; row > 0; row--)
                for (int col = 0; col < NC; col++)
                    table[col, row] = table[col, row - 1];

            for (int col = 0; col < NC; col++)
                table[col, 0] = emptyBlock;

            if (tableChanged != null) tableChanged();
        }
        private void onTimedEvent(object sender, EventArgs e)   /* When slider timer is gone FUNCTION */
        {
            moveCurrentBlock(TetrisM.Actions.DOWN, true);       /* Force movement when player doesn't move block */
        }

        private void onTimeOut(object sender, EventArgs e)      /* Timer Interval of 1 second (GAME TIMER) FUNCTION */
        {
            secondsLeft = secondsLeft.Subtract(TimeSpan.FromSeconds(1));    /* Decrease 1 second to timeLeft */
            if (clockTick != null) clockTick(secondsLeft);                  /* Reproduce event with time remaining */

            if (secondsLeft.TotalSeconds == 0)                              /* When time is gone */
                gameFinished();                                             /* Game over */
        }
        private void gameFinished()                         /* Game over FUNCTION */
        {
            timeOut.Stop();                                 /* Stop all timers related with the game */
            slideTimer.Stop();                              /* Slider and time out */
            paused = true;                                  /* Flag paused to prevent actions from user on game */

            if (gameEnd != null) gameEnd(currentScore);     /* Reproduce event with final score */
            
            nextBlock = null;                               /* Make objects pointing to null */
            gblock = null;                                  /* Except currentBlock that still have to be painted on table */
        }
        private void changeScore(int newscore)                      /* Change Score FUNCTION */
        {
            currentScore = newscore;                                /* Refresh value */
            if (scoreChanged != null) scoreChanged(newscore);       /* Call event */
        }
    }
}

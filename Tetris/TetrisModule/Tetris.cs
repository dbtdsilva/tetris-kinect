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
        public delegate void PauseStatusEventHandler(bool pause);
        public event PauseStatusEventHandler pauseStatus;

        /*** Public stats related with the game */
        public static int NR = 20;                                  /* Number of rows */
        public static int NC = 10;                                  /* Number of columns */
        public static Color emptyBlock = Colors.Transparent;        /* Default color */
        public enum Actions { NONE, LEFT, DOWN, F_DOWN, RIGHT, ROTATE };     /* Available actions */
        
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
        public static TetrisM getInstance()     /* METHOD Getter for instance (Singleton) */
        {
            if (instance == null)
                instance = new TetrisM();
            return instance;
        }
        public Color[,] getTable()              /* METHOD Getter for table */
        {
            return table;
        }
        public void startGame()                 /* METHOD Start tetris game */
        {
            currentScore = 0;                                           /* Reset current score */
            if (scoreChanged != null) scoreChanged(currentScore);       /* EVENT - Inform about the score reset */
            changePauseStatus(false);                                   /* Put flag pause to false */

            clearTable();                                               /* Clear table from previous games */
            if (tableChanged != null) tableChanged();                   /* EVENT - Inform that all table may be changed */
            currentBlock = BlockFactory.generateBlock();                /* Generate current block */
            nextBlock = BlockFactory.generateBlock();                   /* Generate next block */
            if (nextBlockChanged != null) nextBlockChanged(nextBlock);  /* EVENT - Inform that next block changed */
            if (blockPaint != null) blockPaint(currentBlock);           /* EVENT - Inform that current block changed */

            checkGhostBlock();                                          /* First check on ghost block */
            
            secondsLeft = new TimeSpan(0, 2, 0);                        /* Create a time span to control the time over */
            if (clockTick != null) clockTick(secondsLeft);              /* EVENT - Inform the time left */
            slideTimer.Start();                                         /* Starts slide timer */
            timeOut.Start();                                            /* Start time over timer */
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
        public HighScore getHighscores() {                      /* METHOD - Getter for highscores table */
            return highscoreTable;
        }
        public bool submitScore(int score, string name)         /* METHOD - Submit a score to the table */
        {
            if (!highscoreTable.addScore(score, name))          /* If that score is not higher that any in the table */
                return false;                                   /* Otherwise, it will insert */
            if (highscoreChanged != null) highscoreChanged();   /* EVENT - Inform about highscores changes */
            return true;
        }
        public void pausePlay()                                 /* METHOD - Pause/Play the game */
        {
            changePauseStatus(!paused);                         /* Change current status */
            if (paused)                                         /* If it changed to pause mode */
            {
                timeOut.Stop();
                slideTimer.Stop();
            }
            else                                                /* If it changed to play mode */
            {
                timeOut.Start();
                slideTimer.Start();
            }
        }
        public void moveCurrentBlock(Actions e, bool forced = false)    /* METHOD - Move current block */
        {
            if (paused || e == Actions.NONE) return;            /* Doesn't allow any movements when paused */

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
                if (blockPaint != null) blockPaint(currentBlock);   /* Paint block before create a new one */
                newBlock();                                         /* And ask for a new block, no need to wait */
                return;
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
                checkGhostBlock();                              /* Ghost block only changes when moving left, right or rotate */
            if (blockPaint != null) blockPaint(currentBlock);         /* Current block needs to be painted */
        }
        public Block getCurrentBlock()                          /* METHOD - Getter for current block */
        {                        
            return currentBlock;
        }
        public int getCurrentScore()                            /* METHOD - Getter for current score */
        {
            return currentScore;
        }
        public bool getGBlockStatus()
        {
            return gblock_act;
        }
        public void changeGBlockStatus()
        {
            gblock_act = !gblock_act;
            if (gblock_act)
                checkGhostBlock();
            else
                blockMoved(gblock);
        }
        public static bool validPos(int x, int y)               /* METHOD static - to find out valid positions */
        {
            if (x >= NC || x < 0 || y >= NR || y < 0)
                return false;
            return true;
        }
        /****** PRIVATE FUNCTIONS ******/
        private void newBlock()                                 /* METHOD - to create a new block */
        {
            slideTimer.Stop();
            fillTableWithBlock();                               /* Before anything, put old block on table with blocks 
                                                                 * It will be useful to check colisions */
            /* Check for lines complete */
            int row, linesNumber = 0;
            while ((row = checkForLinesComplete()) != -1)
            {
                linesNumber += 1;
                deleteRow(row);                                 /* Delete that row from table */ 
                if (rowComplete != null) rowComplete(row);      /* Call event to inform that row is gone */
            }
            /* If there is lines complete, increase current score */
            switch (linesNumber)
            {
                case 1: changeScore(currentScore + 100); break;
                case 2: changeScore(currentScore + 300); break;
                case 3: changeScore(currentScore + 500); break;
                case 4: changeScore(currentScore + 800); break;
            }

            currentBlock = nextBlock;                           /* Current block gets the value on nextBlock */
            /* Check if there's collision right before block appears, it means that game is over */
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
            /* If there is no game over yet */
            gblock = new GhostBlock(currentBlock.getList());            /* Generate Ghost Block */
            checkGhostBlock();                                          /* Check his position */
            nextBlock = BlockFactory.generateBlock();                   /* Generate next block */
            if (nextBlockChanged != null) nextBlockChanged(nextBlock);  /* Call event informing that next block changed */
            if (blockPaint != null) blockPaint(currentBlock);           /* Call event informing that current block also changed */
            slideTimer.Start();
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
            if (direction == Actions.NONE)
                return false;
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
            changePauseStatus(true);                        /* Flag paused to prevent actions from user on game */

            if (gameEnd != null) gameEnd(currentScore);     /* Reproduce event with final score */
            
            nextBlock = null;                               /* Make objects pointing to null */
            gblock = null;                                  /* Except currentBlock that still have to be painted on table */
        }
        private void changeScore(int newscore)                      /* Change Score FUNCTION */
        {
            currentScore = newscore;                                /* Refresh value */
            if (scoreChanged != null) scoreChanged(newscore);       /* Call event */
        }
        private void changePauseStatus(bool pause)
        {
            paused = pause;
            if (pauseStatus != null)
                pauseStatus(paused);
        }
    }
}

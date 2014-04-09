using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.TetrisModule.SingularBlocks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

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
        public delegate void GameEndEventHandler();
        public event GameEndEventHandler gameEnd;

        /*** Public stats related with the game */
        public static int NR = 20;                                  /* Number of rows */
        public static int NC = 10;                                  /* Number of columns */
        public static Color emptyBlock = Colors.Transparent;        /* Default color */
        public enum Actions { LEFT, DOWN, RIGHT, ROTATE };     /* Available actions */
        
        private static TetrisM instance;    /* Private and static instance -> Singleton implementation */

        private DispatcherTimer slideTimer;     /* Timer to make block go down after a certain amount of time */
        private DispatcherTimer timeOut;        /* Timer to control a Tetris game (2 minutes default) */
        private TimeSpan secondsLeft;           /* Temporary TimeSpan to control the time left */
        private Block nextBlock;                /* Next block that will appear */
        private Block currentBlock;             /* Current block on the table */
        private Color[,] table;                 /* Tetris table */
        private bool paused;

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
        }
        public static TetrisM getInstance()
        {
            if (instance == null)
            {
                instance = new TetrisM();
            }
            return instance;
        }
        public void startGame()
        {
            paused = false;
            currentBlock = BlockFactory.generateBlock();
            nextBlock = BlockFactory.generateBlock();
            if (nextBlockChanged != null) nextBlockChanged(nextBlock);

            clearTable();
            if (tableChanged != null) tableChanged();
            if (blockPaint != null) blockPaint(currentBlock);
            secondsLeft = new TimeSpan(0, 2, 0);
            if (clockTick != null) clockTick(secondsLeft);
            slideTimer.Start();
            timeOut.Start();
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
        public void moveCurrentBlock(Actions e)
        {
            if (paused) return;

            if (checkCollisions(e))
            {
                if (e == Actions.DOWN)
                    newBlock();
                return;
            }

            if (blockMoved != null) blockMoved(currentBlock);

            if (e == Actions.DOWN)
            {
                currentBlock.moveDown();
                slideTimer.Stop();              /* Reset timer */
                slideTimer.Start();
            }
            else if (e == Actions.LEFT)
                currentBlock.moveLeft();
            else if (e == Actions.RIGHT)
                currentBlock.moveRight();
            else
                currentBlock.rotate();

            if (blockPaint != null) blockPaint(currentBlock);
        }
        public static bool validPos(int x, int y)
        {
            if (x >= NC || x < 0 || y >= NR || y < 0)
                return false;
            return true;
        }
        private void newBlock()
        {
            fillTableWithBlock();
            int row;
            while ((row = checkForLinesComplete()) != -1)
            {
                deleteRow(row);
                rowComplete(row);
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
        public Color[,] getTable()
        {
            return table;
        }
        private bool checkCollisions(Actions direction)
        {
            Point2D [] list = currentBlock.getList();
            Point2D pos = currentBlock.getPosition();
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
                    Point2D[] preview = currentBlock.rotatePreviewList();
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

        private void deleteRow(int rowClear)
        {
            for (int row = rowClear; row > 0; row--)
            {
                for (int col = 0; col < NC; col++)
                {
                    table[col, row] = table[col, row - 1];
                }
            }
            for (int col = 0; col < NC; col++) {
                table[col, 0] = emptyBlock;
            }

            if (tableChanged != null) tableChanged();
        }
        private void onTimedEvent(object sender, EventArgs e)
        {
            moveCurrentBlock(TetrisM.Actions.DOWN);
        }

        private void onTimeOut(object sender, EventArgs e)
        {
            secondsLeft = secondsLeft.Subtract(TimeSpan.FromSeconds(1));
            if (clockTick != null) clockTick(secondsLeft);

            if (secondsLeft.TotalSeconds == 0)
                gameFinished();
        }
        private void gameFinished()
        {
            timeOut.Stop();
            slideTimer.Stop();
            paused = true;

            if (gameEnd != null) gameEnd();
        }
    }
}

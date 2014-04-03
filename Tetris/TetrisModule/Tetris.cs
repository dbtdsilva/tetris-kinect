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
    class TetrisM /* Implementing Tetris class as a singleton */
    {
        public delegate void TableChangedEventHandler();
        public event TableChangedEventHandler tableChanged;
        public delegate void BlockMovedEventHandler(Block currentBlock);
        public event BlockMovedEventHandler blockMoved;
        public delegate void BlockRepaintEventHandler(Block currentBlock);
        public event BlockRepaintEventHandler blockPaint;
        public delegate void RowCompleteEventHandler(int row);
        public event RowCompleteEventHandler rowComplete;

        public static int NR = 20;
        public static int NC = 10;
        public static Color emptyBlock = Colors.Transparent;
        public enum MovePosition { LEFT, DOWN, RIGHT, ROTATE };
        
        private static TetrisM instance;

        DispatcherTimer slideTimer;
        private Block nextBlock;
        private Block currentBlock;
        private Color[,] table;

        private TetrisM()
        {
            slideTimer = new DispatcherTimer();
            slideTimer.Tick += new EventHandler(onTimedEvent);
            slideTimer.Interval = TimeSpan.FromMilliseconds(750);

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
            nextBlock = BlockFactory.generateBlock();

            if (blockPaint != null) 
                blockPaint(currentBlock);
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
        public void clearTable()
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
        public void startGame()
        {
            currentBlock = BlockFactory.generateBlock();
            nextBlock = BlockFactory.generateBlock();

            clearTable();
            if (blockPaint != null) blockPaint(currentBlock);

            slideTimer.Start();
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
        public void moveCurrentBlock(MovePosition e)
        {
            if (checkCollisions(e))
            {
                if (e == MovePosition.DOWN)
                    newBlock();
                return;
            }

            if (blockMoved != null) blockMoved(currentBlock);

            if (e == MovePosition.DOWN)
            {
                currentBlock.moveDown();
                slideTimer.Stop();
                slideTimer.Start();
            }
            else if (e == MovePosition.LEFT)
                currentBlock.moveLeft();
            else if (e == MovePosition.RIGHT)
                currentBlock.moveRight();
            else
                currentBlock.rotate();

            if (blockPaint != null) blockPaint(currentBlock);
        }

        public bool checkCollisions(MovePosition direction)
        {
            Point2D [] list = currentBlock.getList();
            Point2D pos = currentBlock.getPosition();
            int x, y;
            switch (direction)
            {
                case MovePosition.DOWN:
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

                case MovePosition.LEFT:
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

                case MovePosition.RIGHT:
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
                case MovePosition.ROTATE:
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
                            moveCurrentBlock(MovePosition.RIGHT);
                        } else {
                            collidePos++;
                            moveCurrentBlock(MovePosition.LEFT);
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
            moveCurrentBlock(TetrisM.MovePosition.DOWN);
        }

        public static bool validPos(int x, int y)
        {
            if (x >= NC || x < 0 || y >= NR || y < 0)
                return false;
            return true;
        }
    }
}

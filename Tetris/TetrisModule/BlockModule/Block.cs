using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Tetris.TetrisModule.BlockModule
{
    public abstract class Block
    {
        private Point2D pos;
        private Color blockColor;
        private Point2D[] list;

        public Block(Point2D[] list, Color blockColor)
        {
            if (list.Length != 4)
                throw new InvalidOperationException("Each tetris block contains only 4 parts");

            pos = new Point2D((TetrisM.NC-1) / 2, 0);
            this.list = new Point2D[4];
            for (int i = 0; i < list.Length; i++)
                this.list[i] = new Point2D(list[i]);

            this.blockColor = blockColor;
        }
        public Block(Block block)
        {
            this.pos = block.pos;
            this.blockColor = block.blockColor;
            this.list = new Point2D[4];
            for (int i = 0; i < list.Length; i++)
                this.list[i] = new Point2D(list[i]);
        }
        public Point2D[] getList()
        {
            return list;
        }
        public Color getColor()
        {
            return blockColor;
        }
        public Point2D getPosition()
        {
            return pos;
        }
        public void setPosition(Point2D p)
        {
            pos = new Point2D(p);
        }
        virtual public void rotate()
        {
            int temp;
            for (int i = 0; i < list.Length; i++)
            {
                temp = list[i].X;
                list[i].X = -1 * list[i].Y;
                list[i].Y = temp;
            }
        }
        public Point2D[] rotatePreviewList()
        {
            Point2D[] temp = new Point2D[4];
            for (int i = 0; i < list.Length; i++)
                temp[i] = new Point2D(-1 * list[i].Y, list[i].X);
            return temp;
        }
        public void moveDown()
        {
            pos.Y++;
        }
        public void moveLeft()
        {
            pos.X--;
        }
        public void moveRight()
        {
            pos.X++;
        }
    }
}

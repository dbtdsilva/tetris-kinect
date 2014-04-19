using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.TetrisModule
{
    public class Point2D
    {
        public int X, Y;
        public Point2D(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public Point2D(Point2D p)
        {
            this.X = p.X;
            this.Y = p.Y;
        }
        public static Point2D operator +(Point2D p1, Point2D p2)
        {
            return new Point2D(p1.X + p2.X, p1.Y + p2.Y);
        }
        public static Point2D operator -(Point2D p1, Point2D p2)
        {
            return new Point2D(p1.X - p2.X, p1.Y - p2.Y);
        }
        public override string ToString()
        {
            return "x: " + X + ", y: " + Y;
        }
    }
}

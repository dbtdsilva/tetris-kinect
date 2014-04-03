using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Tetris.TetrisModule.SingularBlocks
{
    class BlockO : Block
    {
        public BlockO() : base(new Point2D[4] {
                            new Point2D(0,-1),
                            new Point2D(0,0),
                            new Point2D(1,-1),
                            new Point2D(1,0)
                        } , Colors.Yellow)
        {
        }

        public override void rotate()
        {
            return;
        }
    }
}

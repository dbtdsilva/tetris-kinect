using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Tetris.TetrisModule.BlockModule.SingularBlocks
{
    class BlockI : Block
    {
        public BlockI() : base(new Point2D[4] {
                            new Point2D(-1,0),
                            new Point2D(-0,0),
                            new Point2D(1,0),
                            new Point2D(2,0)
                        }, Colors.Cyan)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Tetris.TetrisModule.BlockModule
{
    class GhostBlock : Block
    {
        public GhostBlock(Point2D[] list)
            : base(list, Colors.Gray)
        {
        }
    }
}

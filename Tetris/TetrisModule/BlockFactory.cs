using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.TetrisModule.SingularBlocks;

namespace Tetris.TetrisModule
{
    class BlockFactory
    {
        public static Block generateBlock()
        {
            switch (new Random().Next(7))
            {
                case 0: return new BlockI();
                case 1: return new BlockJ();
                case 2: return new BlockL();
                case 3: return new BlockO();
                case 4: return new BlockS();
                case 5: return new BlockT();
                case 6: return new BlockZ();
                default: throw new NotImplementedException();
            }
        }
    }
}

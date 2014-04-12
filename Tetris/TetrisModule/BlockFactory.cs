using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.TetrisModule.SingularBlocks;

namespace Tetris.TetrisModule
{
    class BlockFactory
    {
        // Algorithm used to generate tetris blocks
        // Basically it picks all seven different blocks and gives it in a random order
        static bool[] done = new bool[7];
        public static Block generateBlock()
        {
            bool temp = false;
            // Check if there is any block left to give
            for (int i = 0; i < done.Length; i++)
                if (done[i] == false)
                    temp = true;

            // If there is no block left, reset array
            if (!temp)
                for (int i = 0; i < done.Length; i++)
                    done[i] = false;

            int value;
            do {
                value = new Random().Next(7);
            } while (done[value]);

            switch (value)
            {
                case 0: done[0] = true; return new BlockI();
                case 1: done[1] = true; return new BlockJ();
                case 2: done[2] = true; return new BlockL();
                case 3: done[3] = true; return new BlockO();
                case 4: done[4] = true; return new BlockS();
                case 5: done[5] = true; return new BlockT();
                case 6: done[6] = true; return new BlockZ();
                default: throw new NotImplementedException();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tetris.TetrisModule.BlockModule.SingularBlocks;

namespace Tetris.TetrisModule.BlockModule
{
    class BlockFactory
    {
        // Algorithm used to generate tetris blocks
        // Basically it picks all seven different blocks and gives it in a random order
        static bool[] done = new bool[7];
        public static Block generateBlock()
        {
            bool blockLeft = false;
            // Check if there is any block left to give
            for (int i = 0; i < done.Length; i++)
                if (done[i] == false)
                    blockLeft = true;

            // If there is no block left, reset array
            if (!blockLeft)
                for (int i = 0; i < done.Length; i++)
                    done[i] = false;

            int value;
            do {
                value = new Random().Next(7);
            } while (done[value]);

            done[value] = true;
            switch (value)
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

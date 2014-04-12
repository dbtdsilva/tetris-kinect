using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.TetrisModule
{
    class HighScore
    {
        private Score[] array;
        private int size;

        public HighScore(int tablesize)
        {
            size = 0;
            array = new Score[tablesize];
        }
        /* Add a new score to the highscore table */
        public bool addScore(int score, String name) {
            if (!checkScore(score))
                return false;

            int pos;
            for (pos = 0; pos < size; pos++)
            {
                if (array[pos].getPoints() < score)
                    break;
            }

            Score[] temp = array;
            System.Array.Copy(temp, pos, array, pos + 1, size - pos);
            array[pos] = new Score(name, score);

            size++;
            return true;
        }
        /* Checks if user beat any value on table or not */
        public bool checkScore(int score)
        {
            if (size != array.Length)
                return true;
            return (array[size - 1].getPoints() < score);
        }
        /* Return array of scores */
        public Score[] getHightscores()
        {
            return array;
        }
    }
}

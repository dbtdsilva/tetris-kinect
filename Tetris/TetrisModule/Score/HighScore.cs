using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.TetrisModule
{
    [Serializable()]
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
        public bool addScore(int score, string name) {
            if (!checkScore(score))
                return false;

            int pos;
            for (pos = 0; pos < size; pos++)
            {
                if (array[pos].Points < score)
                    break;
            }
            
            Score[] temp = array;
            if (size != array.Length)
            {
                System.Array.Copy(temp, pos, array, pos + 1, size - pos);
                size++;
            }
            else
            {
                System.Array.Copy(temp, pos, array, pos + 1, size - pos - 1);
            }
            array[pos] = new Score(name, score);
            return true;
        }
        /* Checks if user beat any value on table or not */
        public bool checkScore(int score)
        {
            if (size != array.Length)
                return true;
            return (array[size - 1].Points < score);
        }
        /* Return array of scores */
        public Score[] getArray()
        {
            return array;
        }
        public int Size
        {
            get
            {
                return size;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.TetrisModule
{
    [Serializable()]
    class Score
    {
        private int points;
        private string name;

        public Score(string name, int points)
        {
            this.name = name;
            this.points = points;
        }
        public string Name
        {
            get {
                return name;
            }
        }
        public int Points
        {
            get
            {
                return points;
            }
        }
        public override string ToString()
        {
            return "Name: " + name + "\tScore: " + points;
        }
    }
}

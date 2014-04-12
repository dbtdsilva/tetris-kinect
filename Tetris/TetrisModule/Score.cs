using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tetris.TetrisModule
{
    class Score
    {
        private int points;
        private String name;

        public Score(String name, int points)
        {
            this.name = name;
            this.points = points;
        }
        public int getPoints() {
            return points;
        }
        public String getName()
        {
            return name;
        }
        public override string ToString()
        {
            return "Name: " + name + "\tScore: " + points;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominos.MVVM.Model
{
    public class Game
    {
        public byte Round;

        public bool Won;

        public Game()
        {
            Round = 1;
            Won = false;
        }

        public void Next()
        {
            Round++;
        }
        
    }
}

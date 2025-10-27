using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominos.MVVM.ViewModel;

namespace Dominos.MVVM.Model
{
    public class Player
    {
        public List<Piece> Hand { get; set; }
        public int Score { get; set; }
        public int Total => Hand.Sum(p => p.Value);
        public Player()
        {
            Score = 0;
            Hand = new List<Piece>();
        }
    }
}

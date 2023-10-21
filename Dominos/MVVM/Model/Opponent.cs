using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Dominos.MVVM.ViewModel;

namespace Dominos.MVVM.Model
{
    public class Opponent
    {
        public List<BoneViewModel> Bones;

        public int Score;

        public Opponent(List<BoneViewModel> bones)
        {
            Bones = bones;
            Score = 0;
        }

        public Opponent()
        {
            Score = 0;
        }
        
        public int Total()
        {
            int total = 0;

            for (int i = 0; i < Bones.Count; i++)
            {
                total += Bones[i].Bone.Value();
            }

            return total;
        }

    }
}

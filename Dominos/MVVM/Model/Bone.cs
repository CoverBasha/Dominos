namespace Dominos.MVVM.Model
{
    public class Bone
    {
        public byte Left;
        public byte Right;

        public Bone(byte left, byte right)
        {
            Left = left;
            Right = right;
        }

        public Bone()
        {
            
        }

        public int Value()
        {
            return Left + Right;
        }
    }
}

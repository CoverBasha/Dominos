namespace Dominos.MVVM.Model
{
    public class Piece
    {
        public Piece(byte left, byte right)
        {
            Left = left;
            Right = right;
        }
        public Piece() { }
        public byte Left { get; }
        public byte Right { get; }
        public int Value => Left + Right;
        public bool IsDouble => Left == Right;
    }
}

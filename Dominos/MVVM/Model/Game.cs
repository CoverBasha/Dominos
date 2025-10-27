using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominos.MVVM.Model
{
    public class Game
    {
        public int Round { get; private set; }
        public Player Winner { get; private set; }
        public Player Player { get; }
        public Player Opponent { get; }
        public LinkedList<Piece> Board { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public Player Starter { get; private set; }
        public int GraveyardCount => graveyard.Count;
        public byte? LeftEnd { get; private set; }
        public byte? RightEnd { get; private set; }
        public Piece LastPlayed {  get; private set; }

        private readonly Random rng;
        private List<Piece> graveyard;

        public Game()
        {
            rng = new Random();
            Player = new Player();
            Opponent = new Player();
            Round = 1;
            Winner = null;
            Reset();
            Starter = DetermineStarter();
            CurrentPlayer = Starter;
        }

        public void Next()
        {
            Round++;
            Reset();
            CurrentPlayer = Starter;
            Winner = null;
        }

        public void EndRound()
        {
            if (Winner != null)
            {
                Winner.Score += AddScore(Winner);
                Starter = Winner;
            }
            else
            {
                Starter = Starter == Player ? Opponent : Player;
            }

        }

        private void Reset()
        {
            Board = new LinkedList<Piece>();
            LeftEnd = null;
            RightEnd = null;
            InitializeHands();
        }

        private void InitializeHands()
        {
            graveyard = new List<Piece>();
            for (byte i = 0; i < 7; i++)
                for (byte j = i; j < 7; j++)
                    graveyard.Add(new Piece(i, j));

            Player.Hand = DrawHand();
            Opponent.Hand = DrawHand();
        }

        private List<Piece> DrawHand()
        {
            var hand = new List<Piece>();
            for (int i = 0; i < 7; i++)
            {
                int x = rng.Next(graveyard.Count);
                hand.Add(graveyard[x]);
                graveyard.RemoveAt(x);
            }
            return hand;
        }

        private Player DetermineStarter()
        {
            int plrHighest = Player.Hand.Where(p => p.Left == p.Right).DefaultIfEmpty(new Piece(0, 0)).Max(p => p.Value);
            int oppHighest = Opponent.Hand.Where(p => p.Left == p.Right).DefaultIfEmpty(new Piece(0, 0)).Max(p => p.Value);

            if (plrHighest == oppHighest)
                return Player.Hand.Max(p => p.Value) >= Opponent.Hand.Max(p => p.Value) ? Player : Opponent;

            return plrHighest > oppHighest ? Player : Opponent;
        }

        public bool CanPlay(Piece piece) =>
            LeftEnd == null ||
            piece.Left == LeftEnd || piece.Right == LeftEnd ||
            piece.Left == RightEnd || piece.Right == RightEnd;
        public bool CanPlay(IEnumerable<Piece> hand) => hand.Any(CanPlay);
        public bool CanPlayLeft(Piece piece) => piece.Left == LeftEnd || piece.Right == LeftEnd;
        public bool CanPlayRight(Piece piece) => piece.Left == RightEnd || piece.Right == RightEnd;
        public Piece PlayableLeft() => Opponent.Hand.Find(CanPlayLeft);
        public Piece PlayableRight() => Opponent.Hand.Find(CanPlayRight);
        public MoveResult PlayPiece(Player player, Piece piece, Side side)
        {
            if (player != CurrentPlayer) return MoveResult.Invalid;
            if (!CanPlay(piece)) return MoveResult.Invalid;
            if (!player.Hand.Contains(piece)) return MoveResult.Invalid;

            if (LeftEnd == null)
            {
                Board.AddFirst(piece);
                LeftEnd = piece.Left;
                RightEnd = piece.Right;
            }
            else if (side == Side.Left)
            {
                Board.AddFirst(piece);
                LeftEnd = AdjustPiece(piece, LeftEnd.Value);
            }
            else
            {
                Board.AddLast(piece);
                RightEnd = AdjustPiece(piece, RightEnd.Value);
            }

            player.Hand.Remove(piece);
            LastPlayed = piece;

            if (WinningMove(player))
                return MoveResult.Win;

            SwitchTurn();
            return MoveResult.Valid;
        }
        public MoveResult OpponentPlay()
        {
            while (true)
            {
                var playable = Opponent.Hand.FirstOrDefault(p => CanPlay(p));
                if (playable != null)
                {
                    var side = CanPlayLeft(playable) ? Side.Left : Side.Right;
                    var result = PlayPiece(Opponent, playable, side);

                    if (result == MoveResult.Win || result == MoveResult.Valid)
                        return result;
                }
                else if (CanDraw())
                {
                    DrawPiece(Opponent);
                    continue;
                }
                else
                {
                    return MoveResult.Invalid;
                }
            }
        }

        private byte AdjustPiece(Piece piece, byte match)
        {
            return piece.Left == match ? piece.Right : piece.Left;
        }

        private void SwitchTurn()
        {
            CurrentPlayer = CurrentPlayer == Player ? Opponent : Player;
        }

        public Piece DrawPiece(Player player)
        {
            if (player != CurrentPlayer) return null;
            if (CanPlay(player.Hand)) return null;

            int x = rng.Next(graveyard.Count);
            var drawn = graveyard[x];
            player.Hand.Add(drawn);
            graveyard.RemoveAt(x);
            return drawn;
        }

        public bool CanDraw() => graveyard.Count > 0 && !CanPlay(CurrentPlayer.Hand);

        private bool WinningMove(Player player)
        {
            if (player.Hand.Count == 0)
            {
                Winner = player;
                return true;
            }
            return false;
        }

        public bool IsBlocked() =>
            graveyard.Count == 0 &&
            !CanPlay(Player.Hand) &&
            !CanPlay(Opponent.Hand);

        public Player BlockDetermineWinner()
        {
            if (!IsBlocked()) return null;
            if (Player.Total < Opponent.Total) return Winner = Player;
            if (Opponent.Total < Player.Total) return Winner = Opponent;
            return Winner = null;
        }

        private int AddScore(Player winner)
        {
            if (winner == Opponent) return Player.Total;
            if (winner == Player) return Opponent.Total;
            return 0;
        }


    }

    public enum Side { Left, Right }

    public enum MoveResult
    {
        Invalid,
        Valid,
        Win
    }

}

using Dominos.Commands;
using Dominos.MVVM.Model;
using Dominos.MVVM.Services;
using Dominos.Services;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;

namespace Dominos.MVVM.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        #region Properties
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _prev;

        private Game game { get; set; }

        public ObservableCollection<PieceViewModel> PlayerHand { get; set; }
        public ObservableCollection<PieceViewModel> OpponentHand { get; set; }
        public ObservableCollection<PieceViewModel> Board { get; set; }


        public ICommand PlayPieceCommand { get; }
        public RelayCommand DrawPieceCommand { get; }
        public RelayCommand NextRoundCommand { get; private set; }
        public ICommand ExitCommand { get; }


        public int Round => game.Round;
        public int PlayerScore => game.Player.Score;
        public int OpponentScore => game.Opponent.Score;
        public int GraveyardCount => game.GraveyardCount;
        public bool IsPlayerTurn => game.CurrentPlayer == game.Player;
        public Player Winner => game.Winner;
        public bool IsEndScreenVisible => IsGameOver || IsRoundOver;
        public bool IsGameOver { get; set; }
        public bool IsRoundOver { get; set; }

        private string _endText;
        public string EndText
        {
            get => _endText;
            set
            {
                _endText = value;
                OnPropertyChanged(nameof(EndText));
            }
        }

        private string _roundButtonText;
        public string RoundButtonText
        {
            get => _roundButtonText;
            set
            {
                _roundButtonText = value;
                OnPropertyChanged(nameof(RoundButtonText));
            }
        }

        public PieceViewModel Selected
        {
            get => _selected;
            set
            {
                _selected = value == _selected ? null : value;
                OnPropertyChanged(nameof(Selected));
            }
        }
        

        private PieceViewModel LeftPlaceHolder;
        private PieceViewModel RightPlaceHolder;
        private PieceViewModel _selected;
        #endregion


        public GameViewModel(NavigationStore navigationStore,Func<ViewModelBase> prev)
        {
            _navigationStore = navigationStore;
            _prev = prev;


            game = new Game();
            Placement.Reset();


            ExitCommand = new NavigateCommand(navigationStore, prev);
            PlayPieceCommand = new RelayCommand<PieceViewModel>(OnPlayPiece, CanPlayPiece);
            DrawPieceCommand = new RelayCommand(OnDrawPiece, CanDraw);
            NextRoundCommand = new RelayCommand(OnNextRound);

            PlayerHand = new ObservableCollection<PieceViewModel>
                (game.Player.Hand.Select(p => new PieceViewModel(p) { IsPlayable = game.CanPlay(p), Angle = 90 }));
            OpponentHand = new ObservableCollection<PieceViewModel>
                (game.Opponent.Hand.Select(p => new PieceViewModel(null) { IsOpponent = true, Angle = 90 }));
            Board = new ObservableCollection<PieceViewModel>
                (game.Board.Select(p => new PieceViewModel(p) { IsBoard = true }));

            if (game.Starter == game.Opponent)
            {
                OpponentPlay();
            }

            PieceViewModel.SelectedChanged += OnSelectedChanged;
        }

        private void OnSelectedChanged(object sender, PieceViewModel e)
        {
            if (e.IsPlaceHolder)
                return;

            ClearPlaceHolders();
            if (Selected == null)
                Selected = e;
            else if (e == Selected)
            {
                Selected = null;
                return;
            }
            else
            {
                Selected.IsSelected = false;
                Selected = e;
            }

            var left = game.LeftEnd;
            var right = game.RightEnd;
            if(game.Board.Count==0)
            {
                LeftPlaceHolder = Placement.PlaceHolderFirst(Selected.Model, PlayPieceCommand);
                Board.Add(LeftPlaceHolder);
                return;
            }
            if (game.CanPlayLeft(e.Model))
            {
                LeftPlaceHolder = Placement.PlaceHolderLeft(Selected.Model, (Byte)left, PlayPieceCommand);
                Board.Add(LeftPlaceHolder);
            }
            if(game.CanPlayRight(e.Model))
            {
                RightPlaceHolder = Placement.PlaceHolderRight(Selected.Model, (Byte)right, PlayPieceCommand);
                Board.Add(RightPlaceHolder);
            }
        }

        private bool CanPlayPiece(PieceViewModel pvm) => pvm != null && game.CanPlay(pvm.Model);
        private void OnPlayPiece(PieceViewModel pvm)
        {
            
            if (pvm == null || game.CurrentPlayer != game.Player)
                return;
            var left = game.LeftEnd;
            var right = game.RightEnd;
            ClearPlaceHolders();
            var result = game.PlayPiece(game.CurrentPlayer, pvm.Model, pvm.Side);

            if (game.Board.Count == 1)
                Board.Add(Placement.PlaceFirst(pvm, false));
            else
            {
                if (pvm.Side == Side.Left)
                    Board.Add(Placement.PlaceLeft(pvm, (byte)left, false));
                else
                    Board.Add(Placement.PlaceRight(pvm, (byte)right, false));
            }
            Sync();

            if (result == MoveResult.Win)
            {
                EndRound(game.Player);
                return;
            }

            if (game.IsBlocked())
            {
                game.BlockDetermineWinner();
                EndRound(game.Winner);
                return;
            }

            OpponentPlay();

            if (game.Winner != null)
            {
                EndRound(game.Winner);
                return;
            }
        }
        private void EndRound(Player winner)
        {
            if (IsGameOver)
            {
                EndText = winner == game.Player ? "You win!" : "Opponent wins!";
                RoundButtonText = "New Game";
            }
            else
            {
                EndText = "Round Over!";
                RoundButtonText = "Next Round";
            }
            OnPropertyChanged(nameof(PlayerScore));
            OnPropertyChanged(nameof(OpponentScore));
        }
        private bool CanDraw() => game.CanDraw();
        private void OnDrawPiece()
        {
            game.DrawPiece(game.CurrentPlayer);
            Sync();
        }
        private void OnNextRound()
        {
            game.Next();
            Placement.Reset();
            IsRoundOver = false;
            Board.Clear();
            Sync();
        }
        private void Sync()
        {
            CheckWin();
            PlayerHand.Clear();
            foreach (var piece in game.Player.Hand)
            {
                var vm = new PieceViewModel(piece)
                {
                    IsPlayable = game.CanPlay(piece) && IsPlayerTurn && !IsRoundOver,
                    Angle = 90
                };
                PlayerHand.Add(vm);
            }

            OpponentHand.Clear();
            foreach (var piece in game.Opponent.Hand)
                OpponentHand.Add(new PieceViewModel(null)
                {
                    IsOpponent = true,
                    Angle = 90
                });

            OnPropertyChanged(nameof(Round));
            OnPropertyChanged(nameof(PlayerScore));
            OnPropertyChanged(nameof(OpponentScore));
            OnPropertyChanged(nameof(GraveyardCount));
            OnPropertyChanged(nameof(IsPlayerTurn));
            OnPropertyChanged(nameof(Winner));
            OnPropertyChanged(nameof(IsRoundOver));
            OnPropertyChanged(nameof(IsGameOver));
            OnPropertyChanged(nameof(IsEndScreenVisible));
            OnPropertyChanged(nameof(NextRoundCommand));
            DrawPieceCommand.RaiseCanExecuteChanged();
            
        }
        private void CheckWin()
        {
            if (game.Winner == null && !game.IsBlocked())
                return;
            game.EndRound();
            if (OpponentScore > 1 || PlayerScore > 1)
                EndGame();
            else
                IsRoundOver = true;
        }
        private void EndGame()
        {
            IsGameOver = true;
            IsRoundOver = true;
            NextRoundCommand = new RelayCommand(NewGame, IsGameOver);
            
            EndRound(Winner);
        }
        private void NewGame()
        {
            _navigationStore.CurrentViewModel = new GameViewModel(_navigationStore, _prev);
        }
        private void ClearPlaceHolders()
        {
            Board.Remove(LeftPlaceHolder);
            Board.Remove(RightPlaceHolder);
            LeftPlaceHolder = null;
            RightPlaceHolder = null;
        }
        private void OpponentPlay()
        {
            var left = game.LeftEnd;
            var right = game.RightEnd;
            var result = game.OpponentPlay();

            if (result == MoveResult.Valid)
            {
                var vm = new PieceViewModel(game.LastPlayed)
                {
                    Side = game.LastPlayed == game.Board.First.Value ? Side.Left : Side.Right
                };

                if (game.Board.Count == 1)
                    Board.Add(Placement.PlaceFirst(vm, false));
                else
                {
                    if (vm.Side == Side.Left)
                        Board.Add(Placement.PlaceLeft(vm, (byte)left, false));
                    else
                        Board.Add(Placement.PlaceRight(vm, (byte)right, false));
                }
            }
            Sync();
        }
    }

}

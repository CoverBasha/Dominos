using System;
using System.Windows.Input;
using Dominos.MVVM.Model;
using GalaSoft.MvvmLight.Command;

namespace Dominos.MVVM.ViewModel
{
    public class PieceViewModel : ViewModelBase
    {
        #region Properties
        public Piece Model { get; set; }
        public int? Left => Model?.Left;
        public int? Right => Model?.Right;
        public Side Side { get; set; }
        #endregion

        #region States
        private bool _isPlayable;
        public bool IsPlayable
        {
            get => _isPlayable || IsPlaceHolder;
            set
            {
                if (_isPlayable != value)
                {
                    _isPlayable = value;
                    OnPropertyChanged(nameof(IsPlayable));
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }
        public bool IsOpponent { get; set; }
        public bool IsBoard { get; set; }
        public bool ShouldDarken => !IsPlayable && !IsOpponent && !IsBoard;
        public bool IsDouble => Model.IsDouble;
        public bool IsPlaceHolder { get; set; } = false;
        #endregion

        #region Placement
        private int _x;
        public int X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged(nameof(X));
                }
            }
        }

        private int _y;
        public int Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }
        }

        private int _angle = 0;
        public int Angle
        {
            get => _angle;
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    OnPropertyChanged(nameof(Angle));
                }
            }
        }
        #endregion

        public ICommand SelectCommand { get; set; }
        public PieceViewModel(Piece piece)
        {
            Model = piece;
            SelectCommand = new RelayCommand(OnSelect, IsPlayable);
        }
        public PieceViewModel(Piece piece, ICommand command)
        {
            Model = piece;
            IsPlayable = true;
            IsPlaceHolder = true;
            SelectCommand = new RelayCommand<PieceViewModel>(p => command.Execute(this), true);
        }
        void OnSelect()
        {
            if (Model != null)
            {
                IsSelected = !IsSelected;
                SelectedChanged?.Invoke(this, this);
            }
        }

        public static event EventHandler<PieceViewModel> SelectedChanged;
    }
}

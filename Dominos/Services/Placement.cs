using Dominos.MVVM.Model;
using Dominos.MVVM.ViewModel;
using System.Windows.Input;

namespace Dominos.MVVM.Services
{
    public static class Placement
    {
        static float _leftLength = 0;
        static float leftLength
        {
            get => _leftLength;
            set
            {
                _leftLength = value;
                if (_leftLength > 3.5 && !turnup && !turnright)
                {
                    turnup = true;
                    justTurnedUp = true;
                    orientationLeft += 90;
                }
            }
        }

        static float _rightLength = 0;
        static float rightLength
        {
            get => _rightLength;
            set
            {
                _rightLength = value;
                if (_rightLength > 3.5 && !turndown && !turnleft)
                {
                    turndown = true;
                    justTurnedDown = true;
                    orientationRight += 90;
                }
            }
        }

        static float _topLength = 0;
        static float topLength
        {
            get => _topLength;
            set
            {
                _topLength = value;
                if (_topLength > 1.5 && turnup)
                {
                    turnup = false;
                    turnright = true;
                    justTurnedRight = true;
                    orientationLeft += 90;
                }
            }
        }

        static float _bottomLength = 0;
        static float bottomLength
        {
            get => _bottomLength;
            set
            {
                _bottomLength = value;
                if (_bottomLength > 1.5 && turndown)
                {
                    turndown = false;
                    justTurnedLeft = true;
                    turnleft = true;
                    orientationRight += 90;
                }
            }
        }

        static bool turnup = false;
        static bool turnright = false;
        static bool turndown = false;
        static bool turnleft = false;

        static int orientationLeft = 0;
        static int orientationRight = 0;

        const int QUARTER = 35;
        const int HALF = 70;
        const int FULL = 140;
        const int TURN_RIGHT = 90;
        const int TURN_OVER = 180;

        static int XLastLeft;
        static int XLastRight;
        static int YLastLeft;
        static int YLastRight;

        static bool lastRightIsDouble;
        static bool lastLeftIsDouble;

        static bool justTurnedUp;
        static bool justTurnedRight;
        static bool justTurnedDown;
        static bool justTurnedLeft;

        public static PieceViewModel PlaceHolderFirst(Piece piece, ICommand command)
        {
            var vm = new PieceViewModel(piece, command);
            PlaceFirst(vm, vm.IsPlaceHolder);
            return vm;
        }
        public static PieceViewModel PlaceFirst(PieceViewModel vm, bool placeholder)
        {
            vm.X = vm.IsDouble ? 650 + QUARTER : 650;
            vm.Y = vm.IsDouble ? 300 : 300 + QUARTER;
            vm.Angle = vm.IsDouble ? TURN_RIGHT : 0;

            if (!placeholder)
            {
                vm.IsPlaceHolder = false;
                vm.IsPlayable = false;
                vm.IsBoard = true;

                if (vm.IsDouble)
                {
                    rightLength += 0.25f;
                    leftLength += 0.25f;
                    YLastLeft = vm.Y + QUARTER;
                    YLastRight = vm.Y + QUARTER;
                    XLastRight = vm.X - HALF;
                }
                else
                {
                    leftLength += 0.5f;
                    rightLength += 0.5f;
                    YLastLeft = vm.Y;
                    YLastRight = vm.Y;
                    XLastRight = vm.X;
                }

                XLastLeft = vm.X;
            }

            return vm;
        }
        public static PieceViewModel PlaceHolderLeft(Piece piece, byte left, ICommand command)
        {
            var vm = new PieceViewModel(piece, command) { Side = Side.Left };
            PlaceLeft(vm, left, vm.IsPlaceHolder);
            return vm;
        }
        public static PieceViewModel PlaceHolderRight(Piece piece, byte right, ICommand command)
        {
            var vm = new PieceViewModel(piece, command) { Side = Side.Right };
            PlaceRight(vm, right, vm.IsPlaceHolder);
            return vm;
        }
        public static PieceViewModel PlaceLeft(PieceViewModel vm, int left, bool placeholder)
        {
            if (vm.IsDouble)
                vm.Angle = orientationLeft + TURN_RIGHT;
            else if (vm.Left == left)
                vm.Angle = orientationLeft + TURN_OVER;
            else
                vm.Angle = orientationLeft;

            if (turnright)
            {
                if (justTurnedRight)
                {
                    vm.X = XLastLeft + HALF;
                    vm.Y = vm.IsDouble ? YLastLeft - QUARTER : YLastLeft;

                    vm.X = lastLeftIsDouble ? XLastLeft + FULL : vm.X;
                    if (!placeholder)
                        justTurnedRight = false;
                }
                else
                {
                    vm.X = lastLeftIsDouble ? XLastLeft + HALF : XLastLeft + FULL;
                    vm.Y = vm.IsDouble ? YLastLeft - QUARTER : YLastLeft;
                }

                if (!placeholder)
                {
                    YLastLeft = vm.IsDouble ? vm.Y + QUARTER : vm.Y;
                    XLastLeft = vm.X;
                }
            }
            else if (turnup)
            {
                if (justTurnedUp)
                {
                    vm.X = vm.IsDouble ? XLastLeft - QUARTER : XLastLeft;
                    vm.Y = vm.IsDouble ? YLastLeft - HALF : YLastLeft - FULL;

                    vm.Y = lastLeftIsDouble ? YLastLeft - FULL - QUARTER : vm.Y;
                    if (!placeholder)
                        justTurnedUp = false;
                }
                else
                {
                    vm.X = vm.IsDouble ? XLastLeft - QUARTER : XLastLeft;
                    vm.Y = vm.IsDouble ? YLastLeft - HALF : YLastLeft - FULL;
                }

                if (!placeholder)
                {
                    topLength += vm.IsDouble ? 0.5f : 1f;
                    XLastLeft = vm.IsDouble ? vm.X + QUARTER : vm.X;
                    YLastLeft = vm.Y;
                }
            }
            else
            {
                vm.X = vm.IsDouble ? XLastLeft - HALF : XLastLeft - FULL;
                vm.Y = vm.IsDouble ? YLastLeft - QUARTER : YLastLeft;

                if (!placeholder)
                {
                    leftLength += vm.IsDouble ? 0.5f : 1f;
                    XLastLeft = vm.X;
                    YLastLeft = vm.IsDouble ? vm.Y + QUARTER : vm.Y;
                }
            }

            if (!placeholder)
            {
                vm.IsPlaceHolder = false;
                vm.IsPlayable = false;
                vm.IsBoard = true;
                lastLeftIsDouble = vm.IsDouble;
            }

            return vm;
        }
        public static PieceViewModel PlaceRight(PieceViewModel vm, int right, bool placeholder)
        {
            if (vm.IsDouble)
                vm.Angle = orientationRight + TURN_RIGHT;
            else if (vm.Right == right)
                vm.Angle = orientationRight + TURN_OVER;
            else
                vm.Angle = orientationRight;

            if (turnleft)
            {
                if (justTurnedLeft)
                {
                    vm.X = vm.IsDouble ? XLastRight - HALF : XLastRight - FULL;
                    vm.Y = vm.IsDouble ? YLastRight + QUARTER : YLastRight + HALF;

                    vm.Y = lastRightIsDouble ? YLastRight : vm.Y;
                    vm.X = lastRightIsDouble ? vm.X - QUARTER : vm.X;
                    if (!placeholder)
                        justTurnedLeft = false;
                }
                else
                {
                    vm.X = vm.IsDouble ? XLastRight - HALF : XLastRight - FULL;
                    vm.Y = vm.IsDouble ? YLastRight - QUARTER : YLastRight;
                }

                if (!placeholder)
                {
                    YLastRight = vm.IsDouble ? vm.Y + QUARTER : vm.Y;
                    XLastRight = vm.X;
                }
            }
            else if (turndown)
            {
                if (justTurnedDown)
                {
                    vm.X = vm.IsDouble ? XLastRight + QUARTER : XLastRight + FULL;
                    vm.Y = vm.IsDouble ? YLastRight + HALF : YLastRight;

                    vm.Y = lastRightIsDouble ? YLastRight + QUARTER : vm.Y;
                    vm.X = lastRightIsDouble ? XLastRight + HALF : vm.X;
                    if (!placeholder)
                        justTurnedDown = false;
                }
                else
                {
                    vm.X = vm.IsDouble ? XLastRight - QUARTER : XLastRight;
                    vm.Y = lastRightIsDouble ? YLastRight + HALF : YLastRight + FULL;
                }

                if (!placeholder)
                {

                    bottomLength += vm.IsDouble ? 0.5f : 1f;
                    XLastRight = vm.IsDouble ? vm.X + QUARTER : vm.X;
                    
                    YLastRight = vm.Y;
                }
            }
            else
            {
                vm.X = lastRightIsDouble ? XLastRight + HALF : XLastRight + FULL;
                vm.Y = vm.IsDouble ? YLastRight - QUARTER : YLastRight;

                if (!placeholder)
                {
                    rightLength += vm.IsDouble ? 0.5f : 1f;
                    XLastRight = vm.X;
                    YLastRight = vm.IsDouble ? vm.Y + QUARTER : vm.Y;

                }
            }

            if (!placeholder)
            {
                vm.IsPlaceHolder = false;
                vm.IsPlayable = false;
                vm.IsBoard = true;
                lastRightIsDouble = vm.IsDouble;
            }

            return vm;
        }
        public static void Reset()
        {
            _bottomLength = 0;
            _topLength = 0;
            _leftLength = 0;
            _rightLength = 0;
            turndown = false;
            turnup = false;
            turnright = false;
            turnleft = false;
            orientationLeft = 0;
            orientationRight = 0;
        }
    }
}

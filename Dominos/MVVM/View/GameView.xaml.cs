using Dominos.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;

namespace Dominos.MVVM.View
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class GameView : UserControl
    {

        private BoneViewModel Selected;
        private BoneViewModel OppBone;
        private BoneView LeftHolder = new BoneView();
        private BoneView RightHolder = new BoneView();
        private float lengthRight = 0, lengthLeft = 0, lengthTop = 0, lengthBottom = 0;
        private int orientationLeft = 0, orientationRight = 0;
        private BoneViewModel LeftViewModel, RightViewModel;
        private int right, left;
        private bool turnDown = false, turnUp = false, turnRight = false, turnLeft = false;
        private int plrHighest = 0, oppHighest = 0;
        private DispatcherTimer timer;
        private DispatcherTimer roundTimer;


        public GameView()
        {
            InitializeComponent();
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.5)
            };
            timer.Tick += Timer_Tick;

            roundTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            roundTimer.Tick += RoundTimer_Tick;

            GameLayout();
        }

        private void InitializeValues()
        {
            lengthBottom = 0;
            lengthLeft = 0;
            lengthRight = 0;
            lengthTop = 0;
            orientationLeft = 0;
            orientationRight = 0;
            turnDown = false;
            turnLeft = false;
            turnRight = false;
            turnUp = false;
            plrHighest = 0;
            oppHighest = 0;
            
        }

        private void GameLayout()
        {
            InitializeValues();
            List<BoneViewModel> PlrBones = new List<BoneViewModel>();
            List<BoneViewModel> OppBones = new List<BoneViewModel>();
            GameViewModel.GraveYard.Clear();
            GameViewModel.Ground.Clear();
            Board.Children.Clear();

            for (byte i = 0; i < 7; i++)
            {
                for (byte j = i; j < 7; j++)
                {
                    GameViewModel.GraveYard.Add(new BoneViewModel(i, j));
                }
            }

            Random random = new Random();

            for (int i = 0; i < 14; i++)
            {
                int x = random.Next(GameViewModel.GraveYard.Count);
                if (i < 7)
                {
                    GameViewModel.GraveYard[x].BoneView.Clicked += DominoSelect;
                    PlrBones.Add(GameViewModel.GraveYard[x]);
                }
                else
                {
                    GameViewModel.GraveYard[x].BoneView.Button.IsEnabled = false;
                    OppBones.Add(GameViewModel.GraveYard[x]);
                }

                GameViewModel.GraveYard.RemoveAt(x);
            }

            GameViewModel.Player.Bones = PlrBones;
            GameViewModel.Opponent.Bones = OppBones;

            UpdateHand(PlrBoard, GameViewModel.Player.Bones, false);
            UpdateHand(OppBoard, GameViewModel.Opponent.Bones, true);


            foreach (var item in GameViewModel.Opponent.Bones)
            {
                if (item.Bone.Value() > oppHighest)
                    oppHighest = item.Bone.Value();
            }

            if (GameViewModel.Game.Round == 1)
            {
                foreach (var item in GameViewModel.Player.Bones)
                {
                    if (item.Bone.Value() > plrHighest)
                        plrHighest = item.Bone.Value();
                }

                YourScore.Text = 0.ToString();
                OpponentScore.Text = 0.ToString();

                if (plrHighest < oppHighest)
                {
                    StartTimer();
                    Blocker.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (!GameViewModel.Game.Won)
                {
                    StartTimer();
                    Blocker.Visibility = Visibility.Visible;
                }
                else
                {
                    Blocker.Visibility = Visibility.Collapsed;
                    timer.Stop();
                }
            }


        }

        private void OpponentPlay()
        {
            if (GameViewModel.Ground.Count == 0)
            {
                foreach (var bone in GameViewModel.Opponent.Bones)
                {
                    if (bone.Bone.Value() == oppHighest)
                    {
                        OppBone = bone;
                        break;
                    }
                }
                if (OppBone.Bone.Right == OppBone.Bone.Left)
                    Rotate(OppBone.BoneView, 90);
                Board.Children.Add(OppBone.BoneView);
                Canvas.SetLeft(OppBone.BoneView, 620);
                Canvas.SetTop(OppBone.BoneView, 260);
                GameViewModel.Ground.Add(OppBone);
                GameViewModel.Opponent.Bones.Remove(OppBone);
                UpdateHand(OppBoard, GameViewModel.Opponent.Bones, true);

                LeftViewModel = OppBone;
                left = LeftViewModel.Bone.Left;
                RightViewModel = OppBone;
                right = RightViewModel.Bone.Right;
                if (left == right)
                {
                    lengthLeft += (float)0.25;
                    lengthRight += (float)0.25;
                }
                else
                {
                    lengthLeft += (float)0.5;
                    lengthRight += (float)0.5;
                }
            }
            else
            {
                bool hasCards = false;

                foreach (var bone in GameViewModel.Opponent.Bones)
                {
                    if (bone.Bone.Right == right || bone.Bone.Right == left || bone.Bone.Left == right || bone.Bone.Left == left)
                    {
                        hasCards = true;
                        OppBone = bone;
                        break;
                    }
                }

                for (int i = 0; !hasCards && GameViewModel.GraveYard.Count != 0; i++)
                {
                    Random random = new Random();
                    int x = random.Next(GameViewModel.GraveYard.Count);

                    OppBone = GameViewModel.GraveYard[x];
                    GameViewModel.Opponent.Bones.Add(OppBone);
                    GameViewModel.GraveYard.Remove(OppBone);
                    CardsCount.Text = GameViewModel.GraveYard.Count.ToString() + " x";
                    if (OppBone.Bone.Right == right || OppBone.Bone.Right == left || OppBone.Bone.Left == right || OppBone.Bone.Left == left)
                    {
                        hasCards = true;
                        i++;
                        DrawText.Text = "Opponent drew " + i + " cards";
                        DrawText.Visibility = Visibility.Visible;
                        UpdateHand(OppBoard, GameViewModel.Opponent.Bones, true);
                    }
                }

                if (hasCards)
                    OppPlace();
                else
                    DrawText.Text = "Opponent skipped";
                
            }
        }

        private void UpdateHand(Canvas panel, List<BoneViewModel> list, bool opp)
        {
            panel.Children.Clear();
            int start = 660 - 45 * list.Count;
            int next = start;
            if (opp)
            {
                foreach (var bone in list)
                {
                    var plain = new BoneView();
                    plain.Button.IsEnabled = false;
                    Rotate(plain, 90);
                    panel.Children.Add(plain);
                    Canvas.SetLeft(plain, next);
                    Canvas.SetBottom(plain, 120);
                    next += 90;
                }
            }
            else
            {
                foreach (var bone in list)
                {
                    Rotate(bone.BoneView, 90);
                    panel.Children.Add(bone.BoneView);
                    Canvas.SetLeft(bone.BoneView, next);
                    Canvas.SetTop(bone.BoneView, 0);
                    next += 90;
                }
            }
        }

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var bone in GameViewModel.Player.Bones)
            {
                if (bone.Bone.Right == right || bone.Bone.Right == left || bone.Bone.Left == right || bone.Bone.Left == left)
                {
                    return;
                }
            }
            Draw();
        }

        private void Rotate(BoneView control, int angle)
        {
            RotateTransform rotate = new RotateTransform();
            rotate.CenterX = 165 / 2;
            rotate.CenterY = 80 / 2;
            rotate.Angle = angle;
            control.angle = (int)rotate.Angle;
            control.RenderTransform = rotate;
        }

        private void DominoSelect(object sender, RoutedEventArgs e)
        {
            var senderbone = (BoneView)sender;

            if (Selected != null)
            {
                if (senderbone == Selected.BoneView)
                {
                    Deselect(ref Selected, false);
                    Board.Children.Remove(LeftHolder);
                    Board.Children.Remove(RightHolder);
                    return;
                }
                else
                {
                    Deselect(ref Selected, false);
                    Board.Children.Remove(LeftHolder);
                    Board.Children.Remove(RightHolder);
                }
            }

            foreach (var bone in GameViewModel.Player.Bones)
            {
                if (bone.BoneView == senderbone)
                {
                    Selected = bone;
                    Selected.BoneView.Selected = true;
                    Canvas.SetTop(Selected.BoneView, Canvas.GetTop(Selected.BoneView) - 20);
                    if (GameViewModel.Ground.Count == 0)
                    {
                        Placeholder();
                    }
                    else
                    {
                        PlaceRight();
                        PlaceLeft();
                    }

                    return;
                }
            }
        }

        private void Deselect(ref BoneViewModel bone, bool Play)
        {
            if (bone.BoneView.Selected)
            {
                if (!Play)
                    Canvas.SetTop(bone.BoneView, Canvas.GetTop(bone.BoneView) + 20);
                bone.BoneView.Selected = false;
                bone = null;
            }
        }

        private void PlayLeft(object sender, RoutedEventArgs e)
        {
            if (GameViewModel.Ground.Count == 0)
            {
                RightViewModel = Selected;
                LeftViewModel = Selected;
                right = RightViewModel.Bone.Right;
                left = LeftViewModel.Bone.Left;
                if (left == right)
                {
                    lengthLeft += (float)0.25;
                    lengthRight += (float)0.25;
                }
                else
                {
                    lengthLeft += (float)0.5;
                    lengthRight += (float)0.5;
                }
            }
            else
            {
                LeftViewModel = Selected;

                if (left == Selected.Bone.Right)
                    left = LeftViewModel.Bone.Left;
                else
                    left = LeftViewModel.Bone.Right;

                if (orientationLeft < 90)
                {
                    if (LeftViewModel.Bone.Right == LeftViewModel.Bone.Left)
                        lengthLeft += (float)0.5;
                    else
                        lengthLeft += 1;
                }
                else if (orientationLeft < 180)
                {
                    if (LeftViewModel.Bone.Right == LeftViewModel.Bone.Left || turnUp)
                        lengthTop += (float)0.5;
                    else
                        lengthTop += 1;
                }
                else
                {
                    if (LeftViewModel.Bone.Right == LeftViewModel.Bone.Left || turnRight)
                        lengthLeft += (float)0.5;
                    else
                        lengthLeft += 1;
                }
            }

            if (turnUp)
            {
                turnUp = false;
            }
            else if (turnRight)
            {
                turnRight = false;
            }

            GameViewModel.Ground.Add(Selected);
            PlayCard(Selected);


            Selected.BoneView.Button.IsEnabled = false;
            Selected.BoneView.RenderTransform = LeftHolder.RenderTransform;

            Canvas.SetLeft(Selected.BoneView, Canvas.GetLeft(LeftHolder));
            Canvas.SetTop(Selected.BoneView, Canvas.GetTop(LeftHolder));


            Board.Children.Remove(LeftHolder);
            Board.Children.Remove(RightHolder);
            Deselect(ref Selected, true);
        }

        private void PlayRight(object sender, RoutedEventArgs e)
        {
            RightViewModel = Selected;

            if (right == Selected.Bone.Left)
                right = RightViewModel.Bone.Right;
            else
                right = RightViewModel.Bone.Left;

            if (orientationRight < 90)
            {
                if (RightViewModel.Bone.Right == RightViewModel.Bone.Left)
                    lengthRight += (float)0.5;
                else
                    lengthRight += 1;
            }
            else if (orientationLeft < 180)
            {
                if (RightViewModel.Bone.Right == RightViewModel.Bone.Left || turnDown)
                    lengthBottom += (float)0.5;
                else
                    lengthBottom += 1;
            }
            else
            {
                if (RightViewModel.Bone.Right == RightViewModel.Bone.Left || turnLeft)
                    lengthRight += (float)0.5;
                else
                    lengthRight += 1;
            }

            if (turnDown)
            {
                turnDown = false;
            }
            else if (turnLeft)
            {
                turnLeft = false;
            }

            PlayCard(Selected);

            Selected.BoneView.Button.IsEnabled = false;
            Selected.BoneView.RenderTransform = RightHolder.RenderTransform;

            Canvas.SetLeft(Selected.BoneView, Canvas.GetLeft(RightHolder));
            Canvas.SetTop(Selected.BoneView, Canvas.GetTop(RightHolder));

            Board.Children.Remove(LeftHolder);
            Board.Children.Remove(RightHolder);
            Deselect(ref Selected, true);
        }

        private void Placeholder()
        {
            LeftHolder = new BoneView(Selected);
            LeftHolder.Clicked += PlayLeft;
            if (Selected.Bone.Right == Selected.Bone.Left)
                Rotate(LeftHolder, 90);
            Board.Children.Add(LeftHolder);
            LeftHolder.Opacity = 0.5;
            Canvas.SetLeft(LeftHolder, 620);
            Canvas.SetTop(LeftHolder, 260);
        }

        private void PlaceRight()
        {
            if (lengthRight > 3.15 && orientationRight == 0)
            {
                orientationRight += 90;
                turnDown = true;
            }
            if (lengthBottom >= 1 && orientationRight == 90)
            {
                orientationRight += 90;
                turnLeft = true;
            }

            if (right == Selected.Bone.Right)
            {
                if (orientationRight == 0)
                {


                    RightHolder = new BoneView(Selected);
                    RightHolder.Clicked += PlayRight;
                    Board.Children.Add(RightHolder);
                    RightHolder.Opacity = 0.5;

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(RightHolder, 90);
                        Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 123);
                    }
                    else
                    {
                        Rotate(RightHolder, 180);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 123);
                        }
                        else
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 166);
                    }
                    Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView));

                    return;
                }
                else if (orientationRight == 90)
                {
                    RightHolder = new BoneView(Selected);
                    RightHolder.Clicked += PlayRight;
                    Board.Children.Add(RightHolder);
                    RightHolder.Opacity = 0.5;

                    if (turnDown)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(RightHolder, 0);
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 82);
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 42);
                        }
                        else
                        {
                            Rotate(RightHolder, 270);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(RightHolder, 0);
                        Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 124);
                    }
                    else
                    {
                        Rotate(RightHolder, 270);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 124);
                        else
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 166);
                    }
                    Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView));
                    return;
                }
                else
                {
                    RightHolder = new BoneView(Selected);
                    RightHolder.Clicked += PlayRight;
                    Board.Children.Add(RightHolder);
                    RightHolder.Opacity = 0.5;

                    if (turnLeft)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(RightHolder, 90);
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 42);
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 82);
                        }
                        else
                        {
                            Rotate(RightHolder, 0);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 82);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(RightHolder, 90);
                        Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                    }
                    else
                    {
                        Rotate(RightHolder, 0);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                        else
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 166);
                    }
                    Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView));
                    return;
                }

            }

            if (right == Selected.Bone.Left)
            {
                if (orientationRight == 0)
                {
                    RightHolder = new BoneView(Selected);
                    RightHolder.Clicked += PlayRight;
                    Board.Children.Add(RightHolder);
                    RightHolder.Opacity = 0.5;

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(RightHolder, 90);
                        Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                    }
                    else
                    {
                        Rotate(RightHolder, 0);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                        }
                        else
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 166);
                    }
                    Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView));

                    return;
                }
                else if (orientationRight == 90)
                {
                    RightHolder = new BoneView(Selected);
                    RightHolder.Clicked += PlayRight;
                    Board.Children.Add(RightHolder);
                    RightHolder.Opacity = 0.5;

                    if (turnDown)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(RightHolder, 0);
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 82);
                        }
                        else
                        {
                            Rotate(RightHolder, 90);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 82);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(RightHolder, 0);
                        Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 124);
                    }
                    else
                    {
                        Rotate(RightHolder, 90);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 124);
                        else
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 166);
                        Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView));

                    }
                    return;
                }
                else
                {
                    RightHolder = new BoneView(Selected);
                    RightHolder.Clicked += PlayRight;
                    Board.Children.Add(RightHolder);
                    RightHolder.Opacity = 0.5;

                    if (turnLeft)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(RightHolder, 90);
                            Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 82);
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 42);
                        }
                        else
                        {
                            Rotate(RightHolder, 180);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 82);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(RightHolder, 90);
                        Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 123);
                    }
                    else
                    {
                        Rotate(RightHolder, 180);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 123);
                        else
                            Canvas.SetLeft(RightHolder, Canvas.GetLeft(RightViewModel.BoneView) - 166);
                    }
                    Canvas.SetTop(RightHolder, Canvas.GetTop(RightViewModel.BoneView));
                    return;
                }
            }
        }

        private void PlaceLeft()
        {
            if (lengthLeft > 3.15 && orientationLeft == 0)
            {
                orientationLeft += 90;
                turnUp = true;
            }
            if (lengthTop >= 1 && orientationLeft == 90)
            {
                orientationLeft += 180;
                turnRight = true;
            }

            if (left == Selected.Bone.Right)
            {
                if (orientationLeft == 0)
                {
                    LeftHolder = new BoneView(Selected);
                    LeftHolder.Clicked += PlayLeft;
                    Board.Children.Add(LeftHolder);
                    LeftHolder.Opacity = 0.5;

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(LeftHolder, 90);
                        Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                    }
                    else
                    {
                        Rotate(LeftHolder, 0);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                        }
                        else
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 166);
                    }
                    Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView));

                    return;
                }
                else if (orientationLeft == 90)
                {
                    LeftHolder = new BoneView(Selected);
                    LeftHolder.Clicked += PlayLeft;
                    Board.Children.Add(LeftHolder);
                    LeftHolder.Opacity = 0.5;

                    if (turnUp)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(LeftHolder, 0);
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 82);
                        }
                        else
                        {
                            Rotate(LeftHolder, 90);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 42);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(LeftHolder, 0);
                        Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 123);
                        Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView));
                    }
                    else
                    {
                        Rotate(LeftHolder, 90);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 123);
                        else
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 166);

                    }
                    Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView));
                    return;
                }
                else
                {
                    LeftHolder = new BoneView(Selected);
                    LeftHolder.Clicked += PlayLeft;
                    Board.Children.Add(LeftHolder);
                    LeftHolder.Opacity = 0.5;

                    if (turnRight)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(LeftHolder, 270);
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                        }
                        else
                        {
                            Rotate(LeftHolder, 180);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(LeftHolder, 270);
                        Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                    }
                    else
                    {
                        Rotate(LeftHolder, 180);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                        else
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 166);
                    }
                    Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView));
                    return;
                }
            }

            if (left == Selected.Bone.Left)
            {
                if (orientationLeft == 0)
                {
                    LeftHolder = new BoneView(Selected);
                    LeftHolder.Clicked += PlayLeft;
                    Board.Children.Add(LeftHolder);
                    LeftHolder.Opacity = 0.5;

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(LeftHolder, 90);
                        Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                    }
                    else
                    {
                        Rotate(LeftHolder, 180);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                        }
                        else
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 166);
                    }
                    Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView));

                    return;
                }
                else if (orientationLeft == 90)
                {
                    LeftHolder = new BoneView(Selected);
                    LeftHolder.Clicked += PlayLeft;
                    Board.Children.Add(LeftHolder);
                    LeftHolder.Opacity = 0.5;

                    if (turnUp)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(LeftHolder, 0);
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 42);
                        }
                        else
                        {
                            Rotate(LeftHolder, 270);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(LeftHolder, 0);
                        Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 123);
                    }
                    else
                    {
                        Rotate(LeftHolder, 270);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 123);
                        else
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 166);
                    }

                    Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView));
                    return;
                }
                else
                {
                    LeftHolder = new BoneView(Selected);
                    LeftHolder.Clicked += PlayLeft;
                    Board.Children.Add(LeftHolder);
                    LeftHolder.Opacity = 0.5;

                    if (turnRight)
                    {
                        if (Selected.Bone.Left == Selected.Bone.Right)
                        {
                            Rotate(LeftHolder, 90);
                            Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 42);
                        }
                        else
                        {
                            Rotate(LeftHolder, 0);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                                Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 42);
                            }
                        }
                        return;
                    }

                    if (Selected.Bone.Left == Selected.Bone.Right)
                    {
                        Rotate(LeftHolder, 90);
                        Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 166);
                    }
                    else
                    {
                        Rotate(LeftHolder, 0);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                        else
                            Canvas.SetLeft(LeftHolder, Canvas.GetLeft(LeftViewModel.BoneView) + 166);
                    }
                    Canvas.SetTop(LeftHolder, Canvas.GetTop(LeftViewModel.BoneView));
                    return;
                }
            }
        }

        private void PlayCard(BoneViewModel boneViewModel)
        {
            GameViewModel.Player.Bones.Remove(boneViewModel);
            UpdateHand(PlrBoard, GameViewModel.Player.Bones, false);
            Board.Children.Add(boneViewModel.BoneView);
            GameViewModel.Ground.Add(boneViewModel);
            DrawText.Visibility = Visibility.Collapsed;
            Blocker.Visibility = Visibility.Visible;
            DrawText.Visibility = Visibility.Collapsed;

            if (GameViewModel.Player.Bones.Count == 0)
            {
                GameViewModel.Player.Score += GameViewModel.Opponent.Total();
                YourScore.Text = GameViewModel.Player.Score.ToString();
                GameViewModel.Game.Next();
                GameViewModel.Game.Won = true;
                GameLayout();
                RoundText.Text = "Round " + GameViewModel.Game.Round;
                WinnerText.Text = "You win!";
                WinnerText.Foreground = Brushes.Green;
                RoundScreen.Visibility = Visibility.Visible;
                roundTimer.Start();
            }
            else if (left == right)
            {
                int count = 0;
                foreach (var bone in GameViewModel.Ground)
                {
                    if (bone.Bone.Left == left || bone.Bone.Right == left)
                        count++;
                }
                if (count == 7)
                {
                    if (GameViewModel.Player.Total() > GameViewModel.Opponent.Total())
                    {
                        GameViewModel.Player.Score += GameViewModel.Opponent.Total();
                        YourScore.Text = GameViewModel.Player.Score.ToString();
                        GameViewModel.Game.Next();
                        GameViewModel.Game.Won = true;
                        GameLayout();
                        RoundText.Text = "Round " + GameViewModel.Game.Round;
                        WinnerText.Text = "You win!";
                        WinnerText.Foreground = Brushes.Green;
                        RoundScreen.Visibility = Visibility.Visible;
                        roundTimer.Start();
                    }
                    else if (GameViewModel.Player.Total() < GameViewModel.Opponent.Total())
                    {
                        GameViewModel.Opponent.Score += GameViewModel.Player.Total();
                        OpponentScore.Text = GameViewModel.Opponent.Score.ToString();
                        GameViewModel.Game.Next();
                        GameViewModel.Game.Won = false;
                        GameLayout();
                        RoundText.Text = "Round " + GameViewModel.Game.Round;
                        WinnerText.Text = "You lose!";
                        WinnerText.Foreground = Brushes.Crimson;
                        RoundScreen.Visibility = Visibility.Visible;
                        roundTimer.Start();
                    }
                    else
                    {
                        GameViewModel.Game.Next();
                        GameViewModel.Game.Won = false;
                        GameLayout();
                        RoundText.Text = "Round " + GameViewModel.Game.Round;
                        WinnerText.Text = "Draw";
                        WinnerText.Foreground = Brushes.Blue;
                        RoundScreen.Visibility = Visibility.Visible;
                        roundTimer.Start();
                    }



                }
                else
                    StartTimer();
            }
            else
                StartTimer();
        }

        private void Draw()
        {
            Random random = new Random();
            int x = random.Next(GameViewModel.GraveYard.Count);
            GameViewModel.GraveYard[x].BoneView.Clicked += DominoSelect;
            GameViewModel.Player.Bones.Add(GameViewModel.GraveYard[x]);
            GameViewModel.GraveYard.Remove(GameViewModel.GraveYard[x]);
            UpdateHand(PlrBoard, GameViewModel.Player.Bones, false);
            CardsCount.Text = GameViewModel.GraveYard.Count.ToString() + " x";
        }

        private void OppPlace()
        {
            if (lengthRight > 3.15 && orientationRight == 0)
            {
                orientationRight += 90;
                turnDown = true;
            }
            if (lengthBottom >= 1 && orientationRight == 90)
            {
                orientationRight += 90;
                turnLeft = true;
            }
            if (lengthLeft > 3.15 && orientationLeft == 0)
            {
                orientationLeft += 90;
                turnUp = true;
            }
            if (lengthTop >= 1 && orientationLeft == 90)
            {
                orientationLeft += 180;
                turnRight = true;
            }

            BoneView OppBoneView = OppBone.BoneView;

            if (right == OppBone.Bone.Right)
            {
                if (orientationRight == 0)
                {
                    Board.Children.Add(OppBone.BoneView);

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 123);
                        lengthRight += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 180);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 123);
                        }
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 166);

                        lengthRight += 1;
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView));
                    right = OppBone.Bone.Left;
                    RightViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else if (orientationRight == 90)
                {
                    Board.Children.Add(OppBoneView);
                    if (turnDown)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 0);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 82);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 42);
                        }
                        else
                        {
                            Rotate(OppBoneView, 270);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 124);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                            }
                        }
                        lengthBottom += (float)0.5;
                        right = OppBone.Bone.Left;
                        RightViewModel = OppBone;
                        turnDown = false;
                        PlaceBone(OppBone);
                        OppBone = null;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 0);
                        Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 124);
                        lengthBottom += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 270);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 124);
                        else
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 166);
                        lengthBottom += 1;
                    }
                    Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView));
                    right = OppBone.Bone.Left;
                    RightViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else
                {
                    Board.Children.Add(OppBoneView);
                    if (turnLeft)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 90);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 42);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 82);
                        }
                        else
                        {
                            Rotate(OppBoneView, 0);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 82);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                            }
                        }
                        right = OppBone.Bone.Left;
                        RightViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnLeft = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                    }
                    else
                    {
                        Rotate(OppBoneView, 0);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 166);
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView));
                    PlaceBone(OppBone);
                    right = OppBone.Bone.Left;
                    RightViewModel = OppBone;
                    OppBone = null;
                    turnLeft = false;
                    return;
                }
            }

            if (right == OppBone.Bone.Left)
            {
                if (orientationRight == 0)
                {
                    Board.Children.Add(OppBone.BoneView);

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                        lengthRight += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 0);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 124);
                        }
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 166);

                        lengthRight += 1;
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView));
                    right = OppBone.Bone.Right;
                    RightViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else if (orientationRight == 90)
                {
                    Board.Children.Add(OppBoneView);
                    if (turnDown)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 0);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 82);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 42);
                        }
                        else
                        {
                            Rotate(OppBoneView, 90);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 82);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 124);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) + 42);
                            }
                        }
                        lengthBottom += (float)0.5;
                        right = OppBone.Bone.Right;
                        RightViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnDown = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 0);
                        Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 123);
                        lengthBottom += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 90);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 123);
                        else
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 166);
                        lengthBottom += 1;
                    }
                    Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView));
                    right = OppBone.Bone.Right;
                    RightViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else
                {
                    Board.Children.Add(OppBoneView);
                    if (turnLeft)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 90);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 42);
                            Canvas.SetLeft(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) - 82);
                        }
                        else
                        {
                            Rotate(OppBoneView, 180);
                            if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 82);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView) + 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                            }
                        }
                        right = OppBone.Bone.Right;
                        RightViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnLeft = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 123);
                    }
                    else
                    {
                        Rotate(OppBoneView, 180);

                        if (RightViewModel.Bone.Left == RightViewModel.Bone.Right)
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 124);
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(RightViewModel.BoneView) - 166);
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(RightViewModel.BoneView));
                    right = OppBone.Bone.Right;
                    RightViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
            }

            if (left == OppBone.Bone.Right)
            {
                if (orientationLeft == 0)
                {
                    Board.Children.Add(OppBone.BoneView);

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 123);

                        lengthLeft += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 0);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 123);
                        }
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 166);

                        lengthLeft += 1;
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView));
                    left = OppBone.Bone.Left;
                    LeftViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else if (orientationLeft == 90)
                {
                    Board.Children.Add(OppBoneView);
                    if (turnUp)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 0);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 42);
                        }
                        else
                        {
                            Rotate(OppBoneView, 90);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                            }
                        }
                        lengthTop += (float)0.5;
                        left = OppBone.Bone.Left;
                        LeftViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnUp = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 0);
                        Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                        lengthTop += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 90);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                        else
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 166);
                        lengthTop += 1;
                    }
                    Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView));
                    left = OppBone.Bone.Left;
                    LeftViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else
                {
                    Board.Children.Add(OppBoneView);
                    if (turnRight)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 90);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                        }
                        else
                        {
                            Rotate(OppBoneView, 180);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                            }
                        }
                        left = OppBone.Bone.Left;
                        LeftViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnRight = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                    }
                    else
                    {
                        Rotate(OppBoneView, 180);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 166);

                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView));
                    left = OppBone.Bone.Left;
                    LeftViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
            }

            if (left == OppBone.Bone.Left)
            {
                if (orientationLeft == 0)
                {
                    Board.Children.Add(OppBone.BoneView);

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                        lengthLeft += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 180);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                        {
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 124);
                        }
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 166);
                        lengthLeft += 1;
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView));
                    left = OppBone.Bone.Right;
                    LeftViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else if (orientationLeft == 90)
                {
                    Board.Children.Add(OppBoneView);
                    if (turnUp)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 0);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 42);
                        }
                        else
                        {
                            Rotate(OppBoneView, 270);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) - 42);
                            }
                        }
                        lengthTop += (float)0.5;
                        left = OppBone.Bone.Right;
                        LeftViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnUp = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 0);
                        Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                        lengthTop += (float)0.5;
                    }
                    else
                    {
                        Rotate(OppBoneView, 270);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 124);
                        else
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 166);
                        lengthTop += 1;
                    }
                    Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView));
                    left = OppBone.Bone.Right;
                    LeftViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
                else
                {
                    Board.Children.Add(OppBoneView);
                    if (turnRight)
                    {
                        if (OppBone.Bone.Left == OppBone.Bone.Right)
                        {
                            Rotate(OppBoneView, 90);
                            Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                        }
                        else
                        {
                            Rotate(OppBoneView, 0);
                            if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 82);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 82);
                            }
                            else
                            {
                                Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView) - 42);
                                Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                            }
                        }
                        left = OppBone.Bone.Right;
                        LeftViewModel = OppBone;
                        PlaceBone(OppBone);
                        OppBone = null;
                        turnRight = false;
                        return;
                    }

                    if (OppBone.Bone.Left == OppBone.Bone.Right)
                    {
                        Rotate(OppBoneView, 90);
                        Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                    }
                    else
                    {
                        Rotate(OppBoneView, 0);

                        if (LeftViewModel.Bone.Left == LeftViewModel.Bone.Right)
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 124);
                        else
                            Canvas.SetLeft(OppBoneView, Canvas.GetLeft(LeftViewModel.BoneView) + 166);
                    }
                    Canvas.SetTop(OppBoneView, Canvas.GetTop(LeftViewModel.BoneView));
                    left = OppBone.Bone.Right;
                    LeftViewModel = OppBone;
                    PlaceBone(OppBone);
                    OppBone = null;
                    return;
                }
            }

        }

        private void PlaceBone(BoneViewModel viewModel)
        {
            GameViewModel.Opponent.Bones.Remove(viewModel);
            UpdateHand(OppBoard, GameViewModel.Opponent.Bones, true);
            GameViewModel.Ground.Add(viewModel);
            if (GameViewModel.Opponent.Bones.Count == 0)
            {
                GameViewModel.Opponent.Score += GameViewModel.Player.Total();
                OpponentScore.Text = GameViewModel.Opponent.Score.ToString();
                GameViewModel.Game.Next();
                GameViewModel.Game.Won = false;
                GameLayout();
                RoundText.Text = "Round " + GameViewModel.Game.Round;
                WinnerText.Text = "You lose!";
                WinnerText.Foreground = Brushes.Crimson;
                RoundScreen.Visibility = Visibility.Visible;
                roundTimer.Start();
            }
            else if (left == right)
            {
                int count = 0;
                foreach (var bone in GameViewModel.Ground)
                {
                    if (bone.Bone.Left == left || bone.Bone.Right == left)
                        count++;
                }
                if (count == 7)
                {
                    if (GameViewModel.Player.Total() > GameViewModel.Opponent.Total())
                    {
                        GameViewModel.Player.Score += GameViewModel.Opponent.Total();
                        YourScore.Text = GameViewModel.Player.Score.ToString();
                        GameViewModel.Game.Next();
                        GameViewModel.Game.Won = true;
                        GameLayout();
                        RoundText.Text = "Round " + GameViewModel.Game.Round;
                        WinnerText.Text = "You win!";
                        WinnerText.Foreground = Brushes.Green;
                        RoundScreen.Visibility = Visibility.Visible;
                        roundTimer.Start();
                    }
                    else if (GameViewModel.Player.Total() < GameViewModel.Opponent.Total())
                    {
                        GameViewModel.Opponent.Score += GameViewModel.Player.Total();
                        OpponentScore.Text = GameViewModel.Opponent.Score.ToString();
                        GameViewModel.Game.Next();
                        GameViewModel.Game.Won = false;
                        GameLayout();
                        RoundText.Text = "Round " + GameViewModel.Game.Round;
                        WinnerText.Text = "You lose!";
                        WinnerText.Foreground = Brushes.Crimson;
                        RoundScreen.Visibility = Visibility.Visible;
                        roundTimer.Start();
                    }
                    else
                    {
                        GameViewModel.Game.Next();
                        GameViewModel.Game.Won = true;
                        GameLayout();
                        RoundText.Text = "Round " + GameViewModel.Game.Round;
                        WinnerText.Text = "Draw";
                        WinnerText.Foreground = Brushes.Blue;
                        RoundScreen.Visibility = Visibility.Visible;
                        roundTimer.Start();
                    }
                }
            }
        }

        private void RoundTimer_Tick(object sender, EventArgs e)
        {
            RoundScreen.Visibility = Visibility.Collapsed;
            roundTimer.Stop();
        }

        private void StartTimer()
        {
            
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            OpponentPlay();
            Blocker.Visibility = Visibility.Collapsed;
            timer.Stop();
            if (GameViewModel.GraveYard.Count == 0)
            {
                foreach (var bone in GameViewModel.Player.Bones)
                {
                    if (bone.Bone.Right == right || bone.Bone.Right == left || bone.Bone.Left == right || bone.Bone.Left == left)
                    {
                        SkipText.Visibility = Visibility.Collapsed;
                        break;
                    }
                    else
                    {
                        Blocker.Visibility = Visibility.Visible;
                        SkipText.Visibility = Visibility.Visible;
                        StartTimer();
                    }
                }
            }
        }
    }
}
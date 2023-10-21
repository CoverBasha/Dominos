using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System;
using Dominos.MVVM.ViewModel;

namespace Dominos.MVVM.View
{
    /// <summary>
    /// Interaction logic for Bone.xaml
    /// </summary>
    public partial class BoneView : UserControl 
    {
        public event EventHandler<RoutedEventArgs> Clicked;
        public event EventHandler<RoutedEventArgs> Deployed;
        public int angle = 0;
        public bool Selected = false;

        public BoneView()
        {
            InitializeComponent();
        }

        public BoneView(BoneViewModel bone)
        {
            InitializeComponent();

            Arrange(Left, bone.Bone.Left);
            Arrange(Right, bone.Bone.Right);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            Clicked?.Invoke(this, e);
            Deployed?.Invoke(this, e);
        }

        private void Arrange(Grid grid, byte dots)
        {

            Border b = new Border
            {
                Background = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(2.5)
            };

            Layout.Children.Add(b);
            Grid.SetColumn(b, 1);

            if (dots == 0)
                return;

            if (dots == 1)
            {
                Ellipse e = new Ellipse
                {
                    Fill = new SolidColorBrush(Colors.Black),
                    Margin = new Thickness(4)
                };

                grid.Children.Add(e);
                Grid.SetColumn(e, 1);
                Grid.SetRow(e, 1);
                return;
            }

            if (dots == 2)
            {
                List<Ellipse> ellipses = new List<Ellipse>();
                for (int i = 0; i < dots; i++)
                {
                    Ellipse e = new Ellipse
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(4)
                    };

                    ellipses.Add(e);
                    grid.Children.Add(ellipses[i]);
                }


                Grid.SetColumn(ellipses[1], 2);
                Grid.SetRow(ellipses[1], 2);
                return;
            }

            if (dots == 3)
            {
                List<Ellipse> ellipses = new List<Ellipse>();
                for (int i = 0; i < dots; i++)
                {
                    Ellipse e = new Ellipse
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(4)
                    };
                    ellipses.Add(e);
                    grid.Children.Add(ellipses[i]);
                }


                Grid.SetColumn(ellipses[1], 2);
                Grid.SetRow(ellipses[1], 2);

                Grid.SetColumn(ellipses[2], 1);
                Grid.SetRow(ellipses[2], 1);
                return;
            }

            if (dots == 4)
            {
                List<Ellipse> ellipses = new List<Ellipse>();
                for (int i = 0; i < dots; i++)
                {
                    Ellipse e = new Ellipse
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(4)
                    };
                    ellipses.Add(e);
                    grid.Children.Add(ellipses[i]);
                }


                Grid.SetColumn(ellipses[1], 2);
                Grid.SetRow(ellipses[1], 2);

                Grid.SetColumn(ellipses[2], 2);
                Grid.SetRow(ellipses[2], 0);

                Grid.SetColumn(ellipses[3], 0);
                Grid.SetRow(ellipses[3], 2);
                return;
            }

            if (dots == 5)
            {
                List<Ellipse> ellipses = new List<Ellipse>();
                for (int i = 0; i < dots; i++)
                {
                    Ellipse e = new Ellipse
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(4)
                    };
                    ellipses.Add(e);
                    grid.Children.Add(ellipses[i]);
                }


                Grid.SetColumn(ellipses[1], 2);
                Grid.SetRow(ellipses[1], 2);

                Grid.SetColumn(ellipses[2], 2);
                Grid.SetRow(ellipses[2], 0);

                Grid.SetColumn(ellipses[3], 0);
                Grid.SetRow(ellipses[3], 2);

                Grid.SetColumn(ellipses[4], 1);
                Grid.SetRow(ellipses[4], 1);
                return;
            }

            if (dots == 6)
            {
                List<Ellipse> ellipses = new List<Ellipse>();
                for (int i = 0; i < dots; i++)
                {
                    Ellipse e = new Ellipse
                    {
                        Fill = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(4)
                    };
                    ellipses.Add(e);
                    grid.Children.Add(ellipses[i]);
                }

                Grid.SetColumn(ellipses[1], 2);
                Grid.SetRow(ellipses[1], 2);

                Grid.SetColumn(ellipses[2], 2);
                Grid.SetRow(ellipses[2], 0);

                Grid.SetColumn(ellipses[3], 0);
                Grid.SetRow(ellipses[3], 2);

                Grid.SetColumn(ellipses[4], 1);
                Grid.SetRow(ellipses[4], 0);

                Grid.SetColumn(ellipses[5], 1);
                Grid.SetRow(ellipses[5], 2);
                return;
            }
        }


    }
}
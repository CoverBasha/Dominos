using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using Dominos.MVVM.Model;
using Dominos.MVVM.View;
using Application = System.Windows.Forms.Application;

namespace Dominos.MVVM.ViewModel
{
    public class BoneViewModel : ViewModelBase
    {
        public Bone Bone { get; set; }
        public BoneView BoneView { get; set; }

        public BoneViewModel(byte left, byte right)
        {
            Bone = new Bone(left, right);
            BoneView = new BoneView();
            Arrange(BoneView.Left, left);
            Arrange(BoneView.Right, right);
        }
        private void Arrange(Grid grid, byte dots)
        {

            Border b = new Border
            {
                Background = new SolidColorBrush(Colors.Black),
                Margin = new Thickness(0, 5, 0, 5),
                CornerRadius = new CornerRadius(2.5)
            };

            BoneView.Layout.Children.Add(b);
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

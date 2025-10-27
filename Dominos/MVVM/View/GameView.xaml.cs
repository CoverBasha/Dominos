using Dominos.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Media.Effects;

namespace Dominos.MVVM.View
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
            DataContextChanged += GameView_DataContextChanged;
        }

        private void GameView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewModel.GameViewModel vm)
            {
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(vm.IsEndScreenVisible))
                    {
                        if (vm.IsEndScreenVisible)
                        {
                            GameArea.Effect = new BlurEffect { Radius = 10 };
                        }
                        else
                        {
                            GameArea.Effect = null;
                        }
                    }
                };
            }
        }
    }
}
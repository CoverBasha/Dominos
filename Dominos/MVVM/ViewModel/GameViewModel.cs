using System;
using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows.Input;
using Dominos.Commands;
using Dominos.MVVM.Model;
using Dominos.MVVM.View;
using Dominos.Services;

namespace Dominos.MVVM.ViewModel
{
    public class GameViewModel : ViewModelBase
    {
        public ICommand ExitCommand { get; }

        public static List<BoneViewModel> GraveYard;

        public static List<BoneViewModel> Ground;
        public static Player Player { get; set; }
        public static Opponent Opponent { get; set; }

        public static Game Game { get; set; }

        public GameViewModel(NavigationStore navigationStore,Func<ViewModelBase> prev)
        {

            ExitCommand = new NavigateCommand(navigationStore, prev);

            GraveYard = new List<BoneViewModel>();

            Ground = new List<BoneViewModel>();

            Player = new Player();

            Opponent = new Opponent();

            Game = new Game();
        }
    }
}

using System;
using System.Windows.Input;
using Dominos.Commands;
using Dominos.MVVM.ViewModel;
using Dominos.Services;

namespace Dominos.MVVM.ViewModel
{
    public class MainMenuViewModel : ViewModelBase
    {
        public ICommand StartCommand { get; }

        public MainMenuViewModel(NavigationStore navigationStore, Func<GameViewModel> createViewModel)
        {
            StartCommand = new NavigateCommand(navigationStore, createViewModel);
        }

    }
}

using System;
using System.Windows;
using Dominos.MVVM.ViewModel;
using Dominos.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dominos
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly NavigationStore _navigationStore;

        public App()
        {
            _navigationStore = new NavigationStore();
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            _navigationStore.CurrentViewModel = mainMenuViewModel();

            MainWindow = new MainWindow()
            {
                DataContext = new MainViewModel(_navigationStore)
            };

            MainWindow.Show();
            base.OnStartup(e);
        }

        private MainMenuViewModel mainMenuViewModel()
        {
            return new MainMenuViewModel(_navigationStore, gameViewModel);
        }

        private GameViewModel gameViewModel()
        {
            return new GameViewModel(_navigationStore, mainMenuViewModel);
        }

    }
}

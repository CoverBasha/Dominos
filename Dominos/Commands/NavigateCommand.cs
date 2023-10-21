using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominos.MVVM.ViewModel;
using Dominos.Services;

namespace Dominos.Commands
{
    public class NavigateCommand : CommandBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly Func<ViewModelBase> _next;

        public NavigateCommand(NavigationStore navigationStore, Func<ViewModelBase> next)
        {
            _next = next;
            _navigationStore = navigationStore;
        }

        public override void Execute(object parameter)
        {
            _navigationStore.CurrentViewModel = _next();
        }
    }
}

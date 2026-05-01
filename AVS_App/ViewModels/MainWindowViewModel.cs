using AVS_Common;
using AVS_Common.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVS_App.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand<string> NavigateCommand { get; set; }
        public ObservableCollection<LogEventModel> LogSource => UiLogSink.LogCollection;
        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager =regionManager;
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigatePath)
        {
            if (!string.IsNullOrEmpty(navigatePath))
            {
                _regionManager.RequestNavigate("MainContentRegion",navigatePath);
            }
        }
    }
}

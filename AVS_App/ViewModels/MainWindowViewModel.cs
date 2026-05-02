using AVS_Common;
using AVS_Common.Model;
using AVS_Service;
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
        ///目前是单plc，如果后续有多plc需求，需要修改
        private readonly ICommunicationService _communicationService;

        private bool _isPlcConnected;
        public bool IsPlcConnected
        {
            get => _isPlcConnected;
            set => SetProperty(ref _isPlcConnected, value);
        }

        public DelegateCommand<string> NavigateCommand { get; set; }
        public ObservableCollection<LogEventModel> LogSource => UiLogSink.LogCollection;
        public MainWindowViewModel(IRegionManager regionManager, ICommunicationService communicationService)
        {
            _regionManager = regionManager;
            _communicationService = communicationService;
            IsPlcConnected = _communicationService.IsActive;
            _communicationService.ConnectionStatusChanged += (isConnected) =>
            {
                IsPlcConnected = isConnected;
            };
            NavigateCommand = new DelegateCommand<string>(Navigate);
        }

        private void Navigate(string navigatePath)
        {
            if (!string.IsNullOrEmpty(navigatePath))
            {
                _regionManager.RequestNavigate("MainContentRegion", navigatePath);
            }
        }
    }
}

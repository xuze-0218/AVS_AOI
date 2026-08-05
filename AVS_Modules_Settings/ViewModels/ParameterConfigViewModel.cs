using AVS_Service;
using AVS_Service.Models;
using Prism.Commands;
using Prism.Mvvm;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace AVS_Modules_Settings.ViewModels
{
    public class ParameterConfigViewModel : BindableBase
    {

        private string _lastModuleName = "未分类模块";
        private ICollectionView _parametersView;
        private IParametersConfigService _configService;
        private bool _isInitialized = false;
        private ILogger _logger;
        public ObservableCollection<ParametersConfig> Parameters => _configService.ConfigParams;


        public ICollectionView ParametersView
        {
            get => _parametersView;
            set => SetProperty(ref _parametersView, value);
        }
        private ParametersConfig _selectedParameter;
        public ParametersConfig SelectedParameter
        {
            get => _selectedParameter;
            set
            {
                if (SetProperty(ref _selectedParameter, value) && value != null)
                {
                    _lastModuleName = value.ModuleName;
                }
            }
        }
        public IEnumerable<ParamOutputType> DataTypeValues => Enum.GetValues(typeof(ParamOutputType)).Cast<ParamOutputType>();

        public DelegateCommand AddCommand { get; }
        public DelegateCommand<ParametersConfig> DeleteCommand { get; }
        public DelegateCommand SaveCommand { get; }
        public DelegateCommand AutoGenerateCommand { get; }

        public ParameterConfigViewModel(IParametersConfigService configService, ILogger logger)
        {
            _configService = configService;
            _logger = logger;
            AddCommand = new DelegateCommand(() =>
            {
                if (SelectedParameter != null)
                    _lastModuleName = SelectedParameter.ModuleName;

                var newParam = new ParametersConfig
                {
                    Name = "New_Param",
                 
                    ModuleName = _lastModuleName
                };
                newParam.PropertyChanged += OnParameterPropertyChanged;
                Parameters.Add(newParam);
            });

            DeleteCommand = new DelegateCommand<ParametersConfig>(p => Parameters.Remove(p));
            SaveCommand = new DelegateCommand(() => _configService.SaveConfig());
            initial();
        }
      
        private void OnParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ParametersConfig.ModuleName) && sender is ParametersConfig p)
            {
                _lastModuleName = p.ModuleName;
            }
        }

        public void initial()
        {
            if (_isInitialized) return;
            _isInitialized = true;

            if (ParametersView != null)
            {
                ParametersView.GroupDescriptions.Clear();
                ParametersView.SortDescriptions.Clear();
                if (ParametersView is ListCollectionView lcv)
                {
                    lcv.Refresh();
                }
            }
            ParametersView = new ListCollectionView(Parameters);
            ParametersView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(ParametersConfig.ModuleName)));
            ParametersView.SortDescriptions.Add(new SortDescription(nameof(ParametersConfig.ModuleName), ListSortDirection.Ascending));
            ParametersView.SortDescriptions.Add(new SortDescription(nameof(ParametersConfig.Name), ListSortDirection.Ascending));
        }
    }
}

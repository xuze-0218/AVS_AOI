using AVS_Service;
using AVS_Service.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace AVS_Modules_Settings.ViewModels
{
    public class CommunicationMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Sender}: {Content}";
    }

    public class PlcDebugViewModel : BindableBase, INavigationAware,IDisposable
    {
        private readonly ILogger _logger;
        private readonly IStationConfigService _stationConfigService;
        private readonly ICommunicationService _communicationService;
        public ObservableCollection<StationConfig> Stations => _stationConfigService.Stations;
        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();
        public ObservableCollection<CommunicationMessage> SentMessages { get; } = new ObservableCollection<CommunicationMessage>();

        #region Prop
        private StationConfig _selectedStation;
        public StationConfig SelectedStation
        {
            get => _selectedStation;
            set
            {
                if (SetProperty(ref _selectedStation, value))
                {
                    if (value != null)
                    {
                        CurrentIp = value.IP;
                        CurrentPort = value.Port;
                        CurrentProtocol = value.Protocol;
                        CurrentRole = value.Role;
                        IsConnected = _communicationService.IsActive(value.StationId);
                        UpdateStatusMessage();
                    }
                }
            }
        }

        private string _receivedMessagesText;
        public string ReceivedMessagesText
        {
            get => _receivedMessagesText;
            set => SetProperty(ref _receivedMessagesText, value);
        }

        private string _currentIp = "127.0.0.1";
        public string CurrentIp
        {
            get => _currentIp;
            set => SetProperty(ref _currentIp, value);
        }

        private int _currentPort = 5000;
        public int CurrentPort
        {
            get => _currentPort;
            set => SetProperty(ref _currentPort, value);
        }

        private CommProtocol _currentProtocol = CommProtocol.TCP;
        public CommProtocol CurrentProtocol
        {
            get => _currentProtocol;
            set => SetProperty(ref _currentProtocol, value);
        }

        private CommRole _currentRole = CommRole.Server;
        public CommRole CurrentRole
        {
            get => _currentRole;
            set => SetProperty(ref _currentRole, value);
        }

        private bool _isConnected;
        public bool IsConnected { get => _isConnected; set => SetProperty(ref _isConnected, value); }

        private string _statusMessage = "未连接";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        private string _messageInput;
        public string MessageInput { get => _messageInput; set => SetProperty(ref _messageInput, value); }
        #endregion

        #region Command
        public DelegateCommand SendCommand { get; }
        public DelegateCommand ClearCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand SaveConfigCommand { get; }
        public DelegateCommand SaveStationConfigCommand { get; }
        public DelegateCommand AddNewStationCommand { get; }
        #endregion


        public PlcDebugViewModel(ICommunicationService communicationService, ILogger logger, IStationConfigService stationConfigService)
        {
            _logger = logger;
            _communicationService = communicationService;
            _stationConfigService = stationConfigService;
            if (Stations.Any())
                SelectedStation = Stations[0];

            ConnectCommand = new DelegateCommand(() =>
            {
                if (SelectedStation == null) return;
                ApplyToSelectedStation();
                _communicationService.Start(SelectedStation.StationId,
                    SelectedStation.Protocol, SelectedStation.Role,
                    SelectedStation.IP, SelectedStation.Port);
                IsConnected = true;
                StatusMessage = $"已连接 {SelectedStation.IP}:{SelectedStation.Port}";
            });
            DisconnectCommand = new DelegateCommand(() =>
            {
                if (SelectedStation == null) return;
                _communicationService.Stop(SelectedStation.StationId);
                IsConnected = false;
                StatusMessage = "已断开";
            });
            SendCommand = new DelegateCommand(async () =>
            {
                if (SelectedStation == null) return;
                await _communicationService.SendAsync(SelectedStation.StationId, MessageInput);
            });
            SaveStationConfigCommand = new DelegateCommand(() =>
            {
                ApplyToSelectedStation();
                _stationConfigService.Save();
                StatusMessage = "配置已保存";
            });
            AddNewStationCommand = new DelegateCommand(() =>
            {
                var newStation = new StationConfig
                {
                    StationId = $"Station{Stations.Count + 1}",
                    IP = "127.0.0.1",
                    Port = 5000,
                    Protocol = CommProtocol.TCP,
                    Role = CommRole.Server
                };
                Stations.Add(newStation);
                SelectedStation = newStation;
                StatusMessage = "新增工位，请编辑并保存";
            });
            ClearCommand = new DelegateCommand(() =>
            {
                Logs.Clear();
                SentMessages.Clear();
                ReceivedMessagesText = string.Empty;
                _logger.Information("日志已清空");
            });
            _communicationService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _communicationService.LogMessage += m => Application.Current?.Dispatcher.Invoke(() =>
            {
                Logs.Insert(0, $"{DateTime.Now:HH:mm:ss} {m}");
            });
            _communicationService.MessageReceived += (s, m) => HandleMessage(s, m);
            UpdateStatusMessage();
            UpdateCommandsCanExecute();
            if (SelectedStation != null)
            {
                IsConnected = _communicationService.IsActive(SelectedStation.StationId);
                UpdateStatusMessage();
            }
            _logger.Debug("通讯配置界面已打开");
        }

        private void OnConnectionStatusChanged(string stationId, bool isConnected)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                return;

            dispatcher.Invoke(() =>
            {
                if (SelectedStation != null && stationId == SelectedStation.StationId)
                {
                    IsConnected = isConnected;
                    UpdateStatusMessage();
                }
                UpdateCommandsCanExecute();
            });
        }

        private void ApplyToSelectedStation()
        {
            if (SelectedStation == null) return;
            SelectedStation.IP = CurrentIp;
            SelectedStation.Port = CurrentPort;
            SelectedStation.Protocol = CurrentProtocol;
            SelectedStation.Role = CurrentRole;
        }

        private void HandleMessage(string source, string message)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                return;

            dispatcher.Invoke(() =>
            {
                var newMessage = new CommunicationMessage
                {
                    Sender = source,
                    Content = message,
                    Timestamp = DateTime.Now
                };

                string line = newMessage.DisplayText + Environment.NewLine;
                ReceivedMessagesText += line;

                Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] 来自 [{source}]: {message}");
                _logger.Information("【接收】{Source}: {Message}", source, message);
            });
        }

        private void UpdateCommandsCanExecute()
        {
            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            SendCommand.RaiseCanExecuteChanged();
        }

        private void UpdateStatusMessage()
        {
            StatusMessage = IsConnected ? "已连接" : "未连接";
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 每次导航到此页面时触发
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void Dispose()
        {
            _communicationService.ConnectionStatusChanged -= OnConnectionStatusChanged;
        }
    }
}

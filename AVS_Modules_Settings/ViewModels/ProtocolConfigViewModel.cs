using AVS_Common.Model;
using AVS_Service.Models;
using AVS_Service.Services;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AVS_Modules_Settings.ViewModels
{
    public class ProtocolConfigViewModel : BindableBase
    {
        private readonly IProtocolEngineService _protocolEngine;
        private readonly IParametersConfigService _paramConfig;
        private readonly ICommunicationService _communicationService;
        private readonly IProtocolConfigRepository _protocolConfigRepository;
        private readonly ILogger _logger;
        private string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/ProtocolConfig.json");
         
        private StationProtocolConfig _currentStationConfig;
        private Action<string, string> _variableChangedHandler;
       

        #region Prop
        private ObservableCollection<ProtocolField> _inputFields;
        public ObservableCollection<ProtocolField> InputFields
        {
            get => _inputFields;
            set
            {
                if (_inputFields != null)
                    _inputFields.CollectionChanged -= OnInputFieldsChanged;
                if (SetProperty(ref _inputFields, value))
                {
                    if (_inputFields != null)
                        _inputFields.CollectionChanged += OnInputFieldsChanged;
                }
            }
        }

        private ObservableCollection<ProtocolField> _outputFields;
        public ObservableCollection<ProtocolField> OutputFields
        {
            get => _outputFields;
            set
            {
                if (_outputFields != null)
                    _outputFields.CollectionChanged -= OnOutputFieldsChanged;
                if (SetProperty(ref _outputFields, value))
                {
                    if (_outputFields != null)
                        _outputFields.CollectionChanged += OnOutputFieldsChanged;
                }
            }
        }

        private ObservableCollection<SessionConfig> _sessions;
        public ObservableCollection<SessionConfig> Sessions
        {
            get => _sessions;
            set => SetProperty(ref _sessions, value);
        }

        private SessionConfig _selectedSession;
        public SessionConfig SelectedSession
        {
            get => _selectedSession;
            set
            {
                if (SetProperty(ref _selectedSession, value))
                {
                    OnSessionChanged();
                }
            }
        }

        private bool _isEditingCommonHeader = false;
        public bool IsEditingCommonHeader
        {
            get => _isEditingCommonHeader;
            set => SetProperty(ref _isEditingCommonHeader, value);
        }

        private string _editableFuncCode;
        public string EditableFuncCode
        {
            get => _editableFuncCode;
            set
            {
                if (SetProperty(ref _editableFuncCode, value))
                {
                    if (SelectedSession != null && SelectedSession.FuncCode != value)
                    {
                        SelectedSession.FuncCode = value;
                        // 强制刷新下拉框显示
                        Sessions = new ObservableCollection<SessionConfig>(_currentStationConfig.Messages);
                    }
                }
            }
        }

        private string _currentStationId;
        public string CurrentStationId
        {
            get => _currentStationId;
            set => SetProperty(ref _currentStationId, value);
        }

        private int _totalLength;
        public int TotalLength
        {
            get => _totalLength;
            set => SetProperty(ref _totalLength, value);
        }

        private string _testRawData = "";
        public string TestRawData
        {
            get => _testRawData;
            set => SetProperty(ref _testRawData, value);
        }

        private string _parseResult = "";
        public string ParseResult
        {
            get => _parseResult;
            set => SetProperty(ref _parseResult, value);
        }

        private string _generatedMessage = "";
        public string GeneratedMessage
        {
            get => _generatedMessage;
            set => SetProperty(ref _generatedMessage, value);
        }

        private string _previewMessage = "";
        public string PreviewMessage
        {
            get => _previewMessage;
            set => SetProperty(ref _previewMessage, value);
        }

        private string _statusMessage = "就绪";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _templateName = "DefaultTemplate";
        public string TemplateName
        {
            get => _templateName;
            set => SetProperty(ref _templateName, value);
        }

        private bool _isListeningPlc = false;
        public bool IsListeningPlc
        {
            get => _isListeningPlc;
            set => SetProperty(ref _isListeningPlc, value);
        }

        private string _listenButtonText = "开始监听 PLC";
        public string ListenButtonText
        {
            get => _listenButtonText;
            set => SetProperty(ref _listenButtonText, value);
        }
        #endregion


        #region Command
        public DelegateCommand AddInputFieldCommand { get; private set; }
        public DelegateCommand AddOutputFieldCommand { get; private set; }
        public DelegateCommand SortCommand { get; private set; }
        public DelegateCommand ParseTestCommand { get; private set; }
        public DelegateCommand PreviewOutputCommand { get; private set; }
        public DelegateCommand SaveConfigCommand { get; private set; }
        public DelegateCommand LoadConfigCommand { get; private set; }
        public DelegateCommand ToggleListenCommand { get; private set; }
        public DelegateCommand AddSessionCommand { get; private set; }
        public DelegateCommand DeleteSessionCommand { get; private set; }
        public DelegateCommand SelectCommonHeaderCommand { get; private set; }
        #endregion


        public ProtocolConfigViewModel(
            ILogger logger,
            IParametersConfigService paramConfig,
            IProtocolEngineService protocolEngine,
            ICommunicationService communicationService,
            IProtocolConfigRepository protocolConfigRepository)
        {
            _paramConfig = paramConfig;
            _protocolEngine = protocolEngine;
            _communicationService = communicationService;
            _protocolConfigRepository = protocolConfigRepository;
            _logger = logger;

            Sessions = new ObservableCollection<SessionConfig>();
            InitializeCommands();
            LoadConfig();
            SubscribeToCommunicationEvents();
            _logger?.Information("ProtocolConfigViewModel 已初始化");
        }

        // 新增对话报文
        private void ExecuteAddSession()
        {
            var newSession = new SessionConfig
            {
                FuncCode = $"2{_currentStationConfig.Messages.Count + 1:000}",
                Description = "新业务"
            };
            _currentStationConfig.Messages.Add(newSession);
            Sessions = new ObservableCollection<SessionConfig>(_currentStationConfig.Messages);
            SelectedSession = newSession;
        }

        private void InitializeCommands()
        {
            AddInputFieldCommand = new DelegateCommand(() =>
            {
                var newField = new ProtocolField { Name = $"Input_{InputFields.Count}", StartIndex = 0, Length = 4, Description = "新字段" };
                InputFields.Add(newField);
            });

            AddOutputFieldCommand = new DelegateCommand(() =>
            {
                var newField = new ProtocolField { Index = OutputFields.Count, Name = $"Output_{OutputFields.Count}", Source = FieldSource.Fixed, Length = 4, Scale = 1.0, FixedValue = $"Output_{OutputFields.Count}" };
                OutputFields.Add(newField);
            });

            AddSessionCommand = new DelegateCommand(ExecuteAddSession);
            DeleteSessionCommand = new DelegateCommand(ExecuteDeleteSession);

            SelectCommonHeaderCommand = new DelegateCommand(() =>
            {
                IsEditingCommonHeader = true;
                SelectedSession = null;
                EditableFuncCode = "公共头 (只读)";
                LoadFieldsToUI(_currentStationConfig.CommonHeaderFields, new List<ProtocolField>());
                StatusMessage = "正在编辑：公共头";
            });



            SortCommand = new DelegateCommand(() =>
            {
                //var sorted = new ObservableCollection<ProtocolField>(OutputFields.OrderBy(f => f.Index).ToList());
                //OutputFields.Clear();
                ////foreach (var field in sorted) OutputFields.Add(field);
                //OutputFields = sorted;
                //UpdateOutputPreview();
                var sortedList = OutputFields.OrderBy(f => f.Index).ToList();
                for (int i = 0; i < sortedList.Count; i++)
                    sortedList[i].Index = i;
                OutputFields = new ObservableCollection<ProtocolField>(sortedList);
                // OutputFields setter 现在应自动处理事件（见下面第3点），无需手动 AttachOutputFieldsEvents
                UpdateTotalLength();
                UpdateOutputPreview();
            });

            ParseTestCommand = new DelegateCommand(() => ExecuteParseTest());
            PreviewOutputCommand = new DelegateCommand(() => ExecutePreviewOutput());
            SaveConfigCommand = new DelegateCommand(() => ExecuteSaveConfig());
            LoadConfigCommand = new DelegateCommand(() => LoadConfig());
            ToggleListenCommand = new DelegateCommand(() => ToggleListen());
        }

        private void OnSessionChanged()
        {
            if (_selectedSession != null)
            {
                EditableFuncCode = _selectedSession.FuncCode;
                LoadFieldsToUI(_selectedSession.InputFields, _selectedSession.OutputFields);
                StatusMessage = $"正在编辑对话：{_selectedSession.FuncCode}";
            }
        }

        private void ExecuteDeleteSession()
        {
            if (SelectedSession == null) return;
            _currentStationConfig.Messages.Remove(SelectedSession);
            Sessions = new ObservableCollection<SessionConfig>(_currentStationConfig.Messages);
            StatusMessage = "对话已删除";
        }

        private void LoadFieldsToUI(List<ProtocolField> inputs, List<ProtocolField> outputs)
        {
            //InputFields = new ObservableCollection<ProtocolField>(CloneFields(inputs));
            //OutputFields = new ObservableCollection<ProtocolField>(CloneFields(outputs));

            //InputFields.CollectionChanged += (s, e) =>
            //{
            //    SyncFieldValues(InputFields);
            //    SyncConfigToEngine();
            //};

            //OutputFields.CollectionChanged += (s, e) =>
            //{
            //    if (e.NewItems != null) foreach (ProtocolField f in e.NewItems.Cast<ProtocolField>()) AttachFieldHandlers(f);
            //    if (e.OldItems != null) foreach (ProtocolField f in e.OldItems.Cast<ProtocolField>()) DetachFieldHandlers(f);
            //    SyncFieldValues(OutputFields);
            //    SyncConfigToEngine();
            //    UpdateOutputPreview();
            //};

            //UpdateTotalLength();

            InputFields = new ObservableCollection<ProtocolField>(CloneFields(inputs));
            OutputFields = new ObservableCollection<ProtocolField>(CloneFields(outputs));
            UpdateTotalLength();
        }

        private List<ProtocolField> CloneFields(List<ProtocolField> fields)
        {
            if (fields == null) return new List<ProtocolField>();
            var json = JsonConvert.SerializeObject(fields);
            return JsonConvert.DeserializeObject<List<ProtocolField>>(json);
        }

        /// <summary>
        /// 订阅通讯服务事件
        /// </summary>
        private void SubscribeToCommunicationEvents()
        {
            _communicationService.MessageReceived += (sender, message) =>
            {
                if (IsListeningPlc)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            _logger?.Debug("从 {Sender} 接收到消息: {Message}", sender, message);
                            TestRawData = message;
                            ExecuteParseTest(clearVariables: false);

                            PreviewMessage = "已自动解析来自 PLC 的消息";
                            _logger?.Information("自动解析 PLC 消息成功");
                        }
                        catch (Exception ex)
                        {
                            PreviewMessage = $"自动解析失败: {ex.Message}";
                            _logger?.Error(ex, "自动解析 PLC 消息失败");
                        }
                    });
                }
            };

            _logger?.Debug("已订阅通讯服务事件");
        }

        /// <summary>
        ///切换监听 PLC
        /// </summary>
        private void ToggleListen()
        {
            IsListeningPlc = !IsListeningPlc;

            if (IsListeningPlc)
            {
                _variableChangedHandler = new Action<string, string>(OnVariableChanged);
                _protocolEngine.VariableChanged += _variableChangedHandler;
                ListenButtonText = "停止监听 PLC";
                StatusMessage = "正在监听 PLC 消息...";
                PreviewMessage = "已开始监听，等待 PLC 发送电文";
                _logger?.Information("已开始监听 PLC");
            }
            else
            {
                if (_variableChangedHandler != null)
                    _protocolEngine.VariableChanged -= _variableChangedHandler;
                ListenButtonText = "开始监听 PLC";
                StatusMessage = "已停止监听";
                PreviewMessage = "已停止监听";
                _logger?.Information("已停止监听 PLC");
            }
        }

        /// <summary>
        /// 执行接收电文解析测试
        /// </summary>
        private void ExecuteParseTest(bool clearVariables = true)
        {
            try
            {
                if (string.IsNullOrEmpty(TestRawData))
                {
                    //PreviewMessage = "请输入原始电文";
                    //ParseResult = "";
                    return;
                }

                if (clearVariables)
                    _protocolEngine.ClearVariables();
                var inputList = InputFields.ToList();
                _protocolEngine.ParseInput(TestRawData, inputList);

                // 构建解析结果显示
                var result = new StringBuilder();
                result.AppendLine("解析成功:");
                result.AppendLine();
                foreach (var field in inputList)
                {
                    string value = _protocolEngine.GetVariable(field.Name);
                    result.AppendLine($"{field.Name}:");
                    result.AppendLine($"  值: {value}");
                    if (!string.IsNullOrEmpty(field.Description))
                        result.AppendLine($"  说明: {field.Description}");
                    result.AppendLine();
                }

                ParseResult = result.ToString();

                if (!IsListeningPlc)
                    UpdateOutputPreview();
                _logger?.Information("电文解析测试完成");
            }
            catch (Exception ex)
            {
                PreviewMessage = $"解析异常: {ex.Message}";
                ParseResult = $"错误: {ex.Message}";
                StatusMessage = "解析失败";
                _logger?.Error(ex, "电文解析测试失败");
            }
        }

        /// <summary>
        /// 执行输出电文预览
        /// </summary>
        private void ExecutePreviewOutput()
        {
            try
            {
                SyncConfigToEngine();
                UpdateOutputPreview();
                PreviewMessage = "电文生成成功";
                StatusMessage = "输出预览完成";
            }
            catch (Exception ex)
            {
                PreviewMessage = $"生成异常: {ex.Message}";
                GeneratedMessage = $"错误: {ex.Message}";
                StatusMessage = "生成失败";
                _logger?.Error(ex, "输出预览失败");
            }
        }

        /// <summary>
        ///  更新输出预览
        /// </summary>
        private void UpdateOutputPreview()
        {
            try
            {
                if (OutputFields == null || OutputFields.Count == 0)
                {
                    GeneratedMessage = "";
                    return;
                }
                var outputList = OutputFields.OrderBy(f => f.Index).ToList();
                var sb = new StringBuilder();

                foreach (var field in outputList)
                {
                    string fieldContent = "";

                    switch (field.Source)
                    {
                        case FieldSource.Fixed:
                            fieldContent = !string.IsNullOrEmpty(field.FixedValue) ? field.FixedValue : (field.Name ?? "");
                            _logger?.Debug("Fixed字段 {FieldName}: 使用值 {Value}", field.Name, fieldContent);
                            break;

                        case FieldSource.Variable:
                            fieldContent = _protocolEngine.GetVariable(field.Name);
                            if (string.IsNullOrEmpty(fieldContent))
                            {
                                fieldContent = field.FixedValue ?? "";
                                _logger?.Debug("Variable字段 {FieldName}: 变量不存在，使用默认值 {Value}",
                                    field.Name, fieldContent);
                            }
                            else
                            {
                                if (field.Scale != 1.0 && double.TryParse(fieldContent, out double d))
                                {
                                    fieldContent = Math.Round(d * field.Scale).ToString();
                                }
                                _logger?.Debug("Variable字段 {FieldName}: 使用变量值 {Value}", field.Name, fieldContent);
                            }
                            break;

                        case FieldSource.Padding:
                            fieldContent = "";
                            break;
                    }

                    int actualLength = field.GetActualLength(_protocolEngine.VariablePool);
                    string alignedContent = fieldContent.Length > actualLength
                        ? fieldContent.Substring(0, actualLength)
                        : fieldContent.PadLeft(actualLength, '0');

                    field.Preview = alignedContent;
                    sb.Append(alignedContent);

                    _logger?.Debug("预览 {FieldName}: {Preview}", field.Name, field.Preview);
                }

                GeneratedMessage = sb.ToString();
                _logger?.Debug("输出预览已更新，长度: {Length}", GeneratedMessage.Length);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "更新输出预览异常");
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private void ExecuteSaveConfig()
        {
            try
            {

                if (IsEditingCommonHeader)
                {
                    _currentStationConfig.CommonHeaderFields = InputFields.ToList();
                }
                else if (SelectedSession != null)
                {
                    SelectedSession.FuncCode = EditableFuncCode; // 保存重命名
                    SelectedSession.InputFields = InputFields.ToList();
                    SelectedSession.OutputFields = OutputFields.ToList();
                }

                //读取现有的全量配置文件
                List<StationProtocolConfig> allStations = new List<StationProtocolConfig>();
                if (File.Exists(_configPath))
                {
                    var existingJson = File.ReadAllText(_configPath);
                    allStations = JsonConvert.DeserializeObject<List<StationProtocolConfig>>(existingJson) ?? new List<StationProtocolConfig>();
                }

                //替换当前工位的配置
                var existStation = allStations.FirstOrDefault(s => s.StationId == CurrentStationId);
                if (existStation != null) allStations.Remove(existStation);
                allStations.Add(_currentStationConfig);

                //序列化覆盖写入
                string dirPath = Path.GetDirectoryName(_configPath);
                if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

                string newJson = JsonConvert.SerializeObject(allStations, Formatting.Indented);
                File.WriteAllText(_configPath, newJson);
                _protocolConfigRepository.ReloadConfig(); // 通知仓库重新加载配置
                SyncConfigToEngine();
                StatusMessage = "配置已保存";
            }
            catch (Exception ex)
            {
                StatusMessage = "保存失败";
                _logger.Error(ex, "保存配置失败");
            }
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                CurrentStationId = _paramConfig.GetString("Global", "CurrentStationID", "Station01");
                List<StationProtocolConfig> allStations = new List<StationProtocolConfig>();
                if (File.Exists(_configPath))
                {
                    var json = File.ReadAllText(_configPath);
                    allStations = JsonConvert.DeserializeObject<List<StationProtocolConfig>>(json);
                }

                _currentStationConfig = allStations.FirstOrDefault(s => s.StationId == CurrentStationId);
                if (_currentStationConfig == null)
                {
                    _currentStationConfig = new StationProtocolConfig { StationId = CurrentStationId };
                }

                Sessions = new ObservableCollection<SessionConfig>(_currentStationConfig.Messages);
                StatusMessage = "配置已加载";
            }
            catch (Exception ex)
            {
                StatusMessage = "加载失败";
                _logger.Error(ex, "加载配置失败");
            }
        }

        /// 字段集合变化时（添加/删除）自动同步
        /// 保存配置后同步
        /// 加载配置后同步
        /// </summary>
        private void SyncConfigToEngine()
        {
            try
            {
                var config = new FullProtocolConfig
                {
                    TemplateName = TemplateName,
                    InputFields = InputFields.ToList(),
                    OutputFields = OutputFields.ToList()
                };

                if (config.InputFields != null && config.InputFields.Count > 0)
                {
                    _protocolEngine.UpdateInputFields(config.InputFields);
                    _logger?.Debug("已同步 {Count} 个输入字段到 ProtocolEngineService",
                        config.InputFields.Count);
                }
                else
                {
                    _logger?.Warning("输入字段为空");
                }

                if (config.OutputFields != null && config.OutputFields.Count > 0)
                {
                    _protocolEngine.UpdateOutputFields(config.OutputFields);
                    _logger?.Debug("已同步 {Count} 个输出字段到 ProtocolEngineService",
                        config.OutputFields.Count);
                }
                else
                {
                    _logger?.Warning("输出字段为空");
                }

                _logger?.Information(
                    "配置已同步- 输入字段: {InputCount}, 输出字段: {OutputCount}",
                    config.InputFields?.Count ?? 0,
                    config.OutputFields?.Count ?? 0);
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "同步配置到 ProtocolEngineService 失败");
            }
        }

        private void SyncFieldValues(ObservableCollection<ProtocolField> fields)
        {
            try
            {
                foreach (var field in fields)
                {
                    if (field.Source == FieldSource.Fixed)
                    {
                        // 仅当 FixedValue 为空时将 Name 赋给 FixedValue，避免覆盖用户已输入的固定值
                        if (!string.IsNullOrEmpty(field.Name) && string.IsNullOrEmpty(field.FixedValue))
                        {
                            field.FixedValue = field.Name;
                            _logger?.Debug("同步 Fixed 字段: Name={Name} → FixedValue={Value}",
                                field.Name, field.FixedValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "同步字段值异常");
            }
        }

        private void UpdateTotalLength()
        {
            TotalLength = OutputFields.Sum(f => f.Length);
        }

        private void AttachFieldHandlers(ProtocolField field)
        {
            if (field == null) return;
            field.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Field_PropertyChanged);
        }

        private void DetachFieldHandlers(ProtocolField field)
        {
            if (field == null) return;
            field.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(Field_PropertyChanged);
        }

        private void Field_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (!(sender is ProtocolField field)) return;
            if (e.PropertyName == nameof(ProtocolField.Source))
            {
                try
                {
                    if (field.Source == FieldSource.Variable)
                    {
                        if (!string.IsNullOrEmpty(field.FixedValue))
                        {
                            field.FixedValue = "";
                            _logger?.Debug("字段 {Name} 切换为 Variable，已清空 FixedValue", field.Name);
                        }
                    }
                    else if (field.Source == FieldSource.Fixed)
                    {
                        if (string.IsNullOrEmpty(field.FixedValue) && !string.IsNullOrEmpty(field.Name))
                        {
                            field.FixedValue = field.Name;
                            _logger?.Debug("字段 {Name} 切换为 Fixed，FixedValue 设为 Name 默认值", field.Name);
                        }
                    }
                    SyncConfigToEngine();
                    UpdateOutputPreview();
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "处理字段 Source 变更时出错");
                }
            }
            if (e.PropertyName == nameof(ProtocolField.Name))
            {
                try
                {
                    if (field.Source == FieldSource.Fixed && string.IsNullOrEmpty(field.FixedValue) && !string.IsNullOrEmpty(field.Name))
                    {
                        field.FixedValue = field.Name;
                        _logger?.Debug("字段 Name 变更且 Source=Fixed，FixedValue 同步为 Name");
                        SyncConfigToEngine();
                        UpdateOutputPreview();
                    }
                }
                catch (Exception ex)
                {
                    _logger?.Error(ex, "处理字段 Name 变更时出错");
                }
            }
        }

        private void OnInputFieldsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SyncFieldValues(InputFields);
            SyncConfigToEngine();
        }

        private void OnOutputFieldsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (ProtocolField f in e.NewItems.Cast<ProtocolField>()) AttachFieldHandlers(f);
            if (e.OldItems != null)
                foreach (ProtocolField f in e.OldItems.Cast<ProtocolField>()) DetachFieldHandlers(f);
            SyncFieldValues(OutputFields);
            SyncConfigToEngine();
            UpdateOutputPreview();
        }

        private void OnVariableChanged(string name, string val)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                UpdateOutputPreview();
            });
        }
    }
}

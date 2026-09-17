using Prism.Mvvm;


namespace AVS_Service.Models
{
    public enum ParamOutputType
    {
        INT,
        FLOAT,
        BOOL,
        STRING
    }
    /// <summary>
    /// 参数配置
    /// </summary>
    public class ParametersConfig : BindableBase
    {
        private string _moduleName;
        /// <summary>
        /// 所属模块
        /// </summary>
        public string ModuleName { get { return _moduleName; } set => SetProperty(ref _moduleName, value); }

        private string _name;
        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get => _name; set => SetProperty(ref _name, value); }

       
        private string _expression;
        /// <summary>
        /// 实际值的表达式
        /// </summary>
        public string Expression { get => _expression; set => SetProperty(ref _expression, value); }

       
        private string _note;
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Note { get => _note; set => SetProperty(ref _note, value); }

        /// <summary>
        /// 初始值，字符串形式，具体类型由OutputType决定
        /// </summary>
        private string _initValue;
        /// <summary>
        /// 初始值
        /// </summary>
        public string InitValue { get => _initValue; set => SetProperty(ref _initValue, value); }

        private ParamOutputType _outputType = ParamOutputType.FLOAT;
        /// <summary>
        /// 属性类型
        /// </summary>
        public ParamOutputType OutputType { get => _outputType; set => SetProperty(ref _outputType, value); }

        //private double _minValue = 0.0;
        //public double MinValue { get => _minValue; set => SetProperty(ref _minValue, value); }

        //private double _maxValue = 10.0;
        //public double MaxValue { get => _maxValue; set => SetProperty(ref _maxValue, value); }

        //private bool _enabled = true;
        //public bool Enabled { get => _enabled; set => SetProperty(ref _enabled, value); }
    }
}

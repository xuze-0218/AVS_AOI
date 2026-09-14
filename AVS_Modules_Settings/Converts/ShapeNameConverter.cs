using AVS_Service.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AVS_Modules_Settings.Converts
{
    public  class ShapeNameConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is BarShape s ? (s == BarShape.SquareBar ? "方形" : "圆形") : string.Empty;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }

    public  class DataTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is VisionDimension d ? (d == VisionDimension.ThreeD ? "3D" : "2D") : string.Empty;
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => throw new NotSupportedException();
    }
}

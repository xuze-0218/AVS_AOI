using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AVS_Common.Converts
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value is bool b && b;

            // ConverterParameter="Inverse" 时反转
            if (parameter?.ToString() == "Inverse")
                result = !result;

            return result
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;

                if (parameter?.ToString() == "Inverse")
                    result = !result;

                return result;
            }

            return false;
        }
    }
}

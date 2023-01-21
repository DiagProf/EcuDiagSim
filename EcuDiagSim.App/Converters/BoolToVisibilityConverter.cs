using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace EcuDiagSim.App.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isVisible)
            {
                return isVisible is true
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            throw new ArgumentException("BoolToVisibilityConverterValueMustBeBool");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException("BoolToVisibilityConverterConvertBack");
    }
}
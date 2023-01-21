using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace EcuDiagSim.App.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                if (Enum.TryParse(enumString, out ElementTheme elementTheme) is true)
                {
                    return elementTheme.Equals(value);
                }
            }

            throw new ArgumentException("ExceptionEnumToBoolConverterParameterMustBeAnEnumName");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("EnumToBoolConverterConvertBack");
        }
    }
}
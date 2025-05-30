using System;
using System.Globalization;
using System.Windows.Data;

namespace DormitoryManagement.Converters;
public class BoolToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? parameter : 50;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StafflyApp.Helpers
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Nếu chuỗi rỗng hoặc null thì ẨN (Collapsed), ngược lại thì HIỆN (Visible)
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object Parameter { get; set; }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;
using System.Windows.Media;

namespace Haley.MVVM.Converters
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
//Incoming object is expected to be a color.
            if (value is Color clr)
            {
                return new SolidColorBrush(clr);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brsh)
            {
                return brsh.Color;
            }
            return value;
        }
    }
}

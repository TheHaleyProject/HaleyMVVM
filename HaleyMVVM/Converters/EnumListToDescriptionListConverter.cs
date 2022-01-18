using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;
using Haley.Utils;

namespace Haley.MVVM.Converters
{
    public class EnumListToDescriptionListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<Enum> input = (List<Enum>)value;
            List<string> result = new List<string>();
            foreach (Enum e in input)
            {
                result.Add(e.GetDescription());
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

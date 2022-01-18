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
    public class MultiBindingEqualityCheckConverter : IMultiValueConverter //Can be used to check if the input parameter and the binded value are similar. Both should match integer.
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return false;
            var length = values.Length;

            for (int i = 0; i < length; i++)
            {
                var current = i;
                var next = i + 1;
                if (next == length || next > length) break;

                var current_obj = values[current];
                var next_obj = values[next];
                if (current_obj == null || next_obj == null || current_obj == DependencyProperty.UnsetValue || next_obj == DependencyProperty.UnsetValue) return false;
                //The object cannot be null or unset.
                var current_string = values[current].AsString();
                var next_string = values[next].AsString();
                if (!current_string.Equals(next_string)) return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

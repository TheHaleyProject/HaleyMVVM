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
    public sealed class MultiBindingValueCheckerConverter : IMultiValueConverter //Can be used to check if the input parameter and the binded value are similar. Both should match integer.
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return false;
            if (parameter == null) throw new ArgumentException("For a multibinding value checker, parameter value is mandatory. All the value strings will be checked against the paramter value string to ensure if all matches.");
            var paramInput = parameter.AsString();

            foreach (var item in values) {
                if (!item.AsString().Equals(paramInput,StringComparison.OrdinalIgnoreCase)) return false;
            }

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

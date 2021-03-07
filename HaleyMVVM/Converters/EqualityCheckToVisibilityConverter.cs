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
    public class EqualityCheckToVisibilityConverter : IValueConverter //Can be used to check if the input parameter and the binded value are similar. Both should match integer.
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null) return false;
            string inputparameter = parameter.asString(); // Parameter is always going to be string.
            bool result = value.asString() == inputparameter;
            if (result == true) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visiblestatus = (Visibility)value;
            if (visiblestatus == Visibility.Visible) return parameter.changeType(targetType); //Idea that the bounded source value is checked against the parameter. We try to send the parameter back as integer.
            return null;
        }
    }

}

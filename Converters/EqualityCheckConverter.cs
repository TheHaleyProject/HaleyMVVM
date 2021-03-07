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
    public class EqualityCheckConverter : IValueConverter //Can be used to check if the input parameter and the binded value are similar. Both should match integer.
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null) return false;
            string inputparameter = parameter.asString(); // Parameter is always going to be string.
            return value.asString() == inputparameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool ActualResult = (bool)value; //If actual result is true, then return the parameter, else, return null.

            if (ActualResult == true) return parameter.changeType(targetType); //Idea that the bounded source value is checked against the parameter. We try to send the parameter back as integer.
            if (targetType.IsValueType) return Activator.CreateInstance(targetType); //Which means that the target type is a nonnullable one (it has a default value)
            if (targetType == typeof(string)) return "Error";
            return null;
        }
    }
}

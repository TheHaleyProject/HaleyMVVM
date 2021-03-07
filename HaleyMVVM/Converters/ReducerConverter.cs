using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;

namespace Haley.MVVM.Converters
{
    public class ReducerConverter : IValueConverter //Expecting a width value and a parameter to reduce from it
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double _actual_length = (double)value;
                if (parameter == null) return value; 
                double _reducer = (double)parameter;
                if (_reducer > _actual_length || _reducer == _actual_length) return value; 
                return (_actual_length - _reducer);
            }
            catch (Exception) //In case of any exception return the actual input value
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double _converted_length = (double)value;
                if (parameter == null) return value;
                double _reducer = (double)parameter;
                return (_converted_length + _reducer);
            }
            catch (Exception) //In case of any exception return the actual input value
            {
                return value;
            }
        }
    }
}

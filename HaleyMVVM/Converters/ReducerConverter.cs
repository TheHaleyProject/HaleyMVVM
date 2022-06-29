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
                //Get direct value.
                if (value == null) return value;
                double _actual_length = 0.0;
                if (value is double dblInput) {
                    _actual_length = dblInput;
                }
                else {
                    if (!double.TryParse(value.ToString(), out _actual_length)) return value;
                }

                //Get parameter
                if (parameter == null) return value;
                double _reducer = 0.0;
                if (parameter is double dblParam) {
                    _reducer = dblParam;
                }
                else {
                    if (!double.TryParse(parameter.ToString(), out _reducer)) return value;
                }
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
                double _converted_length = (double)value; //We know that the value is double for sure.

                //Get parameter
                if (parameter == null) return value;
                double _reducer = 0.0;
                if (parameter is double dblParam) {
                    _reducer = dblParam;
                }
                else {
                    if (!double.TryParse(parameter.ToString(), out _reducer)) return value;
                }

                var _actual_length = _converted_length + _reducer;
                if (_actual_length >= (2*_reducer) ) return _converted_length;

                return _actual_length;
            }
            catch (Exception) //In case of any exception return the actual input value
            {
                return value;
            }
        }
    }
}

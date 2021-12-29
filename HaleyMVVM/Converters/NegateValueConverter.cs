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
    public class NegateValueConverter : IValueConverter
    {
        /// <summary>
        /// To negate the value of a converter
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Inverse: False = 0</param>
        /// <param name="parameter">Inverse: True = 1</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return _negateConvert(value);
            }
            catch (Exception)
            {
                return value;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return _negateConvert(value);
            }
            catch (Exception)
            {
                return value;
            }
            
        }
        private object _negateConvert(object value)
        {
            if (value == null) return value;
            if (value is double _dblValue) return _dblValue * -1;
            if (value is int _intValue) return _intValue * -1;
            if (value is Thickness thickVal)
            {
                var _newthick = new Thickness((thickVal.Left * -1), (thickVal.Top * -1), (thickVal.Right * -1), (thickVal.Bottom * -1));
                return _newthick;
            }
            return value;
        }
    }
}

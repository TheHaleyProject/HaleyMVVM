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
    public sealed class NullCheckerConverter : IValueConverter
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
                int inverse = 0; //Sometimes users can choose not to enter parameter value, in such cases, we make 0 as default.
                if (parameter != null) int.TryParse(parameter.ToString(), out inverse);

                //case 1: Input- null , return true
                //case 2: Input- not null, return false
                //case 3: Input- null, Inverse, return false.
                //case 4: Input- not null, inverse, return true.

                if ((value == null && inverse == 0) || (value != null && inverse != 0)) return true;
                return false;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not supposed to convert back.");
        }
    }
}

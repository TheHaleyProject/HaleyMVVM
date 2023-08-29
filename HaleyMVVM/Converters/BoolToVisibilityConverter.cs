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
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// To convert boolean to visibility
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">Inverse: False = 0</param>
        /// <param name="parameter">Inverse: True = 1</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool input = (bool)value;
            int inverse = 0; //Sometimes users can choose not to enter parameter value, in such cases, we make 0 as default.
            if (parameter != null) int.TryParse((string)parameter,out inverse);

            //case 1: Input- true , return visible
            //case 2: Input- false, return collapsed
            //case 3: Input- true, Inverse, return collapsed.
            //case 4: Input- false, inverse, return visible.

            if ((input && inverse == 0) || (!input && inverse != 0)) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility input = (Visibility)value;
            int inverse = 0; 
            if (parameter != null) int.TryParse((string)parameter, out inverse);


            //case 1: Input- Visible , return true
            //case 2: Input- Collapsed, return false
            //case 3: Input- Visible, Inverse, return false.
            //case 4: Input- Collapsed, inverse, return true.
            if ((input == Visibility.Visible && inverse == 0) || (!(input == Visibility.Visible) && inverse != 0)) return true;
            return false;
        }
    }
}

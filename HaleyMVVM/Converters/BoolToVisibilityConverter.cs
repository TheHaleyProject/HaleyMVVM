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
            int inverse = 0; //Sometimes users can choose not to enter parameter value, in such cases, we make 1 as default.
            if (parameter != null) int.TryParse((string)parameter,out inverse); 

            switch (input)
            {
                case true:
                    if (inverse == 0) return Visibility.Visible;
                    return Visibility.Collapsed;
                case false:
                default:
                    if (inverse == 0) return Visibility.Collapsed;
                    return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool input = (bool)value;
            int inverse = 0; //Sometimes users can choose not to enter parameter value, in such cases, we make 1 as default.
            if (parameter != null) int.TryParse((string)parameter, out inverse);

            switch (input)
            {
                case true:
                    if (inverse == 0) return Visibility.Visible;
                    return Visibility.Collapsed;
                case false:
                default:
                    if (inverse == 0) return Visibility.Collapsed;
                    return Visibility.Visible;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Haley.MVVM.Converters {
    public class IntToBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is int input)) return null;
            int inverse = 0;
            if (parameter != null) int.TryParse((string)parameter, out inverse);
            if ((input > 0 && inverse == 0) || (input <= 0 && inverse != 0)) return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is bool input)) return null;
            int inverse = 0;
            if (parameter != null) int.TryParse((string)parameter, out inverse);
            if ((input && inverse == 0) || (!input && inverse != 0)) return 1;
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Haley.MVVM.Converters {
    public class Null2VisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            try {
                int inverse = 0; //Sometimes users can choose not to enter parameter value, in such cases, we make 0 as default.
                if (parameter != null) int.TryParse(parameter.ToString(), out inverse);

                //case 1: Input- null , return true
                //case 2: Input- not null, return false
                //case 3: Input- null, Inverse, return false.
                //case 4: Input- not null, inverse, return true.

                if ((value == null && inverse == 0) || (value != null && inverse != 0)) return  Visibility.Visible;
                return Visibility.Collapsed;
            } catch (Exception) {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}


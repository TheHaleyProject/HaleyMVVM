using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Haley.Utils {
    public class ImageHelper {
        public static void ChangeColorWithName(string propname, DependencyObject d, object value) {
            ImageHelperInternal.ChangeColorWithName(propname, d, value);
        }

        public static void ChangeColor(DependencyProperty prop, DependencyObject d, object value) {
            ImageHelperInternal.ChangeColor(prop, d, value);
        }


        public static ImageSource ChangeColor(ImageSource source, SolidColorBrush brush) {
            return ImageHelperInternal.ChangeColor(source, brush);
        }
    }
}

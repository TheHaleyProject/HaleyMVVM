using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Haley.Enums;
using Haley.Models;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using dwg = System.Drawing;
using System.Windows.Markup;
using System.ComponentModel;
using System.Reflection;

namespace Haley.Utils
{
    public static class CommonUtils {
        public static bool GetTargetElement(IServiceProvider serviceProvider, out DependencyElement target) {
           return InternalUtilsCommon.GetTargetElement(serviceProvider,out target);   
        }

        public static object FetchValueAndMonitor(object sourceObject, string prop_name, PropertyChangedEventHandler PropChangeHandler) {
            return InternalUtilsCommon.FetchValueAndMonitor(sourceObject, prop_name, PropChangeHandler);    
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;
using Haley.Enums;


namespace Haley.MVVM.Converters
{
    public sealed class KeyToControlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
               //GET PARAMETERS
                int param = 0; //Sometimes users can choose not to enter parameter value, in such cases, we make 0 as default.
                if (parameter != null) int.TryParse((string)parameter, out param);
                ResolveMode _resolve_mode = ResolveMode.AsRegistered;
                switch (param)
                {
                    //None
                    case 0:
                        _resolve_mode = ResolveMode.AsRegistered;
                        break;
                    //TargetOnly
                    default:
                        _resolve_mode = ResolveMode.Transient;
                        break;
                }

                return ContainerStore.Controls.GenerateViewFromKey(value, mode: _resolve_mode);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
   
}

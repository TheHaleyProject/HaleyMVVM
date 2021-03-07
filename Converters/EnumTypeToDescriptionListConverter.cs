using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;
using Haley.Utils;

namespace Haley.MVVM.Converters
{
    public class EnumTypeToDescriptionListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var _type = (Type)value;
            var _basetype = _type.BaseType;
            //Input value is expected to be type of enum.
            if (_basetype != typeof(Enum)) return null;
            List<string> result = new List<string>();

            foreach (Enum @enum in Enum.GetValues(_type))
            {
                result.Add(@enum.getDescription());
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

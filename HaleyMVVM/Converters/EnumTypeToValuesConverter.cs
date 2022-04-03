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
    public class EnumTypeToValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null; 
            Type _type = value as Type;
            if (_type == null)
            {
                _type = value.GetType();
                if (_type == null) return null; //Even if we don't get the type now, return.
            }
            var _basetype = _type.BaseType;
            //Input value is expected to be type of enum.
            if (_basetype != typeof(Enum)) return null;
            List<string> result = new List<string>();
            return Enum.GetValues(_type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

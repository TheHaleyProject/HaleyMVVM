using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Reflection;
using System.ComponentModel;
using Haley.Utils;
using System.Collections;


namespace Haley.MVVM.Converters
{
    public sealed class DictionaryValueFetchConveter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!value.IsDictionary() || parameter == null) return value;
            //shoudl work only for dictionary
            //Parameter should be the key which will be used to fetch the value from the input dictionary
            var _key = parameter.ToString();
            var _dic = value as IDictionary;
            if (_dic.Contains(_key)) return _dic[_key];
            return "KeyMissing";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

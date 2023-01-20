using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Haley.Abstractions;
using System.Globalization;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;
using System.Reflection;
using System.Resources;
using Haley.Events;

namespace Haley.Models
{
    public class MarkupValueProvider : ChangeNotifier
    {
        object _value = null;
        public object Value => _value;

        public MarkupValueProvider(object def_value)
        {
            _value = def_value;
        }

        public void ChangeValue(object new_value)
        {
            _value = new_value;
            OnPropertyChanged("Value");
        }
    }
}

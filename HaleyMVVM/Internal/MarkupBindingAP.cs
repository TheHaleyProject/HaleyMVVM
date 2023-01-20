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
    public static class MarkupBindingAP 
    {
        public const string CALLBACK_PREFIX = "CALLBACK";
        public static Func<string, Type, DependencyProperty> DPFetcher = getDpFetcher();

        static Func<string, Type, DependencyProperty> getDpFetcher() {
            var _method = typeof(DependencyProperty).GetMethod("FromName", BindingFlags.Static | BindingFlags.NonPublic);
            var _delegate = (Func<string, Type, DependencyProperty>)_method.CreateDelegate(typeof(Func<string, Type, DependencyProperty>)); //The FromName method takes in string, type and returns a dependency property. We use same signature for creating the delegate. Reason for creating delegate is that we don't have to use Reflection to fetch the FromName method each time. instead we will reuse the delegate which will be fast compared to reflection.
            return _delegate;
        }

        //PROPERTIES ARE ADDED DYNAMICALLY.
        //To add dynamic properties based on the Property Name like Text, Data, IsEnabled etc. .Since the properties are only added to this static class, it will be one time.. May be we might end up with 10-15 properties. It is same like create properties manually but will not add to memory as these same properties will be reused across different objects through out the application.
        internal static void PropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

            var cbName = e.Property.Name + CALLBACK_PREFIX;
            var cb_dp = DPFetcher.Invoke(cbName, typeof(MarkupBindingAP));
            if (cb_dp == null) return;
            var cb = d.GetValue(cb_dp);
            if (cb is Action<object> cbAction) {
                cbAction.Invoke(e.NewValue);
            }
        }
    }
}

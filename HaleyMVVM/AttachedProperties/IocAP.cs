using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Haley.Abstractions;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;
using Haley.Enums;
using Haley.Utils;

namespace Haley.Models
{
    public static class IocAP
    {

        #region Key_String

        public static object GetKey(DependencyObject obj)
        {
            return (object)obj.GetValue(KeyProperty);
        }

        public static void SetKey(DependencyObject obj, object value)
        {
            obj.SetValue(KeyProperty, value);
        }

        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.RegisterAttached("Key", typeof(object), typeof(IocAP), new PropertyMetadata(null));

        #endregion

        #region ResolveMode
        public static ResolveMode GetResolveMode(DependencyObject obj)
        {
            return (ResolveMode)obj.GetValue(ResolveModeProperty);
        }

        public static void SetResolveMode(DependencyObject obj, ResolveMode value)
        {
            obj.SetValue(ResolveModeProperty, value);
        }

        public static readonly DependencyProperty ResolveModeProperty =
            DependencyProperty.RegisterAttached("ResolveMode", typeof(ResolveMode), typeof(IocAP), new PropertyMetadata(ResolveMode.AsRegistered));
        #endregion

        #region FindKey

        public static bool GetFindKey(DependencyObject obj)
        {
            return (bool)obj.GetValue(FindKeyProperty);
        }

        public static void SetFindKey(DependencyObject obj, bool value)
        {
            obj.SetValue(FindKeyProperty, value);
        }

        public static readonly DependencyProperty FindKeyProperty =
            DependencyProperty.RegisterAttached("FindKey", typeof(bool), typeof(IocAP), new PropertyMetadata(false));
        #endregion

        #region InjectVM
        public static bool GetInjectVM(DependencyObject obj)
        {
            return (bool)obj.GetValue(InjectVMProperty);
        }

        public static void SetInjectVM(DependencyObject obj, bool value)
        {
            obj.SetValue(InjectVMProperty, value);
        }

        public static readonly DependencyProperty InjectVMProperty =
            DependencyProperty.RegisterAttached("InjectVM", typeof(bool), typeof(IocAP), new PropertyMetadata(false, InjectVMPropertyChanged));

        private static void InjectVMPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!((bool)e.NewValue)) return; //If value is false, do not do anything.
            //Applicable only to HaleyControls and Windows.
            if (d is UserControl || d is Window)
            {
                try
                {
                    //If d is usercontrol and also implements UserControl, then resolve the viewmodel
                    string _key = _getKey(d);

                    //TODO: ADD IMPLEMENTATIONS TO INCLUDE CUSTOM CONTROLCONTAINER & WINDOW CONTAINER
                    if (d is UserControl)
                    {
                        var _vm = ContainerStore.Singleton.Controls.GenerateViewModelFromKey(_key, GetResolveMode(d));
                        if (_vm != null) //Only if not null, assign it.
                        {
                            ((UserControl)d).DataContext = _vm;
                        }
                    }
                    else if (d is Window)
                    {
                        var _vm = ContainerStore.Singleton.Windows.GenerateViewModelFromKey(_key, GetResolveMode(d));
                        if (_vm != null) //Only if not null, assign it.
                        {
                            ((Window)d).DataContext = _vm;
                        }
                    }
                }
                catch (Exception)
                {
                    //Do not set the viewmodel
                }
            }
        }

        private static string _getKey(DependencyObject d)
        {
            var key_obj = d.GetValue(KeyProperty) as object;

            string _key = string.Empty;
            //new value of e could be string or enum.
            if (key_obj is string)
            {
                _key = key_obj as string;
            }
            else if (key_obj is Enum)
            {
                var _enum = key_obj as Enum;
                if (_enum != null)
                {
                    _key = _enum.GetKey();
                }
            }
                       
            if (string.IsNullOrWhiteSpace(_key)) //If container key is absent, then give preference to finding the key.
            {
                if (GetFindKey(d))
                {
                    if (d is UserControl)
                    {
                        _key = ContainerStore.Singleton.Controls.FindKey(d.GetType());
                    }
                    else
                    {
                        _key = ContainerStore.Singleton.Windows.FindKey(d.GetType());
                    }
                }
                if (_key == null) _key = d.GetType().ToString();
            }
            return _key;
        }
        #endregion
    }
}

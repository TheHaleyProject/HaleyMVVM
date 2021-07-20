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

        public static string GetContainerKey(DependencyObject obj)
        {
            return (string)obj.GetValue(ContainerKeyProperty);
        }

        public static void SetContainerKey(DependencyObject obj, string value)
        {
            obj.SetValue(ContainerKeyProperty, value);
        }

        // Using a DependencyProperty as the backing store for ContainerKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerKeyProperty =
            DependencyProperty.RegisterAttached("ContainerKey", typeof(string), typeof(IocAP), new PropertyMetadata(null));

        #endregion

        #region Key_Enum

        public static Enum GetContainerKeyEnum(DependencyObject obj)
        {
            return (Enum)obj.GetValue(ContainerKeyEnumProperty);
        }

        public static void SetContainerKeyEnum(DependencyObject obj, Enum value)
        {
            obj.SetValue(ContainerKeyEnumProperty, value);
        }

        // Using a DependencyProperty as the backing store for ContainerKeyEnum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerKeyEnumProperty =
            DependencyProperty.RegisterAttached("ContainerKeyEnum", typeof(Enum), typeof(IocAP), new PropertyMetadata(null));

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

        // Using a DependencyProperty as the backing store for ResolveMode.  This enables animation, styling, binding, etc...
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

        // Using a DependencyProperty as the backing store for FindKey.  This enables animation, styling, binding, etc...
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

        // Using a DependencyProperty as the backing store for InjectVM.  This enables animation, styling, binding, etc...
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
                    //If d is usercontrol and also implements ihaleycontrol, then resolve the viewmodel
                    string _key = _getKey(d);
                    if (d is UserControl)
                    {
                        var _vm = ContainerStore.Singleton.controls.generateViewModel(_key, GetResolveMode(d));
                        if (_vm != null) //Only if not null, assign it.
                        {
                            ((UserControl)d).DataContext = _vm;
                        }
                    }
                    else if (d is Window)
                    {
                        var _vm = ContainerStore.Singleton.windows.generateViewModel(_key, GetResolveMode(d));
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
            string _key = GetContainerKey(d);
            //if _key is null, check if enum is present.
            if (_key == null)
            {
                var _enum = GetContainerKeyEnum(d);
                if (_enum != null)
                {
                    _key = _enum.getKey();
                }
            }
            

            if (_key == null) //If container key is absent, then give preference to finding the key.
            {
                if (GetFindKey(d))
                {
                    if (d is UserControl)
                    {
                        _key = ContainerStore.Singleton.controls.findKey(d.GetType());
                    }
                    else
                    {
                        _key = ContainerStore.Singleton.windows.findKey(d.GetType());
                    }
                }
                if (_key == null) _key = d.GetType().ToString();
            }
            return _key;
        }
        #endregion
    }
}

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
    public static class ControlRetrieverAP
    {
        #region Key_String

        public static string GetKey(DependencyObject obj)
        {
            return (string)obj.GetValue(KeyProperty);
        }

        public static void SetKey(DependencyObject obj, string value)
        {
            obj.SetValue(KeyProperty, value);
        }

        // Using a DependencyProperty as the backing store for Key.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyProperty =
            DependencyProperty.RegisterAttached("Key", typeof(string), typeof(ControlRetrieverAP), new PropertyMetadata(null,KeyPropertyChanged));

        #endregion

        #region Key_Enum

        public static Enum GetKeyEnum(DependencyObject obj)
        {
            return (Enum)obj.GetValue(KeyEnumProperty);
        }

        public static void SetKeyEnum(DependencyObject obj, Enum value)
        {
            obj.SetValue(KeyEnumProperty, value);
        }

        // Using a DependencyProperty as the backing store for KeyEnum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyEnumProperty =
            DependencyProperty.RegisterAttached("KeyEnum", typeof(Enum), typeof(ControlRetrieverAP), new PropertyMetadata(null,KeyEnumPropertyChanged));

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
            DependencyProperty.RegisterAttached("ResolveMode", typeof(ResolveMode), typeof(ControlRetrieverAP), new PropertyMetadata(ResolveMode.AsRegistered));
        #endregion

        #region ControlContainer

        public static IControlContainer GetControlContainer(DependencyObject obj)
        {
            return (IControlContainer)obj.GetValue(ControlContainerProperty);
        }

        public static void SetControlContainer(DependencyObject obj, IControlContainer value)
        {
            obj.SetValue(ControlContainerProperty, value);
        }

        // Using a DependencyProperty as the backing store for ControlContainer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ControlContainerProperty =
            DependencyProperty.RegisterAttached("ControlContainer", typeof(IControlContainer), typeof(ControlRetrieverAP), new PropertyMetadata(null));

        #endregion

        #region Private Methods
        static void KeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RetrieveView(d, e);
        }
        static void KeyEnumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //invoke only if key is null.
            var _stringKey = d.GetValue(KeyProperty) as string;
            if (string.IsNullOrEmpty(_stringKey))
            {
                RetrieveView(d, e);
            }
        }

        static void RetrieveView(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                //We donot apply this for windows.
                if (d is Window || d is UserControl) return;

                if (!(d is ContentControl)) return;

                string _key = string.Empty;
                //new value of e could be string or enum.
                if (e.NewValue is string)
                {
                    _key = e.NewValue as string;
                }
                else if (e.NewValue is Enum)
                {
                    var _enum = e.NewValue as Enum;
                    if (_enum != null)
                    {
                        _key = _enum.getKey();
                    }
                }

                //Get resolve mode
                ResolveMode _resolve_mode = (ResolveMode)d.GetValue(ResolveModeProperty);

                //Get Control Container
                var _container = d.GetValue(ControlContainerProperty) as IControlContainer;

                if (_container == null)
                {
                    _container = ContainerStore.Singleton.Controls;
                }

                //Get control
                UserControl _targetControl = null;

                if (_key != null)
                {
                    _targetControl = _container.GenerateView(_key, mode: _resolve_mode);
                }

            ((ContentControl)d).Content = _targetControl;
            }
            catch (Exception)
            {
                //do nothing as of now.
            }
        }
        #endregion
    }
}

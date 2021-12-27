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
        private static bool holdProcessing = false;

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
            DependencyProperty.RegisterAttached("Key", typeof(object), typeof(ControlRetrieverAP), new PropertyMetadata(null, CommonPropertyChanged));

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

        public static readonly DependencyProperty ControlContainerProperty =
            DependencyProperty.RegisterAttached("ControlContainer", typeof(IControlContainer), typeof(ControlRetrieverAP), new PropertyMetadata(null, CommonPropertyChanged));

        #endregion

        #region ViewModel

        public static IHaleyVM GetDataContext(DependencyObject obj)
        {
            return (IHaleyVM)obj.GetValue(DataContextProperty);
        }

        public static void SetDataContext(DependencyObject obj, IHaleyVM value)
        {
            obj.SetValue(DataContextProperty, value);
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.RegisterAttached("DataContext", typeof(IHaleyVM), typeof(ControlRetrieverAP), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CommonPropertyChanged));

        #endregion

        #region Private Methods

        static void CommonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (holdProcessing) return;
            RetrieveView(d, null);
        }

        static void RetrieveView(DependencyObject d, object e)
        {
            try
            {
                if (e == null)
                {
                    e = d.GetValue(KeyProperty) as object;
                }

                string _key = string.Empty;
                //new value of e could be string or enum.
                if (e is string)
                {
                    _key = e as string;
                }
                else if (e is Enum)
                {
                    var _enum = e as Enum;
                    if (_enum != null)
                    {
                        _key = _enum.getKey();
                    }
                }

                var _dcontext = d.GetValue(DataContextProperty) as IHaleyVM;
                RetrieveView(d, _key, _dcontext);
            }
            catch (Exception)
            {
                //do nothing as of now.
            }
        }

        static void RetrieveView(DependencyObject d, string key, IHaleyVM datacontext)
        {
            try
            {
                if (!(d is ContentControl)) return;

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

                if (key != null)
                {
                    _targetControl = _container.GenerateView(key,datacontext, mode: _resolve_mode);
                }

                //Sometimes the datacontext can be empty.
                if (datacontext == null)
                {
                    //This is for two way binding.
                    holdProcessing = true;
                    d.SetValue(DataContextProperty, _targetControl.DataContext);
                }

                holdProcessing = false;

                ((ContentControl)d).Content = _targetControl;

            }
            catch (Exception)
            {
                //do nothing as of now.
            }
            finally
            {
                holdProcessing = false;
            }
        }
        
        #endregion
    }
}
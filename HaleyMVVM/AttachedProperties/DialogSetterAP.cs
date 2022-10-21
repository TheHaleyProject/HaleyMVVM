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
using System.Windows.Input;
using Haley.MVVM;
using Haley.Enums;
using System.Text.RegularExpressions;
using Microsoft.Xaml.Behaviors;

namespace Haley.Models
{
    /// <summary>
    /// To set dialog for an usercontrol
    /// </summary>
    public static class DialogSetterAP
    {
        private static IDialogService ds;
        public static IServiceProvider GetServiceProvider(DependencyObject obj)
        {
            return (IServiceProvider)obj.GetValue(ServiceProviderProperty);
        }
        public static void SetServiceProvider(DependencyObject obj, IServiceProvider value)
        {
            obj.SetValue(ServiceProviderProperty, value);
        }

        public static readonly DependencyProperty ServiceProviderProperty =
            DependencyProperty.RegisterAttached("ServiceProvider", typeof(IServiceProvider), typeof(DialogSetterAP), new PropertyMetadata(null));

        public static bool GetBlurWindows(DependencyObject obj)
        {
            return (bool)obj.GetValue(BlurWindowsProperty);
        }
        public static void SetBlurWindows(DependencyObject obj, bool value)
        {
            obj.SetValue(BlurWindowsProperty, value);
        }

        public static readonly DependencyProperty BlurWindowsProperty =
            DependencyProperty.RegisterAttached("BlurWindows", typeof(bool), typeof(DialogSetterAP), new PropertyMetadata(false));

        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string), typeof(DialogSetterAP), new PropertyMetadata("Dialog"));

        public static DataTemplate GetContent(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(ContentProperty);
        }

        public static void SetContent(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.RegisterAttached("Content", typeof(DataTemplate), typeof(DialogSetterAP), new FrameworkPropertyMetadata(null,propertyChangedCallback: OnAnyPropertyChanged));

        public static string GetContainerKey(DependencyObject obj)
        {
            return (string)obj.GetValue(ContainerKeyProperty);
        }

        public static void SetContainerKey(DependencyObject obj, string value)
        {
            obj.SetValue(ContainerKeyProperty, value);
        }

        public static readonly DependencyProperty ContainerKeyProperty =
            DependencyProperty.RegisterAttached("ContainerKey", typeof(string), typeof(DialogSetterAP), new FrameworkPropertyMetadata(null, propertyChangedCallback: OnAnyPropertyChanged));

        private static void OnAnyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //here subcribe to the events 
            if (d is UIElement uie)
            {
                uie.MouseLeftButtonDown -= ControlMouseDownEvent; //Remove old subscriptions.
                uie.MouseLeftButtonDown += ControlMouseDownEvent;
            }
        }

        private static void ControlMouseDownEvent(object sender, MouseButtonEventArgs e)
        {
            //The content could be a user control or datatemplate
            if(sender is UIElement uie)
            {
                var _content = GetContent(uie);
                var _containerKey = GetContainerKey(uie);
                //preference to content.
                var _title = GetTitle(uie) ?? "Dialog Window";
                var _blur = GetBlurWindows(uie);
                if (_content == null && _containerKey == null) return;

                //use this datatemplate or usercontrol and display on a notification dialog
                var _ds = getDS(sender as DependencyObject);
                if (_ds == null) return;
                if (_ds is IDialogServiceEx _dsEx)
                {
                    if (_content != null)
                    {
                        _dsEx.ShowCustomView(_title, templateOrControl: _content, blurOtherWindows: _blur);
                    }
                    else
                    {
                        _dsEx.ShowContainerView(_title, _containerKey, blurOtherWindows: _blur);
                    }
                }
                e.Handled = true; // Don't send the event beyond this.
            }
        }

        private static IDialogService getDS(DependencyObject obj)
        {
            if (obj != null)
            {
                //See if the user has provided a dialogservice 
                var _provider = GetServiceProvider(obj);
                if (_provider != null)
                {
                    var _userDs = _provider.GetService(typeof(IDialogService)) as IDialogService;
                    if (_userDs != null) return _userDs;
                }
            }

            if (ds == null)
            {
                ds = ContainerStore.DI.Resolve<IDialogService>();
            }
            return ds;
        }
    }
}

using System.Windows;
using Haley.Enums;
using Haley.Utils;
using System.Windows.Data;
using Haley.Services;
using System;
using System.Reflection;

namespace Haley.Models
{
    public static class ThemeAP
    {
        private static object GetActiveTheme(DependencyObject obj)
        {
            return (object)obj.GetValue(ActiveThemeProperty);
        }

        public static readonly DependencyProperty ActiveThemeProperty =
            DependencyProperty.RegisterAttached("ActiveTheme", typeof(object), typeof(ThemeAP), new PropertyMetadata(null));

        private static EventHandler<object> GetChangeHandler(DependencyObject obj)
        {
            return (EventHandler<object>)obj.GetValue(ChangeHandlerProperty);
        }

        private static readonly DependencyProperty ChangeHandlerProperty =
            DependencyProperty.RegisterAttached("ChangeHandler", typeof(EventHandler<object>), typeof(ThemeAP), new PropertyMetadata(null));

        public static bool GetMonitorChange(DependencyObject obj)
        {
            return (bool)obj.GetValue(MonitorChangeProperty);
        }

        public static void SetMonitorChange(DependencyObject obj, bool value)
        {
            obj.SetValue(MonitorChangeProperty, value);
        }

        public static readonly DependencyProperty MonitorChangeProperty =
            DependencyProperty.RegisterAttached("MonitorChange", typeof(bool), typeof(ThemeAP), new PropertyMetadata(false,MonitorChangePropertyChanged));
        static void MonitorChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null) return; 
            var _themeChangeHandler = GetChangeHandler(d);
            var calling_assembly = d.GetType().Assembly;
            if (_themeChangeHandler == null)
            {
                EventHandler<object> handler = (sender, themedata) => { ThemeChangeHandler(sender, themedata, d, calling_assembly); };
                d.SetCurrentValue(ChangeHandlerProperty, handler);
                _themeChangeHandler = handler;
            }

            ThemeService.Singleton.ThemeChanged -= _themeChangeHandler;

            if (GetMonitorChange(d))
            {
                ThemeService.Singleton.ThemeChanged += _themeChangeHandler;
            }

            //Handle late join
            if (d is FrameworkElement fe)
            {
                fe.Loaded += Fe_Loaded;
                fe.Unloaded += Fe_Unloaded;
            }
        }

        private static void Fe_Unloaded(object sender, RoutedEventArgs e)
        {
            //Unregister
            if (!(sender is DependencyObject d && sender is FrameworkElement fe)) return;
            fe.Unloaded -= Fe_Unloaded;
            var _themeChangeHandler = GetChangeHandler(d);
            ThemeService.Singleton.ThemeChanged -= _themeChangeHandler;
        }

        private static void Fe_Loaded(object sender, EventArgs e)
        {
            if (!(sender is DependencyObject d && sender is FrameworkElement fe)) return;
            
            fe.Loaded -= Fe_Loaded;
            //if the startuptheme and active theme are not same, then we have a change (even though there is no trigger).
            var centralStartupTheme = ThemeService.Singleton.StartupTheme;
            var centeralActiveTheme = ThemeService.Singleton.ActiveTheme;
            var internalActiveTheme = GetActiveTheme(d);

            //If internal active theme is null, then it means, we have not yet initiated. (like first start)
            //Set it to match startuptheme or ActiveTheme.
            if (internalActiveTheme == null)
            {
                var calling_assembly = d.GetType().Assembly;
                //Late join
                ThemeService.Singleton.ChangeTheme(centeralActiveTheme??centralStartupTheme, d, calling_assembly, ThemeSearchMode.FrameworkElement);
            }
        }

        private static void ThemeChangeHandler(object sender, object e, DependencyObject d,Assembly asmbly)
        {
            //Sender will be themeservice. (who raises the event).
            if (e == null) return; //On first setup, previous theme path will be null. so, we don't have to worry about startup settings.
            var internalActiveTheme = GetActiveTheme(d);
            if (internalActiveTheme == e) return; //If incoming theme and my local theme are same, no point to try to change again.
            if (!GetMonitorChange(d)) return; //We are not monitoring
            if (ThemeService.Singleton.ChangeTheme(e, d, asmbly, ThemeSearchMode.FrameworkElement))
            {
                //Successfully changed.
                d.SetCurrentValue(ActiveThemeProperty, e);
            }
        }
    }
}

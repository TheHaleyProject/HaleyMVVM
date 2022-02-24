using System.Windows;
using Haley.Enums;
using Haley.Utils;
using System.Windows.Data;
using Haley.Services;
using System;

namespace Haley.Models
{
    public static class ThemeAP
    {
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
            if (_themeChangeHandler == null)
            {
                EventHandler<object> handler = (sender, theme) => { ThemeChangeHandler(sender, theme, d); };
                d.SetCurrentValue(ChangeHandlerProperty, handler);
                _themeChangeHandler = handler;
            }

            ThemeService.Singleton.ThemeChanged -= _themeChangeHandler;

            if (GetMonitorChange(d))
            {
                ThemeService.Singleton.ThemeChanged += _themeChangeHandler;
            }
        }

        private static void ThemeChangeHandler(object sender, object e,DependencyObject d)
        {
            object theme = e;
            if (e == null) return; //On first setup, previous theme path will be null. so, we don't have to worry about startup settings.
            if (!GetMonitorChange(d)) return; //We are not monitoring
            var _priority = SearchPriority.FrameworkElement; //We will replace the framework element theme.
            ThemeService.Singleton.ChangeTheme(d, e, _priority, false, false);
        }
    }
}

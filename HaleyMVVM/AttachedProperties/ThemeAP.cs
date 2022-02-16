using System.Windows;
using Haley.Enums;
using Haley.Utils;
using System.Windows.Data;
using System;

namespace Haley.Models
{
    public static class ThemeAP
    {
        private static EventHandler<Theme> GetChangeHandler(DependencyObject obj)
        {
            return (EventHandler<Theme>)obj.GetValue(ChangeHandlerProperty);
        }

        private static readonly DependencyProperty ChangeHandlerProperty =
            DependencyProperty.RegisterAttached("ChangeHandler", typeof(EventHandler<Theme>), typeof(ThemeAP), new PropertyMetadata(null));

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
            if (d == null) return; //Theme hasn't changed.
            var _themeChangeHandler = GetChangeHandler(d);
            if (_themeChangeHandler == null)
            {
                EventHandler<Theme> handler = (sender, theme) => { ThemeChangeHandler(sender, theme, d); };
                d.SetCurrentValue(ChangeHandlerProperty, handler);
                _themeChangeHandler = handler;
            }
            ThemeLoader.Singleton.ActiveThemeChanged -= _themeChangeHandler;

            if (GetMonitorChange(d))
            {
                ThemeLoader.Singleton.ActiveThemeChanged += _themeChangeHandler;
            }
        }

        private static void ThemeChangeHandler(object sender, Theme e,DependencyObject d)
        {
            Theme active = e;
            if (active == null || active.Path == null || active.PreviousThemePath == null) return; //On first setup, previous theme path will be null. so, we don't have to worry about startup settings.
            if (!GetMonitorChange(d)) return; //We are not monitoring
            var _priority = SearchPriority.FrameworkElement; //We will replace the framework element theme.
            ThemeLoader.Singleton.ChangeTheme(d, active, _priority, false, false);
        }
    }
}

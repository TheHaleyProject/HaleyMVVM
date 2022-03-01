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
        private static EventHandler<(object newTheme, object oldTheme)> GetChangeHandler(DependencyObject obj)
        {
            return (EventHandler<(object newTheme, object oldTheme)>)obj.GetValue(ChangeHandlerProperty);
        }

        private static readonly DependencyProperty ChangeHandlerProperty =
            DependencyProperty.RegisterAttached("ChangeHandler", typeof(EventHandler<(object newTheme, object oldTheme)>), typeof(ThemeAP), new PropertyMetadata(null));

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
            var _dassembly = d.GetType().Assembly;
            var _cassembly = Assembly.GetCallingAssembly();
            if (_themeChangeHandler == null)
            {
                EventHandler<(object newTheme, object oldTheme)> handler = (sender, themeTuple) => { ThemeChangeHandler(sender, themeTuple, d,_cassembly); };
                d.SetCurrentValue(ChangeHandlerProperty, handler);
                _themeChangeHandler = handler;
            }

            ThemeService.Singleton.ThemeChanged -= _themeChangeHandler;

            if (GetMonitorChange(d))
            {
                ThemeService.Singleton.ThemeChanged += _themeChangeHandler;
            }

            //Handle late join
            //if the startuptheme and active theme are not same, then we have a change (even though there is no trigger).
            var _startupTheme = ThemeService.Singleton.StartupTheme;
            var _activeTheme = ThemeService.Singleton.ActiveTheme;
            if (_startupTheme != null && _activeTheme != null && (_startupTheme != _activeTheme))
            {
                //Themes has changed.
                ThemeService.Singleton.ChangeTheme(_startupTheme, _activeTheme, d, _cassembly, ThemeSearchMode.FrameworkElement, false, false);
            }
        }

        private static void ThemeChangeHandler(object sender, (object newTheme, object oldTheme) e, DependencyObject d,Assembly asmbly)
        {
            //Sender will be themeservice. (who raises the event).
            if (e.newTheme == null || e.oldTheme == null) return; //On first setup, previous theme path will be null. so, we don't have to worry about startup settings.
            if (!GetMonitorChange(d)) return; //We are not monitoring
            ThemeService.Singleton.ChangeTheme(e.newTheme, e.oldTheme, d, asmbly, ThemeSearchMode.FrameworkElement, false, false);
        }
    }
}

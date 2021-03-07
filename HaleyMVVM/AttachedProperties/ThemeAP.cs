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
    public static class ThemeAP
    {
        public static Theme GetOldTheme(DependencyObject obj)
        {
            return (Theme)obj.GetValue(OldThemeProperty);
        }

        public static void SetOldTheme(DependencyObject obj, Theme value)
        {
            obj.SetValue(OldThemeProperty, value);
        }

        // Using a DependencyProperty as the backing store for OldTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OldThemeProperty =
            DependencyProperty.RegisterAttached("OldTheme", typeof(Theme), typeof(ThemeAP), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static Theme GetActiveTheme(DependencyObject obj)
        {
            return (Theme)obj.GetValue(ActiveThemeProperty);
        }

        public static void SetActiveTheme(DependencyObject obj, Theme value)
        {
            obj.SetValue(ActiveThemeProperty, value);
        }

        // Using a DependencyProperty as the backing store for ActiveTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveThemeProperty =
            DependencyProperty.RegisterAttached("ActiveTheme", typeof(Theme), typeof(ThemeAP), new PropertyMetadata(null,ActiveThemePropertyChanged));

        private static void ActiveThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null || e.NewValue == null) return;
            Theme active = e.NewValue as Theme;
            if (active == null || active.theme_PackURI == null || active.theme_to_replace == null || active.base_dictionary_name == null) return;

            //If Old theme is not null and if old theme packURI and current theme PackURI matches, then don't do anything.
            var oldtheme = GetOldTheme(d);
            if (oldtheme != null)
            {
                if (oldtheme.theme_PackURI == active.theme_PackURI) return;
            }
           
            SetOldTheme(d, active);

            ThemeLoader.changeTheme(d,active);
        }
    }
}

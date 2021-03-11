using System.Windows;
using Haley.Enums;
using Haley.Utils;
using System.Windows.Data;

namespace Haley.Models
{
    public static class ThemeAP
    {
        public static SearchPriority GetPriority(DependencyObject obj)
        {
            return (SearchPriority)obj.GetValue(PriorityProperty);
        }

        public static void SetPriority(DependencyObject obj, SearchPriority value)
        {
            obj.SetValue(PriorityProperty, value);
        }

        // Using a DependencyProperty as the backing store for Priority.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PriorityProperty =
            DependencyProperty.RegisterAttached("Priority", typeof(SearchPriority), typeof(ThemeAP), new PropertyMetadata(SearchPriority.FrameworkElement));

        //If users wants to set the active theme from control side 
        public static Theme GetNewTheme(DependencyObject obj)
        {
            return (Theme)obj.GetValue(NewThemeProperty);
        }

        public static void SetNewTheme(DependencyObject obj, Theme value)
        {
            obj.SetValue(NewThemeProperty, value);
        }

        // Using a DependencyProperty as the backing store for NewTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewThemeProperty =
            DependencyProperty.RegisterAttached("NewTheme", typeof(Theme), typeof(ThemeAP), new PropertyMetadata(null,NewThemePropertyChanged));

        private static void NewThemePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null || e.NewValue == null) return;
            Theme active = e.NewValue as Theme;
            if (active == null || active.new_theme_uri == null || active.old_theme_uri == null) return;

            var _priority = GetPriority(d);
            var _compareWithActiveTheme = _priority == SearchPriority.Application; //Only in case of application level resources, we need to compare with old theme because it is changed for all. Else, we do not compare
           
            ThemeLoader.Singleton.changeTheme(d,active,_priority, _compareWithActiveTheme,false);
        }

        public static bool GetTriggerChange(DependencyObject obj)
        {
            return (bool)obj.GetValue(TriggerChangeProperty);
        }

        public static void SetTriggerChange(DependencyObject obj, bool value)
        {
            obj.SetValue(TriggerChangeProperty, value);
        }

        // Using a DependencyProperty as the backing store for TriggerChange.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerChangeProperty =
            DependencyProperty.RegisterAttached("TriggerChange", typeof(bool), typeof(ThemeAP), new PropertyMetadata(false,TriggerChangePropertyChanged));
        static void TriggerChangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d == null || !((bool) e.NewValue)) return; //Theme hasn't changed.

            //d.SetCurrentValue(NewThemeProperty, ThemeLoader.Singleton.active_theme);
            //Bind with themeloader's active theme
            var binding = new Binding
            {
                Path = new PropertyPath(ThemeLoader.active_themeProperty),
                Source = ThemeLoader.Singleton
            };

            BindingOperations.SetBinding(d, NewThemeProperty, binding);
        }
    }
}

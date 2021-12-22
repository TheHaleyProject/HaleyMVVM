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
    /// To set specific collections to elements via Style setters.
    /// </summary>
    public static class StyleSetterAP
    {

        #region Input Bindings
        public static InputBindingCollection GetInputBindings(DependencyObject obj)
        {
            return (InputBindingCollection)obj.GetValue(InputBindingsProperty);
        }

        public static void SetInputBindings(DependencyObject obj, InputBindingCollection value)
        {
            obj.SetValue(InputBindingsProperty, value);
        }

        // Using a DependencyProperty as the backing store for InputBindings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputBindingsProperty =
            DependencyProperty.RegisterAttached("InputBindings", typeof(InputBindingCollection), typeof(StyleSetterAP), new FrameworkPropertyMetadata(new InputBindingCollection(), propertyChangedCallback: InputBindinsPropertyChanged));
        static void InputBindinsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.InputBindings.Clear(); //Should we clear or just add?
                element.InputBindings.AddRange((InputBindingCollection)e.NewValue);
            }
        }
        #endregion

        #region Behaviours
        public static Collection<Behavior> GetBehaviours(DependencyObject obj)
        {
            return (Collection<Behavior>)obj.GetValue(BehavioursProperty);
        }

        public static void SetBehaviours(DependencyObject obj, Collection<Behavior> value)
        {
            obj.SetValue(BehavioursProperty, value);
        }

        // Using a DependencyProperty as the backing store for Behaviours.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BehavioursProperty =
            DependencyProperty.RegisterAttached("Behaviours", typeof(Collection<Behavior>), typeof(StyleSetterAP), new FrameworkPropertyMetadata(new Collection<Behavior>(), propertyChangedCallback: OnBehaviorsChanged));
        static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && e.NewValue is Collection<Behavior> behCollect)
            {
                foreach (var bhvr in behCollect)
                {
                    Interaction.GetBehaviors(element).Add(bhvr);
                }
            }
        }

        #endregion
    }
}

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
using beh = Microsoft.Xaml.Behaviors;

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
        public static BehaviourCollection GetBehaviours(DependencyObject obj)
        {
            return (BehaviourCollection)obj.GetValue(BehavioursProperty);
        }

        public static void SetBehaviours(DependencyObject obj, BehaviourCollection value)
        {
            obj.SetValue(BehavioursProperty, value);
        }

        public static readonly DependencyProperty BehavioursProperty =
            DependencyProperty.RegisterAttached("Behaviours", typeof(BehaviourCollection), typeof(StyleSetterAP), new FrameworkPropertyMetadata(default(BehaviourCollection), propertyChangedCallback: OnBehaviorsChanged));
        static void OnBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try {
                if (d is UIElement element && e.NewValue is BehaviourCollection behCollect) {
                    foreach (var bhvr in behCollect) {
                        Interaction.GetBehaviors(element).Add(bhvr);
                    }
                }
            } catch (Exception) {

            }
        }

        #endregion

        #region Triggers

        public static TriggerCollection GetTriggers(DependencyObject obj) {
            return (TriggerCollection)obj.GetValue(TriggersProperty);
        }

        public static void SetTriggers(DependencyObject obj, TriggerCollection value) {
            obj.SetValue(TriggersProperty, value);
        }

        // Using a DependencyProperty as the backing store for Triggers.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.RegisterAttached("Triggers", typeof(TriggerCollection), typeof(StyleSetterAP), new PropertyMetadata(default(TriggerCollection), propertyChangedCallback: OnTriggersChanged));

        private static void OnTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is UIElement element && e.NewValue is TriggerCollection trigColl) {
                foreach (var trgr in trigColl) {
                    Interaction.GetTriggers(element).Add(trgr);
                }
            }
        }
        #endregion
    }
}

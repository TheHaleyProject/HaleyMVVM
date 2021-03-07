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

namespace Haley.Models
{
    /// <summary>
    /// To restrict the entry to only allow numbers
    /// </summary>
    public static class InputAP
    {
        private static readonly Regex _integerRegex = new Regex(@"^\d+$"); //Numers can happen one or more times
        private static readonly Regex _doubleRegex = new Regex(@"^(-?)((0|\d+)[.]?(\d+)?)?$"); // (\d) is same as ([0-9]). " *" indicates that it happens zero or more time.  [.]? indicates that "." can happen zero or one time. ^ should match starting. $ should also match ending
        private static readonly Regex _textRegex = new Regex(@"^[a-zA-Z]+$"); //Only text characters are allowed. Not even numbers are allowed.

        public static InputConstraintType GetConstraint(DependencyObject obj)
        { return (InputConstraintType)obj.GetValue(ConstraintProperty); }

        public static void SetConstraint(DependencyObject obj, InputConstraintType value)
        { obj.SetValue(ConstraintProperty, value); }

        // Using a DependencyProperty as the backing store for Constraint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConstraintProperty =
            DependencyProperty.RegisterAttached(
            "Constraint",
            typeof(InputConstraintType),
            typeof(InputAP),
            new FrameworkPropertyMetadata(InputConstraintType.All, ConstraintPropertyChanged));

        private static void ConstraintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBox)
            {
             //Input events.
                //Unsubscribe previous ones 
                ((TextBox)d).PreviewTextInput -= InputAP_PreviewInput;
                //Subscribe again
                ((TextBox)d).PreviewTextInput += InputAP_PreviewInput;
            }
        }

        static void InputAP_PreviewInput(object sender, TextCompositionEventArgs e)
        {
            //Reason for inverse handling is that, if the requirement doesn't match the regex, then we handle it right here and doesn't allow it to bubble up.
            //If we test only the current value, then we will get irrelvant results. We need to check it against the whole text already present to match the regex.
            DependencyObject d = sender as DependencyObject;
            TextBox tbx = d as TextBox;
            if (tbx != null)
            {
                var existing = tbx.Text;
                existing += e.Text;
                e.Handled = !HandleEvent(d, existing);
            }
            
        }
     
        private static bool HandleEvent(DependencyObject sender, string text)
        {
            //Process it accordingly to ensure that the text is only numeric.
            var constraintType = GetConstraint(sender);
            switch(constraintType)
            {
                case InputConstraintType.Double:
                    return CheckRegexStatus(_doubleRegex, text);
                case InputConstraintType.Integer:
                    return CheckRegexStatus(_integerRegex, text);
                case InputConstraintType.TextOnly:
                    return CheckRegexStatus(_textRegex, text);
            }
            return false;
        }

        private static bool CheckRegexStatus(Regex regex, string text) { return regex.IsMatch(text); }
    }
}

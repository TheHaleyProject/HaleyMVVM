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
    /// Limit the maximum and minimum values for a textbox.
    /// </summary>
    public class NumericLimiter : Behavior<TextBox>
    {
        private bool _ischanging = false;
        public object MaxValue
        {
            get { return (object)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(object), typeof(NumericLimiter), new PropertyMetadata(null));
        public object MinValue
        {
            get { return (object)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(object), typeof(NumericLimiter), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            //We need not validate on Preview (text input). We need to validate after it is entered. Because, we will modify the text value after the content is entered.
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
        }
        void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (_ischanging) return;
            //Check if either max or min value is set. if yes, then check if the provided value is with in the limits. Always set the values with in the max/min range.

            if (!(sender is TextBox tbx)) return; //If not a text box, return.

            //Since this is a numeric limiter, the input should also be in numeric format. Try parsing it as double. if failed, do not proceed.

            if (!double.TryParse(tbx.Text, out var _inputdbl)) return;

            if (MaxValue != null)
            {
                //We have a max value. Limit the input.
                //A single provided input cannot cross both max and min limit at same time. So, once this is set, return.
                if (double.TryParse((string)MaxValue,out var _maxdbl)) //if successfully parsed.
                {
                    if (_inputdbl > _maxdbl )
                    {
                        _ischanging = true; //So that the new change is not triggered and we donot end up in a loop. (may be we won't end up in loop, but the call is made twice).
                        //Input is greater than the max set limit.
                        tbx.SetCurrentValue(TextBox.TextProperty, _maxdbl.ToString()); //The text box text coulde be binded, so use set current value.
                        _ischanging = false;
                        return;
                    }
                }
            }

            if (MinValue != null)
            {
                //We have a minimum value, limit the input.

                if (double.TryParse((string)MinValue, out var _mindbl)) //if successfully parsed.
                {
                    if (_inputdbl < _mindbl)
                    {
                        _ischanging = true; //So that the new change is not triggered and we donot end up in a loop. (may be we won't end up in loop, but the call is made twice).
                        //Input is greater than the max set limit.
                        tbx.SetCurrentValue(TextBox.TextProperty, _mindbl.ToString()); //The text box text coulde be binded, so use set current value.
                        _ischanging = false;
                        return;
                    }
                }
            }
        }
    }
}

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
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Haley.Models
{
    //reference: https://stackoverflow.com/questions/28040646/transparent-blurred-background-to-canvas
    //reference: https://gist.github.com/walterlv/752669f389978440d344941a5fcd5b00
    //reference  https://github.com/joelspadin/AudioPipe/blob/master/AudioPipe/Extensions/WindowAccentExtensions.cs

    /// <summary>
    /// To set background blur in windows.
    /// </summary>
    public static class WindowBlurAP
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);


        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(WindowBlurAP), new PropertyMetadata(false,OnIsEnabledPropertyChanged));

        private static void OnIsEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ( d is Window wndow)
            {
                if (e.OldValue.Equals(true))
                {
                    //If old value was true, it means that we already have some blur or atleast we have subscribed to the sourceinitialized event. Unsubscribe it.
                    wndow.SourceInitialized -= OnSourceInitialized;
                }

                //We have been requested to add blur. Add it.
                var source = (HwndSource)PresentationSource.FromVisual(wndow);
                if (source == null)
                {
                    wndow.SourceInitialized += OnSourceInitialized;
                    //This event is raised to support interoperation with Win32.
                    //If the source is null, then it means it is not loaded yet. So, wait until it is initialized and then enable the blur.
                }
                else
                {
                    SetBackground(wndow);
                }
            }
        }


        private static void OnSourceInitialized(object sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.SourceInitialized -= OnSourceInitialized; //Unsubscribe
                SetBackground(window);
            }
        }

        private static void SetBackground(Window wndw)
        {
            var _acntstate = AccentState.ACCENT_DISABLED;
            if (GetIsEnabled(wndw))
            {
                //Blue is enabled.
                _acntstate = AccentState.ACCENT_ENABLE_BLURBEHIND;
            }
            SetAccentPolicy(wndw, _acntstate);
        }

        private static void SetAccentPolicy(Window window, AccentState accentState)
        {
            var windowHelper = new WindowInteropHelper(window);

            var accent = new AccentPolicy
            {
                AccentState = accentState,
                AccentFlags = AccentFlags.DrawAllBorders,
                
            };

            var structSize = Marshal.SizeOf(accent);
            var structPtr = Marshal.AllocHGlobal(structSize);

            try
            {
                Marshal.StructureToPtr(accent, structPtr, false);

                var data = new WindowCompositionAttributeData()
                {
                    Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                    SizeOfData = structSize,
                    Data = structPtr
                };

                SetWindowCompositionAttribute(windowHelper.Handle, ref data);
            }
            finally
            {
                Marshal.FreeHGlobal(structPtr);
            }
        }
    }
}

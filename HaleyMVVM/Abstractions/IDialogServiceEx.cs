using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Enums;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;

namespace Haley.Abstractions
{
    public interface IDialogServiceEx : IDialogService
    {
        SolidColorBrush AccentColor { get; set; }
        SolidColorBrush AccentForeground { get; set; }
        Brush ToastBackground { get; set; }
        Brush Foreground { get; set; }
        SolidColorBrush ToastForeground { get; set; }
        Brush Background { get; set; }
        Brush ContentBackground { get; set; }
        Color? GlowColor { get; set; }
        WindowStartupLocation StartupLocation { get; set; }

        INotification ShowCustomView(string title, object templateOrControl = null, bool blurOtherWindows = false);
    }
}

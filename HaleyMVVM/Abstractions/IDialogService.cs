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
    public interface IDialogService
    {
        SolidColorBrush AccentColor { get; set; }
        SolidColorBrush AccentForeground { get; set; }
        Brush ToastBackground { get; set; }
        Brush Foreground { get; set; }
        SolidColorBrush ToastForeground { get; set; }
        bool EnableBackgroundBlur { get; set; }
        Brush Background { get; set; }
        Brush ContentBackground { get; set; }
        Color? GlowColor { get; set; }
        double GlowRadius { get; set; }
        bool TopMost { get; set; }
        bool ShowInTaskBar { get; set; }
        WindowStartupLocation StartupLocation { get; set; }

        INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false,bool blurOtherWindows = false);
        INotification Info(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false);
        INotification Warning(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false);
        INotification Error(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false);
        INotification Success(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false);
        bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7);

        //For fetching views from Container
        INotification ShowContainerView(string title, string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered,bool blurOtherWindows = false, IControlContainer container = null);
        INotification ShowContainerView(string title, Enum key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null);
        INotification ShowContainerView<ViewType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null) where ViewType : UserControl;
        INotification ShowContainerView<VMType>(string title, VMType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null) where VMType : class, IHaleyVM;

        INotification ShowCustomView(string title, DataTemplate template = null, bool blurOtherWindows = false);
    }
}

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
       /// <summary>
       /// To Enable user to change color as and when required.
       /// </summary>
       /// <param name="AccentColor"></param>
       /// <param name="AccentForeground"></param>
       /// <param name="ToastBackground"></param>
       /// <param name="ToastForeground"></param>
        void ChangeAccentColors(SolidColorBrush AccentColor = null, SolidColorBrush AccentForeground = null, Brush ToastBackground = null, SolidColorBrush ToastForeground = null);
        void SetGlow(Color? glowColor, double glowRadius = 3);
        void ChangeSettings(bool? topMost = null, bool? showInTaskBar = null, DialogStartupLocation startupLocation = DialogStartupLocation.CenterParent);
        INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false,bool blurWindows = false);

        INotification Info(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false);
        INotification Warning(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false);
        INotification Error(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false);
        INotification Success(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false);
        bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7);

        //For fetching views from Container
        INotification ShowContainerView(string title, string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered,bool blurWindows = false, IControlContainer container = null);
        INotification ShowContainerView(string title, Enum key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false, IControlContainer container = null);
        INotification ShowContainerView<ViewType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false, IControlContainer container = null) where ViewType : UserControl;
        INotification ShowContainerView<VMType>(string title, VMType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false, IControlContainer container = null) where VMType : class, IHaleyVM;

        INotification ShowCustomView(string title, DataTemplate template = null, bool blurWindows = false);
    }
}

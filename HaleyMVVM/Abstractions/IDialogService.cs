using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Enums;
using System.Windows.Media;

namespace Haley.Abstractions
{
    public interface IDialogService
    {
        [Obsolete("To be deprecated in future. Use DialogService.Show")]
        bool send(string title,string message, DialogMode mode = DialogMode.Notification);
        [Obsolete("To be deprecated in future. Use DialogService.Show")]
        bool receive(string title, string message, out string user_input);
        
       /// <summary>
       /// To Enable user to change color as and when required.
       /// </summary>
       /// <param name="AccentColor"></param>
       /// <param name="AccentForeground"></param>
       /// <param name="ToastBackground"></param>
       /// <param name="ToastForeground"></param>
        void ChangeAccentColors(SolidColorBrush AccentColor = null, SolidColorBrush AccentForeground = null, Brush ToastBackground = null, SolidColorBrush ToastForeground = null);
        INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false, bool topMost= true, bool showInTaskBar = false);
        bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7);

        //For fetching views from Container
        INotification ShowContainerView(string title, string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false);
        INotification ShowContainerView(string title, Enum key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false);
        INotification ShowContainerView<ViewType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false) where ViewType :class, IHaleyControl;
        INotification ShowContainerView<VMType>(string title, VMType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false) where VMType : class, IHaleyControlVM;
    }
}

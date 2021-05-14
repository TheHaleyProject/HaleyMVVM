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
    public interface INotification
    {
        #region Properties
        string Id { get;  }
        NotificationType Type { get; set; }
        string Message { get; set; }
        NotificationIcon NotificationIcon { get; set; }
        bool ShowNotificationIcon { get; set; }
        SolidColorBrush AccentColor { get; set; }
        SolidColorBrush AccentForeground { get; set; }
        SolidColorBrush ToastForeground { get; set; }
        Brush ToastBackground { get; set; }
        string UserInput { get; set; }
        bool? DialogResult { get; set; }
        string AppName { get; set; }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Haley.Utils {
    public class NotificationCommands {
        public readonly static RoutedUICommand OkayResult = new RoutedUICommand("To set the dialog result of the notification window to True", nameof(OkayResult), typeof(NotificationCommands));
        public readonly static RoutedUICommand CancelResult = new RoutedUICommand("To set the dialog result of the notification window to False", nameof(CancelResult), typeof(NotificationCommands));
        public readonly static RoutedUICommand CloseAllToasts = new RoutedUICommand("Close all the open toasts", nameof(CloseAllToasts), typeof(NotificationCommands));
        public readonly static RoutedUICommand DragMove = new RoutedUICommand("Drag and move", nameof(DragMove), typeof(NotificationCommands));
    }
}

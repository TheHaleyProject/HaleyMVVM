using Haley.Abstractions;
using Haley.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haley.Models {
    public class NotificationResult : INotification {
        public object ViewModel{ get; internal set; }
        public string Id { get; internal set; }
        public DisplayType Type { get; set; }
        public string Message { get; set; }
        public NotificationIcon NotificationIcon { get; set; }
        public bool ShowNotificationIcon { get; set; }
        public string UserInput { get; set; }
        public bool? DialogResult { get; set; }
        public string AppName { get; set; }
        public bool AutoClose { get; set; }
        public int CountDown { get; set; }
        public double GlowRadius { get; set; }

        public NotificationResult() { }
    }
}

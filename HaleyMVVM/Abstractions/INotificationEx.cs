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
    public interface INotificationEx : INotification
    {
        #region Properties
        SolidColorBrush AccentColor { get; set; }
        SolidColorBrush AccentForeground { get; set; }
        SolidColorBrush ToastForeground { get; set; }
        Brush ToastBackground { get; set; }
        Color GlowColor { get; set; }
        #endregion
    }
}

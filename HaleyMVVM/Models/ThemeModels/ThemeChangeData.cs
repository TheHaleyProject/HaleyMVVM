using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Haley.Enums;

namespace Haley.Models
{
    public struct ThemeChangeData
    {
        public ThemeInfoBase OldTheme { get; set; }
        public ThemeInfoBase NewTheme { get; set; }
        public bool RaiseNotifications { get; set; }
        public object Sender { get; set; }
        public ThemeSearchMode Priority { get; set; }
    }
}

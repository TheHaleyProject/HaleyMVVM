using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows;

namespace Haley.Models
{
    public class ThemeInfoWrapper
    {
        public ThemeInfo Theme { get; set; }
        public Assembly Source { get; set; }
        public ThemeTracker Tracker { get; set; }
        public bool IsTracked { get; set; }
        public ThemeInfoWrapper(ThemeInfo theme,Assembly assembly) 
        {
            Theme = theme;
            Source = assembly;
            IsTracked = false;
            Tracker = null;
        }
    }
}

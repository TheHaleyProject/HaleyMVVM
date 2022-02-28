using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows;

namespace Haley.Models
{
    public class ExternalThemeInfo :ThemeInfo
    {
        public Assembly Source { get; set; }
        public ThemeTracker Tracker { get; set; }
        public bool IsTracked { get; set; }
        public ExternalThemeInfo(string name, Uri path) :base(name,path)
        {
            IsTracked = false;
            Tracker = null;
        }
    }
}

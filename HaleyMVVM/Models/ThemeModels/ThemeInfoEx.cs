using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows;
using Haley.Enums;

namespace Haley.Models
{
    public class ThemeInfoEx :ThemeInfo
    {
        public Assembly SourceAssembly { get; set; }
        public ThemeTracker Tracker { get; set; }
        public ThemeDictionary StoredDB { get; set; }
        public bool IsTracked { get; set; }
        public ThemeInfoEx(string name, Uri path) :base(name,path)
        {
            IsTracked = false;
            Tracker = null;
        }
    }
}

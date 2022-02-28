using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows;
using Haley.Enums;
namespace Haley.Models
{
    public class InternalThemeInfo : ThemeInfo
    {
        public InternalThemeMode Mode { get; set; }
        public InternalThemeInfo(string name, Uri path) :base(name,path)
        {
            
        }
    }
}

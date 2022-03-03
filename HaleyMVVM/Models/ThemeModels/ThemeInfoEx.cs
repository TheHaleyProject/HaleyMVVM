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
        public string CrossReferenceId { get; set; }
        public Assembly SourceAssembly { get; set; }
        public ThemeInfoEx(Uri path,string groupId) :base(path,groupId)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Haley.Models
{
    public class Theme
    {
        public Uri Path { get; set; }
        public Uri PreviousThemePath { get; set; }
        public Uri BaseDictionaryPath { get; set; }
        public Theme(Uri path, Uri previous_path,Uri base_dictionary = null) 
        {
            Path = path; 
            PreviousThemePath = previous_path;
            BaseDictionaryPath = base_dictionary;
        }
    }
}

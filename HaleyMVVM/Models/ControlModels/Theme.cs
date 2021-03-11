using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Haley.Models
{
    public class Theme
    {
        public Uri new_theme_uri { get; set; }
        public Uri old_theme_uri { get; set; }
        public Uri base_dictionary_uri { get; set; }
        public Theme(Uri new_theme, Uri old_theme,Uri base_dictionary = null) 
        {
            new_theme_uri = new_theme; 
            old_theme_uri = old_theme;
            base_dictionary_uri = base_dictionary;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Haley.Models
{
    public class Theme
    {
        public string name { get; set; }
        public string theme_PackURI { get; set; }
        public string theme_to_replace { get; set; }
        public string base_dictionary_name { get; set; }
        public object sender { get; set; }
        public Theme() { }
    }
}

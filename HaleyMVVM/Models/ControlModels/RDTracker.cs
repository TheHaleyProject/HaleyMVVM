using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Haley.Models
{
    public class RDTracker
    {
        public string id { get; set; }
        public ResourceDictionary resource { get; set; }
        public RDTracker child { get; set; }
        public bool is_last { get; set; }
        public RDTracker(ResourceDictionary parent, RDTracker child, bool is_last)
        {
            id = Guid.NewGuid().ToString();
            this.resource = parent;
            this.child = child;
            this.is_last = is_last;
        }
    }
}

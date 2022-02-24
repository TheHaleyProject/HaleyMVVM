using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Haley.Models
{
    public class ThemeTracker
    {
        public string Id { get; set; }
        public ResourceDictionary Resource { get; set; }
        public ThemeTracker Child { get; set; }
        public bool IsTarget { get; set; }
        public ThemeTracker(ResourceDictionary resource, ThemeTracker child, bool is_target)
        {
            Id = Guid.NewGuid().ToString();
            this.Resource = resource;
            this.Child = child;
            this.IsTarget = is_target;
        }
    }
}

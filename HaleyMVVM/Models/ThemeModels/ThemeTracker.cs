using System;
using System.Collections.Generic;
using System.Windows;

namespace Haley.Models
{
    public class ThemeTracker
    {
        public string Id { get;}
        public ResourceDictionary RD { get; set; }
        public ThemeTracker Parent { get; set; }
        public ThemeTracker Child { get; set; }
        public bool IsTarget { get; set; }
        public bool IsRoot { get; set; }
        public ThemeTracker()
        {
            Id = Guid.NewGuid().ToString();
        }
        public ThemeTracker(ResourceDictionary rd, ThemeTracker child, bool isTarget):base()
        {
            this.RD = rd;
            this.Child = child;
            this.IsTarget = isTarget;
            IsRoot = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Globalization.Test
{
    public class DemoOptionsObject
    {
        [Display(Name =nameof(Properties.Resources.name))]
        public string Name { get; set; }
        [Display(Name = nameof(Properties.Resources.age))]
        public int Age { get; set; }
        public DemoOptionsObject(string name, int age) { Name = name; Age = age; }
    }
}

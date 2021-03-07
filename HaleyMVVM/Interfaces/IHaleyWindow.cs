using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Haley.Abstractions
{
    public interface IHaleyWindow
    {
        bool? DialogResult { get; set; }
        Object DataContext { get; set; }
        void Close();
        bool? ShowDialog();
        void Show();
    }
}

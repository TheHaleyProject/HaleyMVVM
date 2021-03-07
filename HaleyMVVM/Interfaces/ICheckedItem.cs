using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Haley.Abstractions
{
    public interface ICheckedItem : INotifyPropertyChanged
    {
        bool is_checked { get; set; }
        string name { get; set; }
        int id { get; set; }
    }

}

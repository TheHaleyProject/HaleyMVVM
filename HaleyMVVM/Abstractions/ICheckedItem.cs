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
        bool IsChecked { get; set; }
        string Name { get; set; }
        string Id { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Enums;
using System.Windows.Controls;

namespace Haley.Abstractions
{
    public interface IControlContainer : IUIContainerBase<IHaleyVM, UserControl> 
    {
        new string Register<VMType, ViewType>(VMType InputViewModel = null, bool use_vm_as_key = false, RegisterMode mode= RegisterMode.Singleton)
           where VMType : class, IHaleyVM
           where ViewType : UserControl;
    }
}

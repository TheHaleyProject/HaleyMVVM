using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Events;
using System.Windows.Controls;
using System.Windows;

namespace Haley.Abstractions
{
    public interface IContainerFactory
    {
        string Id { get; }
        IBaseContainer DI { get; set; }
        IControlContainer Controls { get; }
        IWindowContainer Windows { get; }
    }
}

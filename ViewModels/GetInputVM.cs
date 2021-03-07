using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Haley.Models;
using Haley.Abstractions;
using Haley.Events;

namespace Haley.WPF.ViewModels
{
    public class GetInputVM : DialogVMBase
    {
        public GetInputVM(string title, string message) : base(title, message)
        {

        }
    }
}

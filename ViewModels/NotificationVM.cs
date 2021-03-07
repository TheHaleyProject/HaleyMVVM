using System;
using System.Collections.Generic;
using System.Text;
using Haley.Abstractions;

namespace Haley.WPF.ViewModels
{
    public class NotificationVM : DialogVMBase
    {
        public NotificationVM(string title, string message) : base(title, message)
        {

        }
    }
}

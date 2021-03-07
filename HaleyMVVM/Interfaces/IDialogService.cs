using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Enums;

namespace Haley.Abstractions
{
    public interface IDialogService
    {
        bool send(string title,string message, DialogMode mode = DialogMode.Notification);
        bool receive(string title, string message, out string user_input);
    }
}

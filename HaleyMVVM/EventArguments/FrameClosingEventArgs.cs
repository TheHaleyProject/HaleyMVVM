using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Haley.Events
{
    public class FrameClosingEventArgs : EventArgs
    {
        public bool? event_result { get; set; } //Should carry if the window closed.
        public object message { get; set; }
        public FrameClosingEventArgs(bool DialogResult, object _message)
        {
            event_result = DialogResult;
            message = _message;
        }
    }
}

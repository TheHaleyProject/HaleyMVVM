using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace Haley.Events
{
    public class HotKeyArgs : EventArgs
    {
        public object AssociatedObject { get; set; }
        /// <summary>
        /// Gives the list of keys pressed in continuously without a gap of 1 second.
        /// </summary>
        public IEnumerable<Key> PressedKeys { get; set; }
        public KeyEventArgs SourceArg { get; set; }
        public HotKeyArgs()
        {
        }
    }
}

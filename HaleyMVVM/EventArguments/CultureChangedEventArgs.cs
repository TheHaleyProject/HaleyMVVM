using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;

namespace Haley.Events
{
    public class CultureChangedEventArgs : EventArgs
    {
        public CultureInfo Culture { get; set; }
        public CultureChangedEventArgs(CultureInfo _cultureInfo)
        {
            Culture = _cultureInfo;
        }
    }
}

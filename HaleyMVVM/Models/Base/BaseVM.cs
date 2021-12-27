using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Abstractions;
using Haley.Events;

namespace Haley.Models
{
    public class BaseVM : ChangeNotifier, IHaleyVM
    {
        public event EventHandler<FrameClosingEventArgs> ViewModelClosed;
        public event EventHandler<EventArgs> ViewLoaded;
        public virtual void OnViewLoaded(object sender)
        {
            //Send event.
            ViewLoaded?.Invoke(sender, null);
        }

        protected void InvokeVMClosed(object sender, FrameClosingEventArgs e)
        {
            ViewModelClosed?.Invoke(sender, e);
        }
        public BaseVM() { }
    }
}

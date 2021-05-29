using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Abstractions;
using Haley.Events;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Threading;

namespace Haley.Models
{
    public class HaleyObserver
    {
        //This is kind of interlinked. Ideally, View is the one which initiates the Close command. 
        // For example,  1. when we press a button in View, it sends a command to ViewModel to perform a Action.
        //2. At the end of the action, it triggers the event inside the ViewModel (OnClosingEvent; it is actually publishing the status about the end of process.) 
        //3. Status can either be true or false. Which means, we can either close the window or keep it active. 
        //4. So, View, which has subscribed to that event, receives that status and saves as dialog result. If dialog result is true, then it will close the winodw else it will keep it active.
        public IHaleyWindow subscriber { get; set; }
        public IHaleyVM publisher { get; set; }

        public void subscribe()
        {
            publisher.OnWindowsClosed += _onWindowsClosedHandler;
        }

        public void unSubscribe() // This takes care of unsubscribing the events
        {
            publisher.OnWindowsClosed -= _onWindowsClosedHandler;
        }

        private void _onWindowsClosedHandler(object sender, FrameClosingEventArgs e)
        {
            if (e.event_result.HasValue)
            {
                subscriber.DialogResult = e.event_result; //The event result will be invoked when a user presses a button. When user presses a button, it will invoke the event stored inside the viewmodel with an input value (bool, in our case). Thus, the event inside the viewmodel is invoked with a value which in turn is stored in the dialogresult.
                unSubscribe(); //the moment we get the dialog resut. we can unsubscribe.
            }
            else
            {
                unSubscribe(); // Under any case, we need to unsubscribe first before closing the dialog window.
                subscriber.Close(); //If user manages to close the dialog without using the predefined button, then the window is closed.
            }
        }

        public HaleyObserver(IHaleyWindow subscriberView, IHaleyVM publisherViewModel)
        {
            subscriber = subscriberView;
            publisher = publisherViewModel;
            subscribe();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.ViewModels;

namespace Haley.MVVM.Services
{
    public class DialogService : IDialogService
    {
        public string input_message { get; set; }

        public bool send(string title,string message, DialogMode mode = DialogMode.Notification)
        {
            switch(mode)
            {
                case DialogMode.Confirmation:
                    //Notification.
                    ConfirmationVM _confirmation = new ConfirmationVM(title, message);
                    if (ContainerStore.Singleton.windows.showDialog<ConfirmationVM>(_confirmation).Value) { return true; }
                    break;
                case DialogMode.GetInput:
                    //Notification.
                    GetInputVM _getInput = new GetInputVM(title, message);
                    if (ContainerStore.Singleton.windows.showDialog<GetInputVM>(_getInput).Value) { return true; }
                    break;
                case DialogMode.Notification:
                    //Notification.
                    NotificationVM _notification = new NotificationVM(title, message);
                    if (ContainerStore.Singleton.windows.showDialog<NotificationVM>(InputViewModel: _notification).Value) { return true; }
                    break;
            }
            return false;
        }

        public bool receive(string title, string message, out string user_input)
        {
            GetInputVM _getInput = new GetInputVM(title, message);
            _getInput.OnWindowsClosed += __baseVm_OnWindowsClosed;
            user_input = null;
            if (ContainerStore.Singleton.windows.showDialog<GetInputVM>(_getInput).Value)
            {
              user_input = input_message;
              return true;
            }
            return false;
        }

        private void __baseVm_OnWindowsClosed(object sender, Events.FrameClosingEventArgs e)
        {
            input_message = (e.message != null) ? (string) e.message : null;
        }
    }
}

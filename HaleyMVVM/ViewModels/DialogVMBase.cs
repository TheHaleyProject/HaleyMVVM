using System;
using System.Collections.Generic;
using System.Text;
using Haley.Models;
using Haley.Abstractions;
using Haley.Events;
using System.Windows.Input;

namespace Haley.WPF.ViewModels
{
    public class DialogVMBase : ChangeNotifier, IHaleyWindowVM
    {
        public event EventHandler<FrameClosingEventArgs> OnWindowsClosed;

        private string _title;
        /// <summary>
        /// Title to be displayed on the window.
        /// </summary>
        public string title
        {
            get { return _title; }
            set { SetProp(ref _title, value); }
        }

        private string _message;
        /// <summary>
        /// Message to display to the user.
        /// </summary>
        public string message
        {
            get { return _message; }
            set { SetProp(ref _message, value); }
        }

        private string _input;
        /// <summary>
        /// Gets the input from the user.
        /// </summary>
        public string input
        {
            get { return _input; }
            set { SetProp(ref _input, value); }
        }

        public bool canSendInput(bool obj)
        {
            return (!string.IsNullOrEmpty(input));
        }

        public ICommand cmd_send_input => new DelegateCommand<bool>(_sendInput, canSendInput);
        public ICommand cmd_close_window => new DelegateCommand<bool>(_closeWindow, null);


        private void _sendInput(bool obj)
        {
            OnWindowsClosed?.Invoke(this, new FrameClosingEventArgs(obj, input));
        }

        private void _closeWindow(bool obj)
        {
            OnWindowsClosed?.Invoke(this, new FrameClosingEventArgs(obj, null));
        }

        public DialogVMBase(string _title, string _message) { title = _title; message = _message; }
    }
}

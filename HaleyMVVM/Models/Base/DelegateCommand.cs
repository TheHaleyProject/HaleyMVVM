using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using System.Reflection;
using System.Windows;

namespace Haley.Models
{
   public class DelegateCommand : RelayCommand
    {
        public DelegateCommand(Action ActionMethod, Func<bool> ValidationFunction) :base(ActionMethod,ValidationFunction)
        {
            //For actions without the need for any parameters. 
        }

        public DelegateCommand(Action ActionMethod) : base(ActionMethod)
        {
        }

        public override void RaiseCanExecuteChanged() {
            CommandManager.InvalidateRequerySuggested();
        }

        public override event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

    
    public class DelegateCommand<T> : RelayCommand<T>
    {
        public override event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action<T> ActionMethod, Func<T, bool> ValidationFunction) : base(ActionMethod, ValidationFunction) {
            
        }

        public DelegateCommand(Action<T> ActionMethod):base(ActionMethod)
        {
            
        }

        public override void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

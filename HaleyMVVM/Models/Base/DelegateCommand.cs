using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using System.Reflection;
using System.Windows;

namespace Haley.Models
{
   public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action ActionMethod, Func<bool> ValidationFunction) :base((p)=>ActionMethod(), (p)=> ValidationFunction())
        {
            //For actions without the need for any parameters. 
        }

        public DelegateCommand(Action ActionMethod) : base((p)=>ActionMethod())
        {
        }
    }

    
    public class DelegateCommand<T> : ICommand
    {
        Action<T> _action;
        Func<T, bool> _validation;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public virtual bool CanExecute(object parameter)
        {
            if (_validation == null) return true;
            T _param = (parameter != null) ? (T)parameter : default(T);
            return _validation.Invoke(_param);
        }

        public virtual void Execute(object parameter)
        {
            _action?.Invoke((T)parameter);
        }

        public DelegateCommand(Action<T> ActionMethod, Func<T, bool> ValidationFunction)
        {
            _action = ActionMethod;
            _validation = ValidationFunction;
        }

        public DelegateCommand(Action<T> ActionMethod)
        {
            _action = ActionMethod;
            _validation = null;
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

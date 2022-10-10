using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows.Input;
using System.Reflection;
using System.Windows;
using Microsoft.Xaml.Behaviors;

#pragma warning disable IDE1006 // Naming Styles
namespace Haley.Models
{
    public sealed class EventToCommand : TriggerAction<DependencyObject>
    {
#region Dependency Properties

        public ICommand Command

        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EventToCommand), null);

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(EventToCommand), null);

        /// <summary>
        /// If set, then command parameter will be reset (even if some binding already exists) with the arguments
        /// </summary>
        public bool BindEventArgs
        {
            get { return (bool)GetValue(BindEventArgsProperty); }
            set { SetValue(BindEventArgsProperty, value); }
        }

        public static readonly DependencyProperty BindEventArgsProperty =
            DependencyProperty.Register(nameof(BindEventArgs), typeof(bool), typeof(EventToCommand), new PropertyMetadata(false));

        #endregion

        private string _command_name;
        public string CommandName
        {
            get { return _command_name; }
            set
            {
                if (_command_name != value) _command_name = value;
            }
        }

#region Methods
        protected override void Invoke(object parameter)
        {
            //if commandparameter is null, then check if we should bind the params. Then bind it.

            object cmdparam = null;
            if (BindEventArgs) {
                cmdparam = parameter;
            } else {
                cmdparam = CommandParameter;
            }

            if (this.AssociatedObject != null)
            {
                ICommand _cmd = _resolveCommand();
                if ((_cmd != null) && _cmd.CanExecute(cmdparam))
                {
                    _cmd.Execute(cmdparam);
                }
            }
        }

        private ICommand _resolveCommand()
        {
            ICommand result_cmd = null;

            if (Command != null) return Command; 

            //IF the users send in command as a string value (for commandname property) than binding actual command, then go below.

            var frameworkElement = this.AssociatedObject as FrameworkElement;
            if (frameworkElement != null)
            {
                object dataContext = frameworkElement.DataContext;
                if (dataContext != null)
                {
                    PropertyInfo commandPropertyInfo = dataContext
                        .GetType()
                        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .FirstOrDefault(
                            p =>
                            typeof(ICommand).IsAssignableFrom(p.PropertyType) &&
                            string.Equals(p.Name, this.CommandName, StringComparison.Ordinal)
                        );

                    if (commandPropertyInfo != null)
                    {
                        result_cmd = (ICommand)commandPropertyInfo.GetValue(dataContext, null);
                    }
                }
            }

            return result_cmd;
        }
#endregion
    }
}
#pragma warning restore IDE1006 // Naming Styles
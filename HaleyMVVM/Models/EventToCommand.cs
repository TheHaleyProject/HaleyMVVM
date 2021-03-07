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

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(EventToCommand), null);

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(EventToCommand), null);

        public object EventParameter
        {
            get { return (object)GetValue(EventParameterProperty); }
            set { SetValue(EventParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventParameterProperty =
            DependencyProperty.Register(nameof(EventParameter), typeof(object), typeof(EventToCommand), null);

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
            EventParameter = parameter; //Assigning the event parameter.

            if (this.AssociatedObject != null)
            {
                ICommand _cmd = _resolveCommand();
                if ((_cmd != null) && _cmd.CanExecute(CommandParameter))
                {
                    _cmd.Execute(CommandParameter);
                }
            }
        }

        private ICommand _resolveCommand()
        {
            ICommand result_cmd = null;

            if (Command != null) return Command;

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Haley.Abstractions;

namespace Haley.Models
{
    public abstract class ChangeNotifier: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propname = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        protected virtual bool SetProp<T>(ref T _attribute, T _value, [CallerMemberName] string propname = null)
        {
            if (EqualityComparer<T>.Default.Equals(_attribute, _value)) return false; //If both are equal don't proceed.

            _attribute = _value;
            OnPropertyChanged(propname);
            return true;
        }

        protected virtual bool SetProp<T>(ref T _attribute, T _value, Func<T, T, bool> validation_callback,[CallerMemberName] string propname = null)
        {
            if (EqualityComparer<T>.Default.Equals(_attribute, _value))
            {
                return false; //If both are equal don't proceed.
            }

            //Sometimes, there is a possibility that value has changed but we don't want to invoke the property change. So, call the validation.

            if (validation_callback != null && !validation_callback.Invoke(_attribute,_value))
            {
                return false;
            }
            _attribute = _value;
            OnPropertyChanged(propname);
            return true;
        }

        public ChangeNotifier() { }
    }
   
}

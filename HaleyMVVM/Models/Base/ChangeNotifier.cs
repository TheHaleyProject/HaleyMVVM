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
            return SetPropInternal(ref _attribute, _value, null, propname, false);
        }
        protected virtual bool SetProp<T>(ref T _attribute, T _value, bool override_equalitycheck, [CallerMemberName] string propname = null)
        {
            return SetPropInternal(ref _attribute, _value, null, propname, override_equalitycheck);
        }

        protected virtual bool SetProp<T>(ref T _attribute, T _value, Func<T, T, bool> validation_callback,[CallerMemberName] string propname = null)
        {
            return SetPropInternal(ref _attribute, _value, validation_callback, propname,false);
        }
        protected virtual bool SetProp<T>(ref T _attribute, T _value, Func<T, T, bool> validation_callback,bool override_equalitycheck, [CallerMemberName] string propname = null)
        {
            return SetPropInternal(ref _attribute, _value, validation_callback, propname, override_equalitycheck);
        }

        private bool SetPropInternal<T>(ref T _attribute, T _value, Func<T, T, bool> validation_callback, string propname,bool override_equalityCheck = false)
        {
            if (!override_equalityCheck)
            {
                //We should not override the equality if not requested.
                if (EqualityComparer<T>.Default.Equals(_attribute, _value))
                {
                    return false; //If both are equal don't proceed.
                }
            }
            
            //Sometimes, there is a possibility that value has changed but we don't want to invoke the property change. So, call the validation.
            if (validation_callback != null && !validation_callback.Invoke(_attribute, _value))
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

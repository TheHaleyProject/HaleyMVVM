using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Haley.Abstractions;
using System.Globalization;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;
using System.Reflection;
using System.Resources;
using Haley.Events;

namespace Haley.Models
{
    public class ResourceData : ChangeNotifier, IWeakEventListener
    {
        private string provider_key = string.Empty;
        private string resource_key = string.Empty;

        public object Value
        {
            get
            {
                return LangUtils.Translate(resource_key, provider_key);
            }
        }

        public ResourceData(string resourceKey,string providerKey)
        {
            resource_key = resourceKey;
            provider_key = providerKey;
            ResourceDataEventManager.AddListener(this);
        }

        //Destructors or finalizers.
        ~ResourceData()
        {
            ResourceDataEventManager.RemoveListener(this);
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            //Since this class is getting registered as a listener to the weakeventmanager, this will receive a call whenever the deliverevent is called.
            //Based on that, we need to raise whatever property we need to change.
            if (managerType == typeof(ResourceDataEventManager))
            {
                OnLanguageChanged(sender, e);
                return true;
            }
            return false;
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("Value");
        }
    }
}

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
    public class ResourceDataEventManager : WeakEventManager
    {
        #region Overridden Methods
        protected override void StartListening(object source)
        {
            LangUtils.Singleton.CultureChanged += LangUtils_CultureChanged;
        }

        protected override void StopListening(Object source)
        {
            LangUtils.Singleton.CultureChanged -= LangUtils_CultureChanged;
        }

        #endregion

        public static void AddListener(IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(null, listener);
        }

        public static void RemoveListener(IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(null, listener);
        }

        
        private void LangUtils_CultureChanged(object sender, CultureChangedEventArgs e)
        {
            DeliverEvent(null, e);
        }

        private static ResourceDataEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(ResourceDataEventManager);
                var manager = (ResourceDataEventManager)GetCurrentManager(managerType);
                if (manager == null)
                {
                    manager = new ResourceDataEventManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }
    }
}

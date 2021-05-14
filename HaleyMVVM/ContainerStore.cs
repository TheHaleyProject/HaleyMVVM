 using System;
using Haley.Abstractions;
using Haley.MVVM.Services;
using Haley.Enums;
using Haley.IOC;

namespace Haley.MVVM
{
    /// <summary>
    /// Sealed Container store which has DI, Controls, Windows 
    /// </summary>
    public sealed class ContainerStore
    {
        public IHaleyDIContainer DI { get; set; }
        public IHaleyControlContainer<IHaleyControlVM,IHaleyControl> controls { get;  }
        public IHaleyWindowContainer<IHaleyWindowVM,IHaleyWindow> windows { get;  }

        public ContainerStore() 
        {
            DI = new DIContainer() {};
            controls = new ControlContainer(DI); 
            windows = new WindowContainer(DI);
            _registerDialogs();
            _registerServices();
        }

        private void _registerDialogs()
        {
        }

        private void _registerServices()
        {
            DI.Register<IDialogService, DialogService>(RegisterMode.Transient);
        }
        public static ContainerStore Singleton = new ContainerStore();
    }
}

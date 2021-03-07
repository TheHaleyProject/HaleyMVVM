 using System;
using Haley.Abstractions;
using Haley.WPF.ViewModels;
using Haley.WPF.Views;
using Haley.MVVM.Services;
using Haley.Enums;
using Haley.IOC;

namespace Haley.MVVM
{
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
            windows.register<NotificationVM, NotificationWindow>(mode:RegisterMode.Transient);
            windows.register<ConfirmationVM, NotificationWindow>(mode: RegisterMode.Transient);
            windows.register<GetInputVM, NotificationWindow>(mode: RegisterMode.Transient);
        }

        private void _registerServices()
        {
            DI.Register<IDialogService, DialogService>(RegisterMode.Transient);
        }
        public static ContainerStore Singleton = new ContainerStore();
    }
}

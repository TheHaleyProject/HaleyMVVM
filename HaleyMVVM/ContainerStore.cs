 using System;
using Haley.Abstractions;
using Haley.MVVM.Services;
using Haley.Enums;
using Haley.IOC;
using System.Windows;
using System.Windows.Controls;

namespace Haley.MVVM
{
    /// <summary>
    /// Sealed Container store which has DI, Controls, Windows 
    /// </summary>
    public sealed class ContainerStore : IContainerFactory
    {
        public string Id { get; }
        public IBaseContainer DI { get; set; }
        public IControlContainer Controls { get;  }
        public IWindowContainer Windows { get;  }

        public ContainerStore() 
        {
            Id = Guid.NewGuid().ToString();
            DI = new DIContainer() {};
            Controls = new ControlContainer(DI); 
            Windows = new WindowContainer(DI);
            _registerSelf();
            _registerDialogs();
            _registerServices();
        }

        private void _registerSelf()
        {
            //Never register Base container because it is already registered.
            DI.Register<IControlContainer, ControlContainer>((ControlContainer)Controls, true);
            DI.Register<IWindowContainer, WindowContainer>((WindowContainer)Windows, true);
            DI.Register<IContainerFactory, ContainerStore>((ContainerStore)this, true);
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

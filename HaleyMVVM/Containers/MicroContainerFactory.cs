using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Models;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Services;

namespace Haley.IOC
{
    public sealed class MicroContainerFactory : ContainerFactory, IMicroContainerFactory
    {
        bool _initialized = false;
        public new IBaseServiceProvider Services { get; }
        public IBaseContainer Container 
        {
            get
            {
                return Services as IBaseContainer;
            }
        }

        bool Initiate()
        {
            if (!_initialized) return RegisterSelf();
            return false;
        }

        /// <summary>
        /// Will register only if the serviceprovider is of type BaseContainer.
        /// </summary>
        /// <returns></returns>
        bool RegisterSelf()
        {
            if (!(Services is IBaseContainer)) return false;
            //Because we know that in our ContainerFactory, the Iconrol,Iwindow,Iservice provider concrete implementations.
            //Forced singleton means that irrespective of how the resolve call is made (ResolveAsTransient, ResolveAsSingleton), we always resolve as singleton. That's what ForcedSingleton means.
            Container.Register<IControlContainer, ControlContainer>((ControlContainer)Controls, SingletonMode.ContainerSingleton);
            Container.Register<IWindowContainer, WindowContainer>((WindowContainer)Windows, SingletonMode.ContainerSingleton);
            Container.Register<IContainerFactory, ContainerFactory>(this, SingletonMode.ContainerSingleton);
            Container.Register<IBaseServiceProvider, MicroContainer>((MicroContainer)Services, SingletonMode.ContainerSingleton);
            Container.Register<IServiceProvider, MicroContainer>((MicroContainer)Services, SingletonMode.ContainerSingleton);
            return true;
        }

        public MicroContainerFactory(IBaseServiceProvider baseProvider):base()
        {
            if (baseProvider == null)
            {
                baseProvider = new MicroContainer(); //Create a new root provider.
            }
            Id = baseProvider.Id;
            Services = baseProvider;
            base.Services = baseProvider; //Also set the base.
            Controls = new ControlContainer(Services);
            Windows = new WindowContainer(Services);
            Initiate();
        }
    }
}

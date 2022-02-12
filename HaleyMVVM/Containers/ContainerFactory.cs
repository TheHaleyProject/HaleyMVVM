using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Models;
using Haley.Abstractions;

namespace Haley.IOC
{
    public class ContainerFactory : IContainerFactory
    {
        bool _initialized = false;
        public string Id { get; }
        public virtual IServiceProvider Services { get; }
        public IControlContainer Controls { get; }
        public IWindowContainer Windows { get; }
        public IBaseContainer GetDI()
        {
            if (Services is IBaseContainer)
            {
                return (IBaseContainer)Services;
            }
            return null;
        }

        public bool Initiate()
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
            if (Services is IBaseContainer)
            {
                //Because we know that in our ContainerFactory, the Iconrol,Iwindow,Iservice provider concrete implementations.
                //Forced singleton means that irrespective of how the resolve call is made (ResolveAsTransient, ResolveAsSingleton), we always resolve as singleton. That's what ForcedSingleton means.
                var DI = GetDI();
                DI.Register<IControlContainer, ControlContainer>((ControlContainer)Controls, true);
                DI.Register<IWindowContainer, WindowContainer>((WindowContainer)Windows, true);
                DI.Register<IContainerFactory, ContainerFactory>((ContainerFactory)this, true);
                DI.Register<IServiceProvider, DIContainer>((DIContainer)Services, true);
                DI.Register<IBaseContainer, DIContainer>((DIContainer)Services, true);
                return true;
            }

            return false;
        }

        public ContainerFactory(IServiceProvider serviceProvider)
        {
            Id = Guid.NewGuid().ToString();
            Services = serviceProvider;
            Controls = new ControlContainer(Services);
            Windows = new WindowContainer(Services); 
        }
    }
}

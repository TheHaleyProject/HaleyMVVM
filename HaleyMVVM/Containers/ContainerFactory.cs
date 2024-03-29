﻿using System;
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

        /// <summary>
        /// Will register only if the serviceprovider is of type BaseContainer.
        /// </summary>
        /// <returns></returns>
        public virtual bool RegisterSelf()
        {
            if (Services is IBaseContainer)
            {
                var DI = GetDI();
                DI.Register<IControlContainer, ControlContainer>((ControlContainer)Controls, true);
                DI.Register<IWindowContainer, WindowContainer>((WindowContainer)Windows, true);
                DI.Register<IContainerFactory, ContainerFactory>((ContainerFactory)this, true);
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

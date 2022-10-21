using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Models;
using Haley.Abstractions;
using Haley.Enums;

namespace Haley.IOC
{
    public class ContainerFactory : IContainerFactory
    {
        
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public IServiceProvider Services { get; protected set; }
        public IControlContainer Controls { get; protected set; }
        public IWindowContainer Windows { get; protected set; }

        public ContainerFactory(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentException("Service provider cannot be null. Please provide a valid provider");
            }

            if (serviceProvider is IMicroContainer basec) {
                Id = basec.Id;
            } else {
                Id = Guid.NewGuid().ToString(); //this should again be overriden 
            }
            Services = serviceProvider;
            Controls = new ControlContainer(Services);
            Windows = new WindowContainer(Services); 
        }
        [HaleyIgnore]
        protected ContainerFactory()
        {
        }

        public virtual void Dispose()
        {
            Controls = null;
            Windows = null;
        }
    }
}

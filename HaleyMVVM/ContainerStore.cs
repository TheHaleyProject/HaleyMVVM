 using System;
using Haley.Abstractions;
using Haley.Services;
using Haley.Enums;
using Haley.IOC;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;

namespace Haley.MVVM
{
    /// <summary>
    /// Sealed Container store which has DI, Controls, Windows 
    /// </summary>
    public sealed class ContainerStore
    {
        #region OBSOLETE
       
        #endregion

        public string Id { get; private set; }
        private IMicroContainerFactory _rootFactory;

        #region Static Attributes
        private static ContainerStore _instance;
        private ConcurrentDictionary<string, IContainerFactory> factories = new ConcurrentDictionary<string, IContainerFactory>();
        #endregion

        #region Root
        public IBaseServiceProvider Provider => _rootFactory.Services;
        public IWindowContainer Windows => _rootFactory.Windows;
        public IControlContainer Controls => _rootFactory.Controls;
        public IBaseContainer DI => _rootFactory.Container;
        #endregion

        /// <summary>
        /// Will return Root Factory
        /// </summary>
        /// <returns></returns>
        public IContainerFactory GetFactory()
        {
            return _rootFactory;
        }

        /// <summary>
        /// Used to return the factory related to a service provider.
        /// </summary>
        /// <param name="serviceProviderId"></param>
        /// <returns></returns>
        public IContainerFactory GetFactory(string serviceProviderId)
        {
            if (!factories.TryGetValue(serviceProviderId, out var factory)) return null;
            return factory;
        }

        private static ContainerStore getSingleton()
        {
            if (_instance == null)
            {
                //We will make default
                _instance = new ContainerStore(new MicroContainer()); ///The new container will be the root container.
            }
            return _instance;
        }

        private ContainerStore(IBaseContainer _baseContainer)
        {
            _baseContainer.ChildCreated += ChildCreatedHandler;
            _baseContainer.ContainerDisposed += ContainerDisposed;
            _rootFactory = new MicroContainerFactory(_baseContainer); //This will also do a self register

            //Since above creation method will also self register the factory and the other contianers into the base container, we don't need to get them from the factories (from below line) at all. However, we are adding it to the factories, so that we have an alternative way of fetching the factory, provided we have only the container id.
            factories.TryAdd(_rootFactory.Id, _rootFactory);
            Id = _baseContainer?.Id ?? Guid.NewGuid().ToString(); //if it is a micro factory it will register it self.
            _registerServices();
        }

        private void ContainerDisposed(object sender, string e)
        {
            //Dispose the continer which was send. //Root will not be removed as it is also stored in a variable.
            factories.TryRemove(e, out var removedFactory);
            if (removedFactory is IMicroContainerFactory microFactory)
            {
                removedFactory.Dispose();
                microFactory.Container.ContainerDisposed -= ContainerDisposed;
            }
        }

        private void ChildCreatedHandler(object sender, IBaseContainer e)
        {
            //Sender is who created this child. //All we need to do is, whenever a new child is created, we create a new factory for it and add it to the repo here.
            //Also subscribe to the new child's events.
            if (e is null) return;

            var microFactory = new MicroContainerFactory(e);
            if (factories.TryAdd(e.Id,microFactory))
            {
                e.ChildCreated += ChildCreatedHandler;
                e.ContainerDisposed += ContainerDisposed;
            }
        }

        private void _registerServices()
        {
            DI.Register<IDialogService, DialogService>(RegisterMode.ContainerSingleton);
            DialogService _dservice = DI.Resolve<IDialogService>() as DialogService;
            DI.Register<IDialogServiceEx, DialogService>(_dservice);
            //If we register the dialogservice as Transient, then for each resolution, it will create separate instance. So, different classes might have different properties (like glow color, header, background).
            //So we register as singleton. If user wishes to resolve as transient, then he/she can still do that by ResolveAsTransient (as it is not forced singleton).
        }
        public static ContainerStore Singleton => getSingleton();
    }
}

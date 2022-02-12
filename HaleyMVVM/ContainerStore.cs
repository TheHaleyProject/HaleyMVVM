 using System;
using Haley.Abstractions;
using Haley.Services;
using Haley.Enums;
using Haley.IOC;
using System.Windows;
using System.Windows.Controls;

namespace Haley.MVVM
{
    /// <summary>
    /// Sealed Container store which has DI, Controls, Windows 
    /// </summary>
    public sealed class ContainerStore
    {
        #region Static Attributes
        private static ContainerStore _instance;

        #endregion

        private IContainerFactory _factory;
        public bool CanRegister { get; private set; }
        public IBaseContainer DI => _getDI();
        public IServiceProvider Provider => _factory.Services;
        public IWindowContainer Windows => _factory.Windows;
        public IControlContainer Controls => _factory.Controls;
        public string Id { get; private set; }

        public IContainerFactory GetFactory()
        {
            return _factory;
        }

        private IBaseContainer _getDI()
        {
            return _factory.GetDI();
        }

        private static ContainerStore getSingleton()
        {
            if (_instance == null)
            {
                //We will make default
                _instance = new ContainerStore(new DIContainer()); //We use the base DI container.
            }
            return _instance;
        }

        public static ContainerStore CreateSingleton(IContainerFactory factory)
        {
            if (_instance != null)
            {
                throw new ArgumentException("Factory can be initiated only once.");
            }

            _instance = new ContainerStore(factory);
            return _instance;
        }

        private ContainerStore(IContainerFactory container_factory)
        {
            _initiate(container_factory);
        }

        private ContainerStore(IBaseContainer _baseContainer)
        {
            _initiate(new ContainerFactory(_baseContainer));
            _registerSelf();
            _registerServices();
        }

        private void _initiate(IContainerFactory container_factory)
        {
            //Set ID
            Id = Guid.NewGuid().ToString();
            //just set this.
            _factory = container_factory;
            CanRegister = _factory is IBaseContainer;
        }

        private void _registerSelf()
        {
            _factory.Initiate();
        }

        private void _registerServices()
        {
            DI.Register<IDialogService, DialogService>(RegisterMode.Singleton);
            DialogService _dservice = DI.Resolve<IDialogService>() as DialogService;
            DI.Register<IDialogServiceEx, DialogService>(_dservice);
            //If we register the dialogservice as Transient, then for each resolution, it will create separate instance. So, different classes might have different properties (like glow color, header, background).
            //So we register as singleton. If user wishes to resolve as transient, then he/she can still do that by ResolveAsTransient (as it is not forced singleton).
        }
        public static ContainerStore Singleton => getSingleton();
    }
}

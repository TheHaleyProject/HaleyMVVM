 using System;
using Haley.Abstractions;
using Haley.Services;
using Haley.Enums;
using Haley.IOC;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Concurrent;
using System.Windows.Navigation;

namespace Haley.MVVM
{
    /// <summary>
    /// Sealed Container store which has DI, Controls, Windows 
    /// </summary>
    public sealed class ContainerStore
    {
        #region Static Attributes
        private static ContainerStore INSTANCE = getSingleton();
        public static IBaseServiceProvider Provider => INSTANCE._rootFactory.Services;
        public static IWindowContainer Windows => INSTANCE._rootFactory.Windows;
        public static IControlContainer Controls => INSTANCE._rootFactory.Controls;
        public static IMicroContainer DI => INSTANCE._rootFactory.Container;
        public static string Id => INSTANCE._id;

        /// <summary>
        /// Will return Root Factory
        /// </summary>
        /// <returns></returns>
        public static IContainerFactory GetFactory() {
            //Always the root.
            return INSTANCE._rootFactory;
        }

        /// <summary>
        /// Used to return the factory related to a service provider.
        /// </summary>
        /// <param name="serviceProviderId"></param>
        /// <returns></returns>
        public static IContainerFactory Getchild(string id, bool search_all_children = false) {
            if (INSTANCE._rootFactory == null) return null;
            return INSTANCE._rootFactory.GetChild(id, search_all_children);
        }

        #endregion

        private IMicroContainerFactory _rootFactory;
        private string _id;
        private static ContainerStore getSingleton()
        {
            if (INSTANCE == null)
            {
                //We will make default
                INSTANCE = new ContainerStore(new MicroContainer()); ///The new container will be the root container.
            }
            return INSTANCE;
        }

        private ContainerStore(IMicroContainer _baseContainer)
        {
            _rootFactory = new MicroContainerFactory(_baseContainer); //This will also do a self register

            //Since above creation method will also self register the factory and the other contianers into the base container, we don't need to get them from the factories (from below line) at all. However, we are adding it to the factories, so that we have an alternative way of fetching the factory, provided we have only the container id.
            _id = _baseContainer?.Id ?? Guid.NewGuid().ToString(); //if it is a micro factory it will register it self.
            _registerServices();
        }

        private void _registerServices()
        {
            var container = _rootFactory?.Container;
            if (container == null) return;
            container.Register<IDialogService, DialogService>(RegisterMode.UniversalSingleton);
            DialogService _dservice = container.Resolve<IDialogService>() as DialogService;
            container.Register<IDialogServiceEx, DialogService>(_dservice,SingletonMode.UniversalSingleton);
            container.Register<IThemeService, ThemeService>(ThemeService.Singleton,SingletonMode.UniversalSingleton);
            container.Register<IConfigService, ConfigManagerService>(RegisterMode.UniversalSingleton);
            //If we register the dialogservice as Transient, then for each resolution, it will create separate instance. So, different classes might have different properties (like glow color, header, background).
            //So we register as singleton. If user wishes to resolve as transient, then he/she can still do that by ResolveAsTransient (as it is not forced singleton).
        }
        [Obsolete(@"Remove the SINGLETON keyword. Replace ""ContainerStore.Singleton.[METHOD/PROPERTY]"" with ""ContainerStore.[METHOD/PROPERTY]""", true)]
        public static ContainerStore Singleton => null;
    }
}

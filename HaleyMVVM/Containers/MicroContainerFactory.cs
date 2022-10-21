using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Models;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Services;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Haley.IOC
{
    public sealed class MicroContainerFactory : ContainerFactory, IMicroContainerFactory {

        public bool IsDisposed { get; private set; }
        bool _initialized = false;
        public event EventHandler<IMicroContainerFactory> ChildFactoryCreated;
        public event EventHandler<string> FactoryDisposed;
        readonly ConcurrentDictionary<string, IMicroContainerFactory> _childFactories = new ConcurrentDictionary<string, IMicroContainerFactory>();
        internal bool IsRoot = false; //Whenever the container is created using the "CreateChildContainerMode" it should not be a root.


        public new IBaseServiceProvider Services {
            get {
                return base.Services as IBaseServiceProvider;
            }
        }
        public IMicroContainer Container {
            get {
                return base.Services as IMicroContainer;
            }
        }

        public IMicroContainerFactory Parent { get; private set; }

        public IMicroContainerFactory Root { get; private set; }

        public IMicroContainerFactory this[string key] => GetChild(key);

        public IMicroContainerFactory GetChild(string id,bool search_all_children = false) {
            if (string.IsNullOrWhiteSpace(id) || _childFactories == null || _childFactories?.Count == 0) return null;
            IMicroContainerFactory factory = null;
            if(!_childFactories.TryGetValue(id, out factory) && search_all_children) {
                //Unable to fetch it.
                foreach (var child in _childFactories?.Values) {
                    factory = child.GetChild(id, search_all_children);
                    if (factory != null) return factory;
                }
            }
            return factory;
        }

        bool Initiate()
        {
            if (!_initialized) {
                _initialized =  RegisterSelf();
            }
            return _initialized;
        }

        /// <summary>
        /// Will register only if the serviceprovider is of type BaseContainer.
        /// </summary>
        /// <returns></returns>
        bool RegisterSelf()
        {
            if (!(Services is IMicroContainer)) return false;
            //Because we know that in our ContainerFactory, the Iconrol,Iwindow,Iservice provider concrete implementations.
            //Forced singleton means that irrespective of how the resolve call is made (ResolveAsTransient, ResolveAsSingleton), we always resolve as singleton. That's what ForcedSingleton means.
            Container.Register<IControlContainer, ControlContainer>((ControlContainer)Controls, SingletonMode.ContainerSingleton);
            Container.Register<IWindowContainer, WindowContainer>((WindowContainer)Windows, SingletonMode.ContainerSingleton);
            Container.Register<IContainerFactory, ContainerFactory>(this, SingletonMode.ContainerSingleton);
            Container.Register<IBaseServiceProvider, MicroContainer>((MicroContainer)Services, SingletonMode.ContainerSingleton);
            Container.Register<IServiceProvider, MicroContainer>((MicroContainer)Services, SingletonMode.ContainerSingleton);
           
            return true;
        }

        public IMicroContainerFactory CreateChild(Guid id, string name, bool ignore_parentcontainer) {
            IMicroContainerFactory result = null;
            try {
                if (Container == null) {
                    throw new NullReferenceException($@"{nameof(Container)} is empty.");

                } else if (Container.IsDisposed) {
                    throw new AccessViolationException($@"{nameof(Container)} is already disposed. Cannot create child to disposed containers.");
                }

                //Whenever a factory is created, we would have already subscribed to the container's child creation/removed events.
                //so we only need to create a child and handle it in the subscription

                var child_container = Container.CreateChild(id, name, ignore_parentcontainer);
                if (child_container == null) throw new NullReferenceException("Child container is not created. Unable to create factory");

                //Handler should have create a factory for this container. Just verify it and return.
                
                if (!_childFactories.TryGetValue(child_container.Id, out result)) {

                    result = CreateChildFactoryInternal(child_container);
                }
                ChildFactoryCreated?.Invoke(this, result); //So, if there are any other actions to be done by others, can happen based on this event.
                                                    //If not, recreate it.
                return result;
            } catch {
                return result;
            }
        }

        public IMicroContainerFactory CreateChild(Guid id = default, bool ignore_parentcontainer = false) {
            return CreateChild(id, null, ignore_parentcontainer);
        }

        public IMicroContainerFactory CreateChild(string name, bool ignore_parentcontainer = false) {
            return CreateChild(default(Guid), name, ignore_parentcontainer);
        }

        public IMicroContainerFactory CreateChild() {
            return CreateChild(default(Guid), null, false);
        }

        public MicroContainerFactory(IMicroContainer microContainer):base(microContainer ?? new MicroContainer())
        {
            if (Container == null) return;

            Container.ChildContainerCreated += ContainerCreated;
            Container.ContainerDisposed += ContainerDisposed;
            IsRoot = true;
            Initiate();
        }

        private void ContainerDisposed(object sender, string e) {
            //Dispose the continer which was send. //Root will not be removed as it is also stored in a variable.
            _childFactories.TryRemove(e, out var removedFactory);
            if (removedFactory is IMicroContainerFactory microFactory) {
                removedFactory.Dispose();
                microFactory.Container.ContainerDisposed -= ContainerDisposed;
            }
        }

        private void ContainerCreated(object sender, IMicroContainer e) {
            //Sender is who created this child. //All we need to do is, whenever a new child is created, we create a new factory for it and add it to the repo here.
            CreateChildFactoryInternal(e);
        }

        private IMicroContainerFactory CreateChildFactoryInternal(IMicroContainer container) {
            
            //Also subscribe to the new child's events.
            if (container is null) return null;

            var microFactory = new MicroContainerFactory(container) { Parent = this, IsRoot = false }; //this will subscribe to the child creation changes of this container.
            microFactory.Root = this.IsRoot ? this : this.Root;//If you are creating this from inside a root container, then "this" is the root for the child else this container's root is also the root for the child.

            microFactory.FactoryDisposed += ChildFactoryDisposed;

            //When this factory is disposed, we need to remove it from our storage as well or else we will still ahve old value.

            _childFactories.TryAdd(container.Id, microFactory);
            return microFactory;
        }

        private void ChildFactoryDisposed(object sender, string e) {
            //Whenever a child factory is directly disposed, we need to remove it from our list.
            _childFactories.TryRemove(e, out var removedFactory);
            if (removedFactory is IMicroContainerFactory microFactory) {
                microFactory.FactoryDisposed -= ChildFactoryDisposed;
            }
        }

        public override void Dispose() {

            if (IsDisposed) return; 
            base.Dispose();
            //then dispose here as well
            foreach (var kvp in _childFactories) {
                kvp.Value.Dispose();
            }
            _childFactories?.Clear(); //All child container registrations will also be cleared.

            Container.Dispose(); //dispose the container itself, which in turn will call and dispose the child containers.
            //Parent and root remains the same.
            
            IsDisposed = true;
            FactoryDisposed?.Invoke(this, Id); //To notify the parent that this is disposed.
        }
    }
}

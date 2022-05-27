using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Events;
using Haley.Utils;
using System.Collections.Concurrent;
using Haley.Enums;

namespace Haley.Abstractions
{
    public abstract class UIContainerBase<BaseViewModelType> : IUIContainerBase<BaseViewModelType>
    {
        public Type BaseViewType;
        public string Id { get; }

        #region Initation
        protected IServiceProvider service_provider;
        private IContainerFactory _factory;
        protected IContainerFactory container_factory 
        {
            get 
            {
                if (_factory == null)
                {
                    _factory = service_provider.GetService(typeof(IContainerFactory)) as IContainerFactory;
                }
                return _factory;
            } 
        }

        protected ConcurrentDictionary<string,(Type VMtype, Type ViewType,RegisterMode mode)> main_mapping { get; set; } //Dictionary to store enumvalue and viewmodel type as key and usercontrol as value

        public UIContainerBase(IServiceProvider _serviceProvider,Type baseViewType)
        {
            service_provider = _serviceProvider;
            BaseViewType = baseViewType;

            if (_serviceProvider is IBaseServiceProvider basePrvdr)
            {
                Id = basePrvdr?.Id ??Guid.NewGuid().ToString(); //For base providers, the ID should match the service provider (so that it would be easy to fetch the relevant container factory)
            }
            else
            {
                Id = Guid.NewGuid().ToString();
            }

            main_mapping = new ConcurrentDictionary<string, (Type VMtype, Type ViewType, RegisterMode mode)>();

            if (_serviceProvider == null)
            {
                throw new ArgumentException("Service provider cannot be empty while initiating UIContainerBase");
            }

            if(BaseViewType == null)
            {
                throw new ArgumentException("baseViewType cannot be empty while initiating UIContainerBase");
            }
           
        }

        #endregion

        #region Register Methods

        public virtual string Register<viewmodelType, viewType>(object key, viewmodelType InputViewModel = null, RegisterMode mode = RegisterMode.ContainerSingleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : class
        {
            //Get the enum value and its type name to prepare a string
            getKey(key,out var _key);
            return RegisterInternal<viewmodelType, viewType>(_key, InputViewModel,null, mode);
        }

        public virtual string DelegateRegister<viewmodelType, viewType>(Func<viewmodelType> creator, bool use_vm_as_key = true, RegisterMode mode = RegisterMode.ContainerSingleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : class
        {
            string _key = null;
            if (use_vm_as_key)
            {
                _key = typeof(viewmodelType).ToString();
            }
            else
            {
                _key = typeof(viewType).ToString();
            }

            return DelegateRegister<viewmodelType, viewType>(_key, creator, mode);
        }

        public virtual string DelegateRegister<viewmodelType, viewType>(object key, Func<viewmodelType> creator, RegisterMode mode = RegisterMode.ContainerSingleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : class
        {
            getKey(key, out var _key);
            return RegisterInternal<viewmodelType, viewType>(_key, null,creator,  mode);
        }

        public virtual string Register<viewmodelType, viewType>(viewmodelType InputViewModel = null, bool use_vm_as_key = true, RegisterMode mode = RegisterMode.ContainerSingleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : class
        {
            string _key = null;
            if (use_vm_as_key)
            {
                _key = typeof(viewmodelType).ToString();
            }
            else
            {
                _key = typeof(viewType).ToString();
            }

           return RegisterInternal<viewmodelType, viewType>(_key, InputViewModel,null, mode);
        }

        protected string RegisterInternal<viewmodelType, viewType>(string key, viewmodelType InputViewModel = null, Func<viewmodelType> vmCreator = null, RegisterMode mode = RegisterMode.ContainerSingleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) key = typeof(viewType).ToString();

                ValidateViewType(typeof(viewType));

                //First add the internal main mappings.
                if (main_mapping.ContainsKey(key) == true)
                {
                    throw new ArgumentException($@"Key : {key} is already registered to - VM : {main_mapping[key].VMtype.GetType()} and View : {main_mapping[key].ViewType.GetType()}");
                }

                var _tuple = (typeof(viewmodelType), typeof(viewType), mode);
                main_mapping.TryAdd(key, _tuple);

                //If service provider is of type base provider then we can register it aswell (as it will have an implementation)

                if(!(service_provider is IBaseContainer baseContainer))return key;
                //If registermode is anything other than singleton or weaksingleton, do not validate.

                if (mode == RegisterMode.UniversalSingleton)
                {
                    throw new ArgumentException("Universal singleton registrations has to be directly done on the Root DI container. Cannot register from the Control/Window or child containers.");
                }

                if (mode != RegisterMode.ContainerSingleton && mode != RegisterMode.ContainerWeakSingleton)return key;

                //For ContainerSingletonMode, directly register using the view or the viewmodel type as key.
                //For WeakSingleton, register using (View/Viewmodel-combo key).
                var vm_status = baseContainer.CheckIfRegistered(typeof(viewmodelType), null);
                if (!vm_status.status)
                {
                    if (InputViewModel != null)
                    {
                        baseContainer.Register(InputViewModel, GetMode(mode));
                    }
                    else if(vmCreator != null)
                    {
                        baseContainer.Register(vmCreator, GetMode(mode));
                    }
                    else
                    {
                        //Dont send in any instance. It will be created based on the type.
                        baseContainer.Register<viewmodelType>(null, GetMode(mode));
                    }
                }

                var view_status = baseContainer.CheckIfRegistered(typeof(viewType), null);
                if (!view_status.status)
                {
                    baseContainer.Register<viewType>(mode);
                }

                return key;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Private Methods
        private SingletonMode GetMode(RegisterMode mode)
        {
            switch (mode)
            {
                case RegisterMode.ContainerSingleton:
                    return SingletonMode.ContainerSingleton;
                case RegisterMode.ContainerWeakSingleton:
                    return SingletonMode.ContainerWeakSingleton;
                case RegisterMode.UniversalSingleton:
                    return SingletonMode.UniversalSingleton;
            }
            return SingletonMode.ContainerSingleton;
        }
        private bool ValidateViewType(Type viewType)
        {
            if (!BaseViewType.IsAssignableFrom(viewType))
            {
                throw new ArgumentException($@"View type is not matching for this container. Expected type of view is {BaseViewType.ToString()}. {viewType} is not derived from {BaseViewType}");
            }
            return true;
        }
        protected (BaseViewModelType view_model, object view) _generateValuePair(string key, ResolveMode mode)
        {
            var _mapping_value = GetMappingValue(key);

            //Generate a View
            object resultcontrol = _generateView(_mapping_value.view_type,mode);
            BaseViewModelType resultViewModel = _generateViewModel(_mapping_value.viewmodel_type, mode);
            return (resultViewModel, resultcontrol);
        }
        protected object _generateView(Type viewType, ResolveMode mode = ResolveMode.AsRegistered)
        {
            try
            {
                //AT PRESENT, ONLY THE VIEWS REGISTERED IN THIS CONTAINER IS RESOLVED.
                //Even view should be resolved by _di instance. because sometimes, views can direclty expect some 
                if (viewType == null) return null;
                object resultcontrol;
                object _baseView = null;

                if (service_provider is IBaseContainer baseContainer)
                {
                    _baseView = baseContainer.Resolve(viewType, mode);
                }
                else
                {
                    _baseView = service_provider.GetService(viewType);
                }

                if (_baseView != null)
                {
                    resultcontrol = _baseView;
                }
                else
                {
                    //Just to ensure that it is not null.
                    resultcontrol = Activator.CreateInstance(viewType);
                }

                return resultcontrol;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected BaseViewModelType _generateViewModel(Type viewModelType, ResolveMode mode = ResolveMode.AsRegistered) //If required we can even return the actural viewmodel concrete type as well.
        {
            try
            {
                BaseViewModelType _result  = default(BaseViewModelType);
                if (viewModelType == null) return default(BaseViewModelType);
                //If the viewmodel is registered in DI as a singleton, then it willbe returned, else, DI will resolve it as a transient and will return the result.
                object _baseVm = null;
                if (service_provider is IBaseContainer)
                {
                    _baseVm = ((IBaseContainer) service_provider).Resolve(viewModelType, mode);
                }
                else
                {

                    _baseVm = service_provider.GetService(viewModelType);
                }

                if (_baseVm != null)
                {
                    _result = (BaseViewModelType)_baseVm;
                }
                
                return _result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region View Retrieval Methods
        //Return a generic type which implements BaseViewType 
        public object GenerateView<viewmodelType>(viewmodelType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered) 
            where viewmodelType : class, BaseViewModelType
        {
            string _key = typeof(viewmodelType).ToString();
            return GenerateViewFromKey(_key, InputViewModel, mode);
        }
        public viewType GenerateView<viewType>(object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered)
            where viewType : class
        {
            ValidateViewType(typeof(viewType));
            string _key = typeof(viewType).ToString();
            return GenerateViewFromKey(_key, InputViewModel, mode) as viewType;
        }
        public abstract object GenerateViewFromKey(object key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered) ;
        
        #endregion

        #region VM Retrieval Methods
        public (Type viewmodel_type, Type view_type, RegisterMode registered_mode) GetMappingValue(Enum @enum)
        {
            //Get the enum value and its type name to prepare a string
            string _key = @enum.GetKey();
            return GetMappingValue(_key);
        }
        public (Type viewmodel_type, Type view_type, RegisterMode registered_mode) GetMappingValue(string key)
        {
            if (main_mapping.Count == 0 || !main_mapping.ContainsKey(key))
            {
                throw new ArgumentException($"Key {key} is not registered to any controls. Please check.");
            }

            (Type _viewmodel_type, Type _view_type, RegisterMode _mode) _registered_tuple = (null, null, RegisterMode.ContainerSingleton);
            main_mapping.TryGetValue(key, out _registered_tuple);

            //if (_registered_tuple._viewmodel_type == null || _registered_tuple._view_type == null)
            //{
            //    StringBuilder sbuilder = new StringBuilder();
            //    sbuilder.AppendLine($@"The key {key} has null values associated with it.");
            //    sbuilder.AppendLine($@"ViewModel Type : {_registered_tuple._viewmodel_type}");
            //    sbuilder.AppendLine($@"View Type : {_registered_tuple._view_type}");
            //    throw new ArgumentException(sbuilder.ToString());
            //}

            return _registered_tuple;
        }
        public BaseViewModelType GenerateViewModelFromKey(object key, ResolveMode mode = ResolveMode.AsRegistered) //If required we can even return the actural viewmodel concrete type as well.
        {
            if (!getKey(key, out var _key)) return default(BaseViewModelType);
            var _mapping_value = GetMappingValue(_key);
            return _generateViewModel(_mapping_value.viewmodel_type, mode);
        }

        public string FindKey(Type target_type)
        {
            //For the given target type, find if it is present in the mapping values. if found, return the first key.
            var _kvp = main_mapping.FirstOrDefault(kvp => kvp.Value.VMtype == target_type || kvp.Value.ViewType == target_type);
            if (_kvp.Value.VMtype == null && _kvp.Value.ViewType == null) return null;
            return _kvp.Key;
        }

        public bool? ContainsKey(object key)
        {
            if (!getKey(key, out var _key)) return null;
            return main_mapping.ContainsKey(_key);
        }

        #endregion

        protected bool getKey(object key,out string processed_key)
        {
            processed_key = string.Empty;
            if (key is Enum @enum)
            {
                processed_key = @enum.GetKey();
            }
            else if(key.GetType() == typeof(string))
            {
                processed_key = key as string;
            }
            if (!string.IsNullOrWhiteSpace(processed_key)) return true;
            return false;
        }
    }
}


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
    public abstract class UIContainerBase<BaseViewModelType,BaseViewType> : IUIContainerBase<BaseViewModelType, BaseViewType>
    {
        public string Id { get; }

        #region Initation
        protected IBaseContainer _di_instance;
        protected ConcurrentDictionary<string,(Type VMtype, Type ViewType,RegisterMode mode)> main_mapping { get; set; } //Dictionary to store enumvalue and viewmodel type as key and usercontrol as value

        public UIContainerBase(IBaseContainer _injection_container)
        {
            Id = Guid.NewGuid().ToString();
            main_mapping = new ConcurrentDictionary<string, (Type VMtype, Type ViewType, RegisterMode mode)>();
            if (_injection_container != null)
            {
                _di_instance= _injection_container;
            }
        }

        #endregion

        #region Register Methods
        public virtual string Register<viewmodelType, viewType>(viewmodelType InputViewModel = null, bool use_vm_as_key = true, RegisterMode mode = RegisterMode.Singleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : BaseViewType
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

           return Register<viewmodelType, viewType>(_key, InputViewModel, mode);
        }

        public virtual string Register<viewmodelType, viewType>(Enum @enum, viewmodelType InputViewModel = null, RegisterMode mode = RegisterMode.Singleton)
           where viewmodelType : class, BaseViewModelType
           where viewType : BaseViewType
        {
            //Get the enum value and its type name to prepare a string
            string _key = @enum.getKey();
           return Register<viewmodelType, viewType>(_key, InputViewModel, mode);
        }

        public virtual string Register<viewmodelType, viewType>(string key, viewmodelType InputViewModel = null, RegisterMode mode = RegisterMode.Singleton)
            where viewmodelType : class, BaseViewModelType
            where viewType : BaseViewType
        {
            try
            {
                //First add the internal main mappings.
                if (main_mapping.ContainsKey(key) == true)
                {
                    throw new ArgumentException($@"Key : {key} is already registered to - VM : {main_mapping[key].VMtype.GetType()} and View : {main_mapping[key].ViewType.GetType()}");
                }

                var _tuple = (typeof(viewmodelType), typeof(viewType), mode);
                main_mapping.TryAdd(key, _tuple);

                //Register this in the DI only if it is singleton
                if (mode == RegisterMode.Singleton)
                {
                    var _status = _di_instance.CheckIfRegistered(typeof(viewmodelType), null);
                    if (!_status.status)
                    {
                        _di_instance.Register<viewmodelType>(InputViewModel);
                    }
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

        protected (BaseViewModelType view_model, BaseViewType view) _generateValuePair(string key, ResolveMode mode)
        {
            var _mapping_value = GetMappingValue(key);

            //Generate a View
            BaseViewType resultcontrol = _generateView(_mapping_value.view_type);
            BaseViewModelType resultViewModel = _generateViewModel(_mapping_value.viewmodel_type, mode);
            return (resultViewModel, resultcontrol);
        }

        protected BaseViewType _generateView(Type viewType)
        {
            //Even view should be resolved by _di instance. because sometimes, views can direclty expect some 
            if (viewType == null) return default(BaseViewType);

            BaseViewType resultcontrol = (BaseViewType)_di_instance.Resolve(viewType);
            return resultcontrol;
        }

        protected BaseViewModelType _generateViewModel(Type viewModelType, ResolveMode mode = ResolveMode.AsRegistered) //If required we can even return the actural viewmodel concrete type as well.
        {
            try
            {
                BaseViewModelType _result;
                if (viewModelType == null) return default(BaseViewModelType);
                //If the viewmodel is registered in DI as a singleton, then it willbe returned, else, DI will resolve it as a transient and will return the result.
                _result = (BaseViewModelType)_di_instance.Resolve(viewModelType, mode);
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
        public BaseViewType GenerateView<viewmodelType>(viewmodelType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered) where viewmodelType : class, BaseViewModelType
        {
            string _key = typeof(viewmodelType).ToString();
            return GenerateView(_key, InputViewModel, mode);
        }
        public BaseViewType GenerateView<viewType>(object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered) where viewType : class, BaseViewType
        {
            string _key = typeof(viewType).ToString();
            return GenerateView(_key, InputViewModel, mode);
        }
        public BaseViewType GenerateView(Enum @enum, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered)
        {
            //Get the enum value and its type name to prepare a string
            string _key = @enum.getKey();
            return GenerateView(_key, InputViewModel, mode);
        }
        public abstract BaseViewType GenerateView(string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered);
        
        #endregion

        #region VM Retrieval Methods
        public (Type viewmodel_type, Type view_type, RegisterMode registered_mode) GetMappingValue(Enum @enum)
        {
            //Get the enum value and its type name to prepare a string
            string _key = @enum.getKey();
            return GetMappingValue(_key);
        }
        public (Type viewmodel_type, Type view_type, RegisterMode registered_mode) GetMappingValue(string key)
        {
            if (main_mapping.Count == 0 || !main_mapping.ContainsKey(key))
            {
                throw new ArgumentException($"Key {key} is not registered to any controls. Please check.");
            }

            (Type _viewmodel_type, Type _view_type, RegisterMode _mode) _registered_tuple = (null, null, RegisterMode.Singleton);
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
        public BaseViewModelType GenerateViewModel(Enum @enum, ResolveMode mode = ResolveMode.AsRegistered)
        {
            //Get the enum value and its type name to prepare a string
            string _key = @enum.getKey();
            BaseViewModelType _result = GenerateViewModel(_key, mode);
            return _result;
        }
        public BaseViewModelType GenerateViewModel(string key, ResolveMode mode = ResolveMode.AsRegistered) //If required we can even return the actural viewmodel concrete type as well.
        {
            var _mapping_value = GetMappingValue(key);
            return _generateViewModel(_mapping_value.viewmodel_type, mode);
        }

        public string FindKey(Type target_type)
        {
            //For the given target type, find if it is present in the mapping values. if found, return the first key.
            var _kvp = main_mapping.FirstOrDefault(kvp => kvp.Value.VMtype == target_type || kvp.Value.ViewType == target_type);
            if (_kvp.Value.VMtype == null && _kvp.Value.ViewType == null) return null;
            return _kvp.Key;
        }

        public bool ContainsKey(String key)
        {
            return main_mapping.ContainsKey(key);
        }

        #endregion
    }
}


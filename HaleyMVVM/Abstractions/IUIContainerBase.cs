using Haley.Enums;
using System;

namespace Haley.Abstractions
{
    public interface IUIContainerBase<BaseVMType,BaseViewType> 
    {
        string Id { get;}

        #region registration methods
        string Register<VMType, ViewType>(VMType InputViewModel=null, bool use_vm_as_key = true, RegisterMode mode = RegisterMode.Singleton)
            where VMType : class, BaseVMType
            where ViewType : BaseViewType;
        string Register<VMType, ViewType>(string key, VMType InputViewModel=null, RegisterMode mode = RegisterMode.Singleton)
            where VMType : class, BaseVMType
            where ViewType : BaseViewType;
        string Register<VMType, ViewType>(Enum key, VMType InputViewModel=null, RegisterMode mode = RegisterMode.Singleton)
            where VMType : class, BaseVMType
            where ViewType : BaseViewType;

        #endregion

        #region View Generation Methods
        BaseViewType GenerateView<VMType>(VMType InputViewModel=null, ResolveMode mode = ResolveMode.AsRegistered) where VMType : class, BaseVMType;
        BaseViewType GenerateView<ViewType>(object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered) where ViewType : class, BaseViewType;
        BaseViewType GenerateView(string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered);
        BaseViewType GenerateView(Enum key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered);
        #endregion

        #region ViewModel Generation methods
        BaseVMType GenerateViewModel(Enum @enum, ResolveMode mode = ResolveMode.AsRegistered);
        BaseVMType GenerateViewModel(string key, ResolveMode mode = ResolveMode.AsRegistered);
        (Type viewmodel_type, Type view_type, RegisterMode registered_mode) GetMappingValue(Enum @enum);
        (Type viewmodel_type, Type view_type, RegisterMode registered_mode) GetMappingValue(string key);
        string FindKey(Type target_type);
        bool ContainsKey(string key);
        #endregion
    }
}

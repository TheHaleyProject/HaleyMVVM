using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Enums;

namespace Haley.Abstractions
{
    public interface IHaleyUIContainer<BaseVMType,BaseViewType> 
    {
        #region registration methods
        string register<VMType, ViewType>(VMType InputViewModel=null, bool use_vm_as_key = true, RegisterMode mode = RegisterMode.Singleton)
            where VMType : class, BaseVMType
            where ViewType : BaseViewType;
        string register<VMType, ViewType>(string key, VMType InputViewModel=null, RegisterMode mode = RegisterMode.Singleton)
            where VMType : class, BaseVMType
            where ViewType : BaseViewType;
        string register<VMType, ViewType>(Enum key, VMType InputViewModel=null, RegisterMode mode = RegisterMode.Singleton)
            where VMType : class, BaseVMType
            where ViewType : BaseViewType;

        #endregion

        #region View Generation Methods
        BaseViewType generateView<VMType>(VMType InputViewModel=null, ResolveMode mode = ResolveMode.AsRegistered) where VMType : class, BaseVMType;
        BaseViewType generateView(string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered);
        BaseViewType generateView(Enum key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered);
        #endregion

        #region ViewModel Generation methods
        BaseVMType generateViewModel(Enum @enum, ResolveMode mode = ResolveMode.AsRegistered);
        BaseVMType generateViewModel(string key, ResolveMode mode = ResolveMode.AsRegistered);
        (Type viewmodel_type, Type view_type, RegisterMode registered_mode) getMappingValue(Enum @enum);
        (Type viewmodel_type, Type view_type, RegisterMode registered_mode) getMappingValue(string key);
        string findKey(Type target_type);
        #endregion
    }
}

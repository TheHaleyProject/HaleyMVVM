using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Haley.Enums;

namespace Haley.Abstractions
{
    public interface IHaleyWindowContainer<BaseVMType, BaseViewType> : IHaleyUIContainer<BaseVMType, BaseViewType>
    {
        #region ShowDialog Methods
        bool? showDialog<VMType>(VMType InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered) where VMType : class, BaseVMType;
        bool? showDialog<ViewType>(ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewType : BaseViewType;
        bool? showDialog(string key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered);
        bool? showDialog(Enum key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered);

        #endregion

        #region Show Methods
        void show<VMType>(VMType InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered) where VMType : class, BaseVMType;
        void show<ViewType>(ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewType : BaseViewType;
        void show(string key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered);
        void show(Enum key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered);
        #endregion
    }
}

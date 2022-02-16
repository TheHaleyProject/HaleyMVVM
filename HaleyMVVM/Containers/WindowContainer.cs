using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Abstractions;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Threading;
using Haley.Models;
using Haley.Utils;
using Haley.Enums;
using System.Windows;

namespace Haley.IOC
{
    public sealed class WindowContainer : UIContainerBase<IHaleyVM>, IWindowContainer  //Implementation of the DialogService Interface.
    {
        public WindowContainer(IServiceProvider _injection_container) : base(_injection_container) { }

        #region ShowDialog Methods
        public bool? ShowDialog(Enum key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            string _key = key.GetKey();
            return ShowDialog(_key, InputViewModel, resolve_mode);
        }
        public bool? ShowDialog<ViewModelType>(ViewModelType InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewModelType : class, IHaleyVM
        {
            string _key = typeof(ViewModelType).ToString();
            return ShowDialog(_key, InputViewModel, resolve_mode);
        }
        public bool? ShowViewDialog<ViewType>(ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewType : class
        {
            if (!(typeof(ViewType).BaseType == typeof(Window) || typeof(ViewType) == typeof(Window)))
            {
                throw new ArgumentException("Works only for objects with base type Window");
            }
            string _key = typeof(ViewType).ToString();
            return ShowDialog(_key, null, resolve_mode);
        }
        public bool? ShowDialog(string key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            return _invokeDisplay(key, InputViewModel, resolve_mode, is_modeless: false); //This is modal
        }
        #endregion

        #region Show Methods
        public void Show<ViewModelType>(ViewModelType InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewModelType : class, IHaleyVM
        {
            string _key = typeof(ViewModelType).ToString();
            Show(_key, InputViewModel, resolve_mode);
        }
        public void ShowView<ViewType>(ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewType : class
        {
            if (!(typeof(ViewType).BaseType == typeof(Window) || typeof(ViewType) == typeof(Window)))
            {
                throw new ArgumentException("Works only for objects with base type Window");
            }
            string _key = typeof(ViewType).ToString();
            Show(_key, null, resolve_mode);
        }
        public void Show(Enum key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            string _key = key.GetKey();
            Show(_key, InputViewModel, resolve_mode);
        }
        public void Show(string key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            _invokeDisplay(key, InputViewModel, resolve_mode, is_modeless: true); //This is modeless
        }

        #endregion

        #region Overridden Methods
        public override object GenerateViewFromKey(object key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered)
        {
            try
            {
                if (!getKey(key, out var _key)) return null;
                //If input view model is not null, then don't try to generate viewmodel.
                Window _view = null;
                IHaleyVM _vm = null;
                if (InputViewModel != null)
                {
                    var _mapping_value = GetMappingValue(_key);
                    _view = _generateView(_mapping_value.view_type,mode) as Window;
                    _vm =  (IHaleyVM) InputViewModel;
                }
                else
                {
                    var _kvp = _generateValuePair(_key, mode);
                    _view = _kvp.view as Window;
                    _vm = _kvp.view_model;
                }
                if (_view == null) return null;
                _view.DataContext = _vm;
                //Enable Haleyobserver so that when view closes, viewmodel event is triggered.
                WindowObserver CustomOP = new WindowObserver(_view, _vm);
                return _view;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        #endregion

        #region Private Methods
        private bool? _invokeDisplay(string key, object InputViewModel, ResolveMode resolve_mode , bool is_modeless)
        {
            bool? _result = null;

            //If Thread is not STA
            if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
            {
                Thread new_ui_thread = new Thread(() =>
                {
                    Window _hwindow = GenerateViewFromKey(key, InputViewModel, resolve_mode) as Window;
                    _result = _displayWindow(_hwindow, is_modeless);
                });
                new_ui_thread.SetApartmentState(ApartmentState.STA);
                new_ui_thread.Start();
                new_ui_thread.Join();
            }
            else
            {
                Window _hwindow = GenerateViewFromKey(key, InputViewModel, resolve_mode) as Window;
                _result = _displayWindow(_hwindow, is_modeless);
            }

            return _result;
        }
        
        private bool? _displayWindow(Window _hwindow, bool is_modeless)
        {
            bool? _result = false;
            if (_hwindow != null)
            {
                if(is_modeless)
                {
                    _hwindow.Show(); //Modeless
                }
                else
                {
                    _result = _hwindow.ShowDialog(); //Modal
                }
            }
            return _result;
        }

        #endregion
      
    }
}

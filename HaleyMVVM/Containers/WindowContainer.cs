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

namespace Haley.IOC
{
    public sealed class WindowContainer : UIContainerBase<IHaleyVM,IHaleyWindow>, IHaleyWindowContainer<IHaleyVM, IHaleyWindow>  //Implementation of the DialogService Interface.
    {
        public WindowContainer(IHaleyDIContainer _injection_container) : base(_injection_container) { }

        #region ShowDialog Methods
        public bool? showDialog(Enum key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            string _key = key.getKey();
            return showDialog(_key, InputViewModel, resolve_mode);
        }
        public bool? showDialog<ViewModelType>(ViewModelType InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewModelType : class, IHaleyVM
        {
            string _key = typeof(ViewModelType).ToString();
            return showDialog(_key, InputViewModel, resolve_mode);
        }
        public bool? showDialog<ViewType>(ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewType : IHaleyWindow
        {
            string _key = typeof(ViewType).ToString();
            return showDialog(_key, null, resolve_mode);
        }
        public bool? showDialog(string key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            return _invokeDisplay(key, InputViewModel, resolve_mode, is_modeless: false); //This is modal
        }
        #endregion

        #region Show Methods
        public void show<ViewModelType>(ViewModelType InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewModelType : class, IHaleyVM
        {
            string _key = typeof(ViewModelType).ToString();
            show(_key, InputViewModel, resolve_mode);
        }
        public void show<ViewType>(ResolveMode resolve_mode = ResolveMode.AsRegistered) where ViewType : IHaleyWindow
        {
            string _key = typeof(ViewType).ToString();
            show(_key, null, resolve_mode);
        }
        public void show(Enum key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            string _key = key.getKey();
            show(_key, InputViewModel, resolve_mode);
        }
        public void show(string key, object InputViewModel = null, ResolveMode resolve_mode = ResolveMode.AsRegistered)
        {
            _invokeDisplay(key, InputViewModel, resolve_mode, is_modeless: true); //This is modeless
        }

        #endregion

        #region Overridden Methods
        public override IHaleyWindow generateView(string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered)
        {
            try
            {
                //If input view model is not null, then don't try to generate viewmodel.
                IHaleyWindow _view = null;
                IHaleyVM _vm = null;
                if (InputViewModel != null)
                {
                    var _mapping_value = getMappingValue(key);
                    _view = _generateView(_mapping_value.view_type);
                    _vm =  (IHaleyVM) InputViewModel;
                }
                else
                {
                    var _kvp = _generateValuePair(key, mode);
                    _view = _kvp.view;
                    _vm = _kvp.view_model;
                }
                _view.DataContext = _vm;

                //Enable Haleyobserver so that when view closes, viewmodel event is triggered.
                HaleyObserver CustomOP = new HaleyObserver(_view, _vm);
                return _view;
            }
            catch (Exception ex)
            {
                throw ex;
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
                    IHaleyWindow _hwindow = generateView(key, InputViewModel, resolve_mode);
                    _result = _displayWindow(_hwindow, is_modeless);
                });
                new_ui_thread.SetApartmentState(ApartmentState.STA);
                new_ui_thread.Start();
                new_ui_thread.Join();
            }
            else
            {
                IHaleyWindow _hwindow = generateView(key, InputViewModel, resolve_mode);
                _result = _displayWindow(_hwindow, is_modeless);
            }

            return _result;
        }
        
        private bool? _displayWindow(IHaleyWindow _hwindow, bool is_modeless)
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

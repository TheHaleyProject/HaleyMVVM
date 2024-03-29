﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Abstractions;
using Haley.Utils;
using System.Collections.Concurrent;
using Haley.Enums;
using System.Windows;
using System.Windows.Controls;
using Haley.Models;

namespace Haley.IOC
{
    public sealed class ControlContainer : UIContainerBase<IHaleyVM,UserControl>, IControlContainer 
    {
        public ControlContainer(IServiceProvider _injection_container):base(_injection_container) { }

        public override UserControl GenerateView(string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered)
        {
            try
            {
                //If input view model is not null, then don't try to generate viewmodel.
                UserControl _view = null;
                IHaleyVM _vm = null;
                if (InputViewModel != null)
                {
                    var _mapping_value = GetMappingValue(key);
                    _view = _generateView(_mapping_value.view_type,mode);
                    _vm = (IHaleyVM)InputViewModel;
                }
                else
                {
                    var _kvp = _generateValuePair(key, mode);
                    _view = _kvp.view;
                    _vm = _kvp.view_model;
                }
                _view.DataContext = _vm;
                //Before returning the view, setup a control observer.
                var _observer = new ControlObserver(_view, _vm);
                return _view;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}


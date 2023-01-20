﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Haley.Models;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Markup;
using System.ComponentModel;

namespace Haley.Utils {
    public class DataTriggerExtension : TriggerExtension {

        //Inspired from :  https://stackoverflow.com/questions/42088734/create-attached-property-dynamically
        public new Binding Path { get; set; }
        Action<object> handler;
        public DataTriggerExtension() {
            handler = HandleCallback;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            if (!GetTarget(serviceProvider)) return FallBack;

            if (FallBack == null) {
                //Try to get the default value
                FallBack = _target.TargetObject.GetValue(_target.TargetProperty);
            }

            InitiateValueProvider();

            var dpTupple = GetDependencyProperty(_target.TargetProperty.Name);
            if (dpTupple.dp == null) return FallBack;
            BindingOperations.SetBinding(_target.TargetObject, dpTupple.dp, Path); //Set original binding
            _target.TargetObject.SetValue(dpTupple.dpCB, handler); //Set the handler callback

            //Do the first comparison
            _valueProvider.ChangeValue(FirstCompare(dpTupple.dp));

            //Create and send a binding
            var binding = CreateBinding();
            return binding.ProvideValue(serviceProvider);
        }

        private void HandleCallback(object obj) {
            //In this context, the target object might be valid for all calls but property might not be valid. So handle with care.
            bool? compare = obj?.ToString().Equals(Value?.ToString(), ValueComparision);
            if (compare.HasValue && compare.Value) {
                _valueProvider.ChangeValue(OnSuccess);
            } else {
                _valueProvider.ChangeValue(FallBack);
            }
        }

        object FirstCompare(DependencyProperty dp) {
            bool? compare = _target.TargetObject.GetValue(dp).ToString().Equals(Value?.ToString(),ValueComparision);
            if (compare.HasValue && compare.Value) return OnSuccess;
            return FallBack;
        }

        (DependencyProperty dp, DependencyProperty dpCB)  GetDependencyProperty(string propname) {
            DependencyProperty dp, dpCallback = null;
            dp = MarkupBindingAP.DPFetcher?.Invoke(propname.ToUpper(), typeof(MarkupBindingAP));
            dpCallback = MarkupBindingAP.DPFetcher?.Invoke(propname.ToUpper() + MarkupBindingAP.CALLBACK_PREFIX, typeof(MarkupBindingAP));
            if (dp == null) {
                CreateDependencyProperty(propname);
                dp = MarkupBindingAP.DPFetcher?.Invoke(propname.ToUpper(), typeof(MarkupBindingAP));
                dpCallback = MarkupBindingAP.DPFetcher?.Invoke(propname.ToUpper() + MarkupBindingAP.CALLBACK_PREFIX, typeof(MarkupBindingAP));
            }
            return (dp, dpCallback); //We try to create dp only once.
        }

        void CreateDependencyProperty(string propname) {
            try {
                DependencyProperty.RegisterAttached(propname.ToUpper(), typeof(object), typeof(MarkupBindingAP), new PropertyMetadata(null, propertyChangedCallback: MarkupBindingAP.PropChanged)); //Propname should be case-insensitive. Keep everything upper
                DependencyProperty.RegisterAttached(propname.ToUpper()+MarkupBindingAP.CALLBACK_PREFIX, typeof(Action<object>), typeof(MarkupBindingAP), new PropertyMetadata(null));
            } catch (Exception) {
                throw;
            }
        }
    }
}
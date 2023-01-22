using System;
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
        DependencyProperty dp;
        Action<object> handler;
        public DataTriggerExtension() {
            handler = HandleCallback;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            if (!Initialize(serviceProvider)) return FallBack;

            //Set updatesource trigger
            Path.NotifyOnSourceUpdated = true;
            Path.NotifyOnTargetUpdated = true;
            Path.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            dp = GetDependencyProperty(_target.TargetProperty.Name,typeof(object),MarkupBindingAP.PropChanged); //only when the property changed, we monitor using propchanged.
            if (dp == null) return FallBack;
            var dpCB = GetDependencyProperty(_target.TargetProperty.Name+"."+MarkupBindingAP.CALLBACK_PREFIX, typeof(Action<object>));
            BindingOperations.SetBinding(_target.TargetObject, dp, Path); //Set original binding
            _target.TargetObject.SetValue(dpCB, handler); //Set the handler callback

            //Do the first comparison
            _valueProvider.ChangeValue(FirstCompare(dp));

            //Create and send a binding
            var binding = CreateBinding();
            return binding.ProvideValue(serviceProvider);
        }

        private void HandleCallback(object obj) {
            ChangeReturnvalue(obj);
        }

        protected override void CompareValueChanged() {
            var source = _target.TargetObject.GetValue(dp);
            ChangeReturnvalue(source);
        }

        protected override void ChangeReturnvalue(object source_value) {
            _valueProvider.ChangeValue(GetReturnValue(source_value));
        }

        object FirstCompare(DependencyProperty dp) {
            var source = _target.TargetObject.GetValue(dp);
            return GetReturnValue(source);
        }
    }
}
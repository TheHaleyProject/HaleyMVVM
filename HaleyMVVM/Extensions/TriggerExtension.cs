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
using Haley.Abstractions;
using Dwg = System.Drawing;

namespace Haley.Utils {
    public class TriggerExtension : MarkupExtension {
        protected MarkupValueProvider _valueProvider;
        protected DependencyElement _target;
        
        PropertyInfo _sourceProp; //Only for trigger. For datatrigger, we use another element's prop as source

        private CompareValueType _compareType = CompareValueType.Object;
        public CompareValueType CompareType {
            get { return _compareType; }
            set { _compareType = value; }
        }

        public string Path { get; set; }
        public object Value { get; set; }
        public StringComparison ValueComparision  = StringComparison.OrdinalIgnoreCase;
        public object OnSuccess { get; set; }
        public object FallBack { get; set; }

        public TriggerExtension() {

        }

        protected void InitiateValueProvider() {
            if (_valueProvider == null) {
                _valueProvider = new MarkupValueProvider(FallBack);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) {

            if (!GetTarget(serviceProvider)) return FallBack;

            if (FallBack == null) {
                //Try to get the default value
                FallBack = _target.TargetObject.GetValue(_target.TargetProperty);
            }


            var targetObj = _target.TargetObject; //Set up the targetObj.

            InitiateValueProvider();

            //Check if the targetObj object has a property with this name.
            var all_props = targetObj.GetType().GetProperties();
            //var from_name = DependencyPathDescriptor.FromName(Path, targetObj.TargetObject.GetType(), targetObj.TargetObject.GetType());
            _sourceProp = all_props.FirstOrDefault(p => p.Name.ToLower() == Path.ToLower());

            do {
                if (_sourceProp == null) break;
                var propDesc = TypeDescriptor.GetProperties(_sourceProp.DeclaringType)?[_sourceProp.Name];
                if (propDesc == null) break;
                var d_propDesc = DependencyPropertyDescriptor.FromProperty(propDesc);
                if (d_propDesc == null) break;

                //Setup monitoring
                d_propDesc.RemoveValueChanged(targetObj, propchanged);
                d_propDesc.AddValueChanged(targetObj, propchanged);

                //Do the first comparison
                _valueProvider.ChangeValue(CompareAndGet(targetObj));

                //Create and send a binding
                var binding = CreateBinding();
                return binding.ProvideValue(serviceProvider);
            } while (false);
            return FallBack;
        }

        protected bool GetTarget(IServiceProvider serviceProvider) {
            if (CommonUtils.GetTargetElement(serviceProvider, out var targetObj)) {
                _target = targetObj;
                return true; 
            }
            return false;
        }

        object CompareAndGet(object sender) {
            var source_val = _sourceProp?.GetValue(sender);
            return GetReturnValue(source_val);
        }

        protected object GetReturnValue(object source) {
            if (source == null) return FallBack;
            bool? compare = null;
            object checking_value = Value;

            //At present Static/Dynamic resources are not parsed since they can only be set on dependency properties.
            if (Value is DynamicResourceExtension dyRsr) {
                checking_value = _target.TargetObject.TryFindResource(dyRsr.ResourceKey);
            } else if (Value is StaticResourceExtension sRsr) {
                checking_value = _target.TargetObject.TryFindResource(sRsr.ResourceKey);
            }

            switch (CompareType) {
                case CompareValueType.Color:
                    var clr = (Color)ColorConverter.ConvertFromString(checking_value.ToString());
                    compare = clr.ToString().ToLower().Equals(source.ToString(),StringComparison.OrdinalIgnoreCase); //For color check ignore case
                    break;
                default:
                    compare = source?.ToString().Equals(checking_value?.ToString(), ValueComparision);
                    break;
            }

            if (compare.HasValue && compare.Value) return OnSuccess;
            return FallBack;
        }

        protected Binding CreateBinding() {
            var binding = new Binding("Value") {
                Source = _valueProvider
            };
            return binding;
        }

        void propchanged(object sender, EventArgs e) {
            _valueProvider.ChangeValue(CompareAndGet(sender)); //This will eventually trigger the NotifyPropertyChanged.
        }
    }
}
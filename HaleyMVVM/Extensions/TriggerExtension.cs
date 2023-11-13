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
        protected object _compareValue;
        protected Dictionary<string, PropertyInfo> _targetObjectProps = new Dictionary<string, PropertyInfo>();

        private CompareTargetType _compareType = CompareTargetType.Object;
        public CompareTargetType CompareType {
            get { return _compareType; }
            set { _compareType = value; }
        }
        public bool SelfBindValue { get; set; }
        public string Path { get; set; } //Currently this is referring to property value. What if this is a binding??
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

        protected bool Initialize(IServiceProvider serviceProvider) {
            if (!GetTarget(serviceProvider)) return false;

            if (FallBack == null) {
                //Try to get the default value
                FallBack = _target.TargetObject.GetValue(_target.TargetProperty);
            }

            InitiateValueProvider();

            _compareValue = Value;
            //Also set up the Value checks
            if (SelfBindValue) {
                
                //Then value itself is not a string, it is coming from Relative Source.
                if (MonitorPropChange(_target.TargetObject, Value.ToString(), CompareValueChanged,out var compareProp)) {
                    if (!_targetObjectProps.ContainsKey(Value.ToString().ToLower())) {
                        _targetObjectProps.Add(Value.ToString().ToLower(), compareProp);
                    }
                    _compareValue = compareProp?.GetValue(_target.TargetObject);
                }
            }
            return true;
        }

        protected bool MonitorPropChange(object targetObj, string propName,EventHandler propChangeHandler,out PropertyInfo sourceProp) {
            var all_props = targetObj.GetType().GetProperties();
            //var from_name = DependencyPathDescriptor.FromName(Path, targetObj.TargetObject.GetType(), targetObj.TargetObject.GetType());
            sourceProp = all_props.FirstOrDefault(p => p.Name.ToLower() == propName.ToLower());

            do {
                if (sourceProp == null) break;
                var propDesc = TypeDescriptor.GetProperties(sourceProp.DeclaringType)?[sourceProp.Name];
                if (propDesc == null) break;
                var d_propDesc = DependencyPropertyDescriptor.FromProperty(propDesc);
                if (d_propDesc == null) break;

                //Setup monitoring
                d_propDesc.RemoveValueChanged(targetObj, propChangeHandler);
                d_propDesc.AddValueChanged(targetObj, propChangeHandler);
                return true;
            } while (false);
            return false;
         }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            
            if (!Initialize(serviceProvider)) return FallBack;

            if (!MonitorPropChange(_target.TargetObject, Path, propchanged,out var sourceProp)) return FallBack;

            if (!_targetObjectProps.ContainsKey(sourceProp.Name.ToLower())) {
                _targetObjectProps.Add(sourceProp.Name.ToLower(), sourceProp);
            }
            //Do the first comparison
            ChangeReturnvalue(_target.TargetObject);

            //Create and send a binding
            var binding = CreateBinding();
            return binding.ProvideValue(serviceProvider);
        }

        protected bool GetTarget(IServiceProvider serviceProvider) {
            if (CommonUtils.GetTargetElement(serviceProvider, out var targetObj)) {
                _target = targetObj;
                return true; 
            }
            return false;
        }

        object CompareAndGet(object sender) {
            var source_val = _targetObjectProps[Path.ToLower()]?.GetValue(sender);
            return GetReturnValue(source_val);
        }

        protected virtual void CompareValueChanged() {
            ChangeReturnvalue(_target.TargetObject); //We use 
        }

        protected virtual void ChangeReturnvalue(object sender) {
            _valueProvider.ChangeValue(CompareAndGet(sender));
        }

        protected object GetReturnValue(object source) {
            if (source == null) return FallBack;
            bool? compare = null;
            object checking_value = _compareValue;


            //At present Static/Dynamic resources are not parsed since they can only be set on dependency properties.
            if (SelfBindValue) {
                compare = source?.ToString().Equals(checking_value?.ToString(), ValueComparision);
            } else {

                if (Value is DynamicResourceExtension dyRsr) {
                    checking_value = _target.TargetObject.TryFindResource(dyRsr.ResourceKey);
                } else if (Value is StaticResourceExtension sRsr) {
                    checking_value = _target.TargetObject.TryFindResource(sRsr.ResourceKey);
                }
                switch (CompareType) {
                    case CompareTargetType.Color:
                        var clr = (Color)ColorConverter.ConvertFromString(checking_value.ToString());
                        compare = clr.ToString().ToLower().Equals(source.ToString(), StringComparison.OrdinalIgnoreCase); //For color check ignore case
                        break;
                    default:
                        compare = source?.ToString().Equals(checking_value?.ToString(), ValueComparision);
                        break;
                }
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
            ChangeReturnvalue(sender);
        }

        void CompareValueChanged(object sender, EventArgs e) {
            //Do a clean up to remove past values as well.

            if (!SelfBindValue) return;
            var compareKey = Value.ToString().ToLower();
            if (_targetObjectProps.ContainsKey(compareKey)) {
                _compareValue = _targetObjectProps[compareKey]?.GetValue(_target.TargetObject);
            }
            CompareValueChanged(); //This should invoke differently
        }

        protected DependencyProperty GetDependencyProperty(string propname, Type propType =null, PropertyChangedCallback callback =null) {
            DependencyProperty dp = null;
            dp = MarkupBindingAP.DPFetcher?.Invoke(propname.ToUpper(), typeof(MarkupBindingAP));
            
            if (dp == null) {
                if (propType == null) {
                    throw new ArgumentException("PropType Cannot be null when creating a dependency property");
                }
                CreateDependencyProperty(propname, propType, callback);
                dp = MarkupBindingAP.DPFetcher?.Invoke(propname.ToUpper(), typeof(MarkupBindingAP));
            }
            return dp; //We try to create dp only once.
        }

        protected void CreateDependencyProperty(string propname, Type propType,PropertyChangedCallback callback) {
            try {
                DependencyProperty.RegisterAttached(propname.ToUpper(), propType, typeof(MarkupBindingAP), new PropertyMetadata(null, propertyChangedCallback: callback)); //Propname should be case-insensitive. Keep everything upper
            } catch (Exception) {
                throw;
            }
        }
    }
}
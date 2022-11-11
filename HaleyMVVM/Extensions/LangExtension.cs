using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Haley.Models;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Markup;

namespace Haley.Utils
{
    public class LangExtension : MarkupExtension
    {
        DependencyElement _target;
        private string _resourceKey;
        //[ConstructorArgument("key")] //This attribute has no value. It is not affected by any means.
        public string ResourceKey
        {
            get { return _resourceKey; }
            set { _resourceKey = value; }
        }

        public string ProviderKey { get; set; } //not mandatory. This is to be used when the provider is from a different assembly.

        public LangExtension(string key)
        {
            _resourceKey = key;
        }

        public string BindingSource { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            //THIS WHOLE PART IS JUST A MIDDLE WARE. WHEN THE XAML REQUESTS FOR A VALUE USING "PROVIDE VALUE" METHOD OF THE EXTENSION, WE MERELY CREATE A NEW PROPERTY BINDING AND USE THE NEW BINDING'S PROVIDE VALUE.

            //PROCESS PROVIDER KEY
            string provider_key = ProviderKey;
            string resource_key = _resourceKey;

            if (string.IsNullOrWhiteSpace(provider_key)) {
                //It should always be left null. But in case, it is not left null, then use this.
                Uri callerUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext)))?.BaseUri;
                if (callerUri != null) {
                    var _splitted = callerUri.Segments[1].Split(new string[] { ";component" }, StringSplitOptions.None);
                    if (_splitted.Length > 1) {
                        //We need more than one
                        var _asmName = _splitted[0]; //Remove the backslash.
                        if (!string.IsNullOrWhiteSpace(_asmName)) {
                            provider_key = _asmName;
                        }
                    }
                }
            }

            //VALIDATE BINDING SOURCE
            if (!string.IsNullOrWhiteSpace(BindingSource)) {
                if (InternalUtilsCommon.GetTargetElement(serviceProvider, out var target)) {
                    target.TargetObject.DataContextChanged -= TargetDataChanged;
                    target.TargetObject.DataContextChanged += TargetDataChanged; //To receive the property changes during runtime
                    //Change resource_key based on the binding value.
                }
            }


            //NOW CREATE A NEW BINDING AND SET A NEW PROVIDE VALUE.
            var binding = CreateBinding(serviceProvider, provider_key,resource_key);
            return binding.ProvideValue(serviceProvider);
        }

        private void TargetDataChanged(object sender, DependencyPropertyChangedEventArgs e) {
            //object propValue = e.NewValue;
            //try {
            //    //To receive message whenever the property value is changed.
            //    //Since we are dealing with DataContextChange, we will always get DataContext Property
            //    //If Binding Source is "." then we directly bind the property. So, don't process or validate.
            //    if (e.NewValue != null && !(e.NewValue is string || e.NewValue is Enum) && BindingSource != ".") {
            //        propValue = InternalUtilsCommon.FetchValueAndMonitor(e.NewValue, BindingSource, ObjectPropertyChanged);
            //    }
            //} catch (Exception) { }
            //_sourceProvider.OnDataChanged(propValue); //this will be the new data.
        }

        Binding CreateBinding(IServiceProvider serviceProvider,string provider_key,string resource_key) {
            //NOW CREATE A NEW BINDING AND SET A NEW PROVIDE VALUE.
            var binding = new Binding("Value") //ResourceData contains the property "Value" which will return the translated value for the given key.
            {
                Source = new ResourceData(resource_key, provider_key)
            };
            return binding;
        }
    }
}
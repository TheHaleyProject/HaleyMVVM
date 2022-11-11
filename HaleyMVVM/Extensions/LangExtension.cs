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

            #region Option 1
            if (!string.IsNullOrWhiteSpace(BindingSource)) {
                //Binding takes top most priority
                //If binding is not null, we try to fetch the datacontext of the target element and then bind to the changes.
                if (HelperUtilsInternal.GetTargetElement(serviceProvider, out var target)) {
                    //create binding and then return the binding expression, rather than directly returning the value.
                    _target = target;
                    //_target.TargetObject.DataContextChanged += TargetDataChanged; //To receive the property changes during runtime
                    //var binding = CreateBinding();
                    //return binding.ProvideValue(serviceProvider); //This will provide the value of IconSource Property
                    ////If we directly add a binding to the property name, then whenever the value of that property changes, we will 
                }
            }
            #endregion

            string provider_key = ProviderKey;

            if (string.IsNullOrWhiteSpace(provider_key))
            {
                //It should always be left null. But in case, it is not left null, then use this.
                Uri callerUri = ((IUriContext)serviceProvider.GetService(typeof(IUriContext)))?.BaseUri;
                if (callerUri != null)
                {
                    //var _splitted = callerUri.AbsolutePath.Split(new string[] { ";component" }, StringSplitOptions.None); //It will be in FullPack URI format.
                    //if (_splitted.Length > 1)
                    //{
                    //    //We need more than one
                    //    var _asmName = _splitted[0].Substring(1, _splitted[0].Length - 1); //Remove the backslash.
                    //    if (!string.IsNullOrWhiteSpace(_asmName))
                    //    {
                    //        provider_key = _asmName;
                    //    }
                    //}

                    var _splitted = callerUri.Segments[1].Split(new string[] { ";component" }, StringSplitOptions.None); 
                    if (_splitted.Length > 1)
                    {
                        //We need more than one
                        var _asmName = _splitted[0]; //Remove the backslash.
                        if (!string.IsNullOrWhiteSpace(_asmName))
                        {
                            provider_key = _asmName;
                        }
                    }
                }
            }

            //NOW CREATE A NEW BINDING AND SET A NEW PROVIDE VALUE.
            var binding = CreateBinding(serviceProvider, provider_key);
            return binding.ProvideValue(serviceProvider);
        }

        Binding CreateBinding(IServiceProvider serviceProvider,string provider_key) {
            //NOW CREATE A NEW BINDING AND SET A NEW PROVIDE VALUE.
            var binding = new Binding("Value") //ResourceData contains the property "Value" which will return the translated value for the given key.
            {
                Source = new ResourceData(_resourceKey, provider_key)
            };
            return binding;
        }
    }
}
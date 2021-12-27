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
using Haley.Events;

namespace Haley.Utils
{
    public class LangUtils
    {
        #region Constructor
        public static LangUtils Singleton = new LangUtils();
        public static LangUtils getSingleton()
        {
            if (Singleton == null) Singleton = new LangUtils();
            return Singleton;
        }

        public static void Clear()
        {
            Singleton = new LangUtils();
        }
        private LangUtils() { }
        #endregion

        public event EventHandler<CultureChangedEventArgs> CultureChanged; //Raise this event whenever a culture changes. The subscribing methods can perform their own operations.

        private static List<ResourceProvider> _resourceProviders = new List<ResourceProvider>();
        public static CultureInfo CurrentCulture { get; private set; }

        public static void ChangeCulture(CultureInfo culture)
        {
            CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;

            Singleton.CultureChanged?.Invoke(null, new CultureChangedEventArgs(culture));
        }

        public static void ChangeCulture(string code)
        {
            CultureInfo _info = CultureInfo.InvariantCulture;
            if (!string.IsNullOrWhiteSpace(code))
            {
                _info = CultureInfo.CreateSpecificCulture(code);
            }
            ChangeCulture(_info);
        }

        public ResourceProvider GetProvider(string provider_key)
        {
            if (string.IsNullOrWhiteSpace(provider_key)) return null;
            //for the given key try to get the provider.
           return _resourceProviders.FirstOrDefault(p => p.Key == provider_key); //Could also be null.
        }
        public static string Translate(string resource_key, string provider_key)
        {
            //if (CurrentCulture == null) CurrentCulture = CultureInfo.CreateSpecificCulture("en");
            if (CurrentCulture == null) CurrentCulture = CultureInfo.InvariantCulture;
            //Get the provider.
            var _provider = _resourceProviders.FirstOrDefault(p => p.Key == provider_key);
            if (_provider == null) return resource_key; //Just return the key.
            try
            {
                var _converted_string = _provider.Manager.GetString(resource_key, CurrentCulture);

                if (string.IsNullOrWhiteSpace(_converted_string))
                {
                    _converted_string = resource_key;
                }
                if (_provider.TranslationOverride != null)
                {
                    return _provider.TranslationOverride.Invoke(resource_key, _converted_string, CurrentCulture) as string;
                }
                return _converted_string;
            }
            catch (Exception ex)
            {
                return resource_key;
            }
        }
        /// <summary>
        /// Register with Calling Assembly
        /// </summary>
        /// <param name="fully_qualified_resourceFileName">Should be a fully qualified name with namespace. Default: ##ProjectName##.Properties.Resources</param>
        /// <param name="CultureChangeCallBack">This delegate will be called whenever the culture changes.</param>
        /// <returns></returns>
        [MethodImplAttribute(MethodImplOptions.NoInlining)]
        public static ResourceProvider Register(string fully_qualified_resourceFileName = null, TranslationOverrideCallBack TranslationOverride = null)
        {
            //The calling method should have an assembly.
            Assembly assembly = Assembly.GetCallingAssembly();
            return Register(assembly, fully_qualified_resourceFileName, TranslationOverride);
        }

        public static ResourceProvider Register(Assembly assembly, string fully_qualified_resourceFileName = null, TranslationOverrideCallBack TranslationOverride = null)
        {
            ResourceProvider _newProvider = null;
            if (assembly != null)
            {
                var _name = assembly.GetName().Name;
                var _existingProvider = _resourceProviders.FirstOrDefault(p => p.Source == assembly && p.Key == _name);
                if (_existingProvider != null && _existingProvider.Manager != null) return _existingProvider;

                if (string.IsNullOrWhiteSpace(fully_qualified_resourceFileName))
                {
                    fully_qualified_resourceFileName = $@"{_name}.Properties.Resources";
                }
                //At present the resource handling is only dealing with Strings. New features might be added later.
                ResourceManager resourceManager = new ResourceManager(fully_qualified_resourceFileName, assembly);
                _newProvider = new ResourceProvider(assembly, resourceManager, TranslationOverride) { Key = _name};
                _resourceProviders.Add(_newProvider);
            }
            return _newProvider;
        }

        public static void UnRegister()
        {
            //The calling method should have an assembly.
            Assembly assembly = Assembly.GetCallingAssembly();
            UnRegister(assembly);
        }

        public static void UnRegister(Assembly assembly)
        {
            if (assembly != null)
            {
                var _name = assembly.GetName().Name;
                var _existingProvider = _resourceProviders.FirstOrDefault(p => p.Source == assembly && p.Key == _name);
                if (_existingProvider != null)
                {
                    _resourceProviders.Remove(_existingProvider);
                }
            }
        }
    }
}
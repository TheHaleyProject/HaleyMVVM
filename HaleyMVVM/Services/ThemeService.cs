using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using Haley.Models;
using System.Linq;
using System.Windows.Controls;
using Haley.MVVM;
using System.Collections.Concurrent;
using System.Reflection;

namespace Haley.Services
{
    public class ThemeService : IThemeService
    {
        #region Properties
        public event EventHandler<object> ThemeChanged;
        public object ActiveTheme
        {
            get { return _activeTheme; }
            private set
            {
                _activeTheme = value;
                ThemeChanged?.Invoke(this, _activeTheme);
            }
        }

        public InternalThemeMode? InternalTheme { get; private set; }

        #endregion

        #region Attributes
        private object _activeTheme;
        private IDialogService _ds = new DialogService();
        private const string _hw_absolute = "pack://application:,,,/Haley.WPF;component/";
        private const string _hm_absolute = "pack://application:,,,/Haley.MVVM;component/";
        private const string _hw_relative = "Haley.WPF;component/";
        private const string _hm_relative = "Haley.MVVM;component/";
        private const string _hw_theme_start = "pack://application:,,,/Haley.WPF;component/Dictionaries/ThemeColors/Theme";

        #region Dictionaries
        private ConcurrentDictionary<string, (List<ThemeInfoWrapper> identities, InternalThemeMode? internalTheme)> _themeDic = new ConcurrentDictionary<string, (List<ThemeInfoWrapper> identities, InternalThemeMode? internalTheme)>();
        private ConcurrentDictionary<Assembly, (ThemeChangeHandler handler, bool beforeChange)> _changeHandlers = new ConcurrentDictionary<Assembly, (ThemeChangeHandler handler, bool beforeChange)>();
        #endregion

        #region HaleyThemes
        private static Uri _hw_RD = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/haleyRD.xaml", UriKind.RelativeOrAbsolute);
        private static Uri _hw_base = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/haleyBase.xaml", UriKind.RelativeOrAbsolute);

        #endregion
        #endregion

        #region Constructors

        public static ThemeService Singleton = new ThemeService();
        public static ThemeService getSingleton()
        {
            if (Singleton == null) Singleton = new ThemeService();
            return Singleton;
        }

        public static void Clear()
        {
            Singleton = new ThemeService();
        }
        private ThemeService() { }

        #endregion

        #region Registrations
        public bool AttachCallBack(ThemeChangeHandler ChangeCallBack, bool callBeforeChange = false)
        {
            var asmbly = Assembly.GetCallingAssembly();
            return AttachCallBack(ChangeCallBack, asmbly, callBeforeChange);
        }
        public bool AttachCallBack(ThemeChangeHandler ChangeCallBack, Assembly targetAssembly, bool callBeforeChange = false)
        {
            if (targetAssembly == null)
            {
                throw new ArgumentNullException(nameof(targetAssembly));
            }
            if (_changeHandlers.ContainsKey(targetAssembly))
            {
                throw new ArgumentException($@"Call back for the assembly {targetAssembly.GetName().Name} is already registered. An assembly can have only one call back.");
            }
            return _changeHandlers.TryAdd(targetAssembly, (ChangeCallBack, callBeforeChange));
        }
        public bool AttachInternalTheme(object key, InternalThemeMode theme)
        {
            var _key = GetKey(key);

            if (!_themeDic.ContainsKey(_key))
            {
                _themeDic.TryAdd(_key, (new List<ThemeInfoWrapper>(), null));
            }

            _themeDic.TryGetValue(_key, out var themeData);
            themeData.internalTheme = theme;
            return true;
        }
        public List<ThemeInfo> GetThemeInfos(object key)
        {
            try
            {
                var _key = GetKey(key);
                if(_themeDic.ContainsKey(_key))
                {
                    _themeDic.TryGetValue(_key, out var themeData);
                    return themeData.identities?.Select(p => p.Theme)?.ToList();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<string> GetThemes()
        {
            try
            {
               return _themeDic?.Keys.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public bool Initiate(object startup_theme_key)
        {
            throw new NotImplementedException();
        }
        public bool IsThemeKeyRegistered(object key)
        {
            try
            {
                var _key = GetKey(key);
                return _themeDic.ContainsKey(_key);
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool Register(object key, ThemeInfo value)
        {
            var asmbly = Assembly.GetCallingAssembly();
            return Register(key, value, asmbly);
        }
        public bool Register(object key, ThemeInfo value, Assembly targetAssembly)
        {
            var _key = GetKey(key);
            if (targetAssembly == null)
            {
                throw new ArgumentNullException(nameof(targetAssembly));
            }
            if (!_themeDic.ContainsKey(_key))
            {
                InternalThemeMode? mode = InternalThemeMode.Normal;
                _themeDic.TryAdd(_key, (new List<ThemeInfoWrapper>(), mode));
            }

            _themeDic.TryGetValue(_key, out var themeData);

            if (themeData.identities == null)
            {
                lock(themeData.identities)
                {
                    themeData.identities = new List<ThemeInfoWrapper>();
                }
            }

            if (themeData.identities.Any(p => p.Source == targetAssembly))
            {
                throw new ArgumentException($@"A theme is already registered for the assembly {targetAssembly.GetName().Name} against key {_key}. An assembly should have unique themes for each key.");
            }

            lock (themeData.identities)
            {
                themeData.identities.Add(new ThemeInfoWrapper(value, targetAssembly));
            }

            return true;
        }
        #endregion

        #region ChangeTheme
        /// <summary>
        /// Change the theme to the new provided key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="showNotifications"></param>
        /// <returns></returns>
        public bool ChangeTheme(object key, bool showNotifications = false)
        {
            //This will only change the application level theme change which will also raise the theme changed event at last, which will be consumed by the controls (if they have subscribed to it).

            //The sender can never be null. Do remember to send the assembly as the sender.
            var _caller = Assembly.GetCallingAssembly();
            return ChangeTheme(_caller, key, showNotifications: showNotifications);
        }

        public bool ChangeTheme(object sender, object key, SearchPriority priority = SearchPriority.Application, bool compare_with_active_theme = true, bool showNotifications = false)
        {
           //Sender could be an assembly or a frameworke element. In case of either, we need to get the assembly from it.

            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            return true;
        }
        #endregion

        #region Helpers
        private string GetKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentException("The theme key cannot be empty. Please provide a valid key for processing.");
            }

            if (key is string keystr)
            {
                return keystr.ToLower();
            }
            return key.AsString()?.ToLower() ?? key.ToString();
        }
        #endregion

    }
}

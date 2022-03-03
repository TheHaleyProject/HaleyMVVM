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
using System.Diagnostics;

namespace Haley.Services
{
    public class ThemeService : IThemeService
    {
        #region Properties
        public event EventHandler<(object newTheme,object oldTheme)> ThemeChanged;
        /// <summary>
        /// Startup theme can be set only once and it will be used by controls on late join (to change their themes to match the Active theme).
        /// </summary>
        public object StartupTheme { get; private set; }
        public object ActiveTheme
        {
            get { return _activeTheme; }
            private set
            {
                _activeTheme = value;
            }
        }
        public ExceptionHandling ErrorHandling { get; set; }
        public bool EnableNotifications { get; set; }
        #endregion

        #region Attributes
        InternalThemeInfoProvider InternalThemes;
        private object internalLock = new object(); //This internal lock is a REENTRANT (for samethread, meaning it can be locked multiple times inside the same thread).
        private object _activeTheme;
        private IDialogService _ds = new DialogService();
        private bool _internalThemeInitialized = false;
        private List<string> _failedGroups = new List<string>();

        #region Dictionaries
        private ConcurrentDictionary<object, List<ThemeInfoEx>> _externalThemes = new ConcurrentDictionary<object, List<ThemeInfoEx>>();
        private ConcurrentDictionary<object, List<ThemeInfo>> _globalThemes = new ConcurrentDictionary<object, List<ThemeInfo>>();
        #endregion

        #endregion

        #region Constructors
        public static ThemeService Singleton = new ThemeService();
        public static ThemeService getSingleton()
        {
            if (Singleton == null) Singleton = new ThemeService();
            return Singleton;
        }
        private ThemeService() 
        { 
            ErrorHandling = ExceptionHandling.Throw;
            EnableNotifications = false;
            StartupTheme = null;
            ActiveTheme = null; 
        }
        #endregion

        #region Public Methods
        public List<ThemeInfo> GetThemeInfos(object key, RegistrationMode dicType)
        {
            try
            {
                if (!IsKeyValid(key)) return null;

                switch (dicType)
                {
                    case RegistrationMode.Independent:
                        if (_globalThemes.ContainsKey(key))
                        {
                            _globalThemes.TryGetValue(key, out var globalThemedata);
                            return globalThemedata;
                        }
                        break;
                    case RegistrationMode.AssemblyBased:
                        if (_externalThemes.ContainsKey(key))
                        {
                            _externalThemes.TryGetValue(key, out var themeData);
                            return themeData?.Cast<ThemeInfo>()?.ToList();
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }
        public List<object> GetThemes(RegistrationMode dicType)
        {
            try
            {
                switch (dicType)
                {
                    case RegistrationMode.Independent:
                        return _globalThemes?.Keys.ToList();
                    case RegistrationMode.AssemblyBased:
                        return _externalThemes?.Keys.ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }
        public bool SetStartupTheme(object startupKey)
        {
            if (StartupTheme != null)
            {
                var _msg = "Startup theme can be set only when the active theme is empty. Cannot set twice.";
                HandleException(_msg,nameof(startupKey),true); //Force throw exception.
                return false;
            }
            StartupTheme = startupKey; //This becomes our startup theme.
            return true;
        }
        public bool SetupInternalTheme(Func<InternalThemeInfoProvider> provider)
        {
            if (provider == null) return false;
            if (_internalThemeInitialized)
            {
                HandleException("Internal Theme is already initialized. Cannot set again.", null);
                return false;
            }
            InternalThemes = provider.Invoke();
            if (InternalThemes != null) _internalThemeInitialized = true;
            return _internalThemeInitialized;
        }
        public bool IsThemeKeyRegistered(object key)
        {
            try
            {
                if (!IsKeyValid(key)) return false;
                //The key should be present in any one of the dictionary.
                bool registered = false;
                registered = IsThemeKeyRegistered(key, RegistrationMode.Independent);
                if (registered) return true;
                registered = IsThemeKeyRegistered(key, RegistrationMode.AssemblyBased);
                return registered;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsThemeKeyRegistered(object key,RegistrationMode dicType)
        {
            if (!IsKeyValid(key)) return false;
            switch (dicType)
            {
                case RegistrationMode.Independent:
                    return (_globalThemes.ContainsKey(key));
                case RegistrationMode.AssemblyBased:
                    return (_externalThemes.ContainsKey(key));
            }
            return false;
        }
        public string Register(Dictionary<object, InternalThemeMode> themeGroup)
        {
            //The internal themes should be initialized.
            if (!_internalThemeInitialized)
            {
                HandleException($@"Internal themes are not yet initialized. Please use the method {nameof(SetupInternalTheme)} to setup the internal themes.", "Internal Themes");
                return null;
            }

            return RegisterWithGroupId(themeGroup,(Gid) => 
            {
                foreach (var kvp in themeGroup)
                {
                    if (!Register(kvp.Key, kvp.Value, Gid)) //Register under same group id.
                    {
                        return false; //Break and return because we failed to register.
                    }
                }
                return true; 
            });
        }
        public string Register(Dictionary<object, Uri> themeGroup, bool assemblyIndependent = false)
        {
            return RegisterWithGroupId(themeGroup, (Gid) =>
            {
                foreach (var kvp in themeGroup)
                {
                    if (!Register(kvp.Key, kvp.Value, Gid, assemblyIndependent)) //Register under same group id.
                    {
                        return false; //Break and return because we failed to register.
                    }
                }
                return true;
            });
        }
        public string Register(Dictionary<object, Uri> themeGroup, Assembly targetAssembly)
        {
            return RegisterWithGroupId(themeGroup, (Gid) =>
            {
                foreach (var kvp in themeGroup)
                {
                    if (!Register(kvp.Key, kvp.Value, targetAssembly, Gid)) //Register under same group id.
                    {
                        return false; //Break and return because we failed to register.
                    }
                }
                return true;
            });
        }
        #endregion

        #region Registration Helpers
        private string RegisterWithGroupId(object themeDicObject, Func<string, bool> regDelegate)
        {
            if (themeDicObject == null)
            {
                HandleException($@"Dictionary values cannot be empty", "ThemeGroup");
                return null;
            }

            string Gid = Guid.NewGuid().ToString();
            if (regDelegate.Invoke(Gid)) return Gid;
            _failedGroups.Add(Gid); //Can be used later to remove.
            return null;
        }
        private bool Register(object key, InternalThemeMode mode, string groupId)
        {
            //The internal themes should be initialized.
            if (!_internalThemeInitialized)
            {
                HandleException($@"Internal themes are not yet initialized. Please use the method {nameof(SetupInternalTheme)} to setup the internal themes.", "Internal Themes");
                return false;
            }

            if (!InternalThemes.InfoDic.ContainsKey(mode))
            {
                HandleException($@"Internal themes doesn't have any registered themeinfo related to the key {mode.ToString()}", "Internal Themes");
                return false;
            }

            if (!InternalThemes.InfoDic.TryGetValue(mode, out var _internalURI)) return false;

            RegisterGlobal(key, _internalURI,groupId);

            return true;
        }
        private bool Register(object key, Uri value, string groupId, bool assemblyIndependent = false)
        {
            if (assemblyIndependent)
            {
                return RegisterGlobal(key, value,groupId); //Register only in the global data.
            }
            var asmbly = Assembly.GetCallingAssembly();
            return Register(key, value, asmbly,groupId);
        }
        private bool Register(object key, Uri value, Assembly targetAssembly, string groupId)
        {
            if (!IsKeyValid(key)) return false;
            if (value == null)
            {
                HandleException("URI value cannot be null. Path of themeinfo needs a proper value.", nameof(value));
                return false;
            }
            if (targetAssembly == null)
            {
                HandleException("targetAssembly cannot be null.", nameof(targetAssembly));
                return false;
            }
            if (!_externalThemes.ContainsKey(key))
            {
                _externalThemes.TryAdd(key, new List<ThemeInfoEx>());
            }

            _externalThemes.TryGetValue(key, out var themeData);

            if (themeData == null)
            {
                themeData = new List<ThemeInfoEx>();
            }

            if (themeData.Any(p => p.SourceAssembly == targetAssembly && p.Path?.ToString() == value.ToString() && p.GroupId == groupId))
            {
                var _msg = $@"The key {GetKeyString(key)} with assembly {targetAssembly.GetName().Name} and path {value.ToString()} is already registered.";
                Debug.WriteLine(_msg);
                return false;
            }

            lock (internalLock)
            {
                themeData.Add(new ThemeInfoEx(value,groupId) { SourceAssembly = targetAssembly});
            }
            return true;
        }
        private bool RegisterGlobal(object key, Uri value, string groupId)
        {
            if (!IsKeyValid(key)) return false;
            if (value == null)
            {
                HandleException("ThemeInfo URI cannot be null. Path of themeinfo needs a proper value.", nameof(value));
                return false;
            }
            if (!_globalThemes.ContainsKey(key))
            {
                if (!_globalThemes.TryAdd(key, new List<ThemeInfo>()))
                {
                    HandleException($@"Unable to add the key {GetKeyString(key)} to the globaltheme dictionary.");
                    return false;
                }
            }

            _globalThemes.TryGetValue(key, out var _themeList);
            if (_themeList.Any(p => p.Path == value && p.GroupId ==groupId)) //is alresady present.
            {
                Debug.WriteLine($@"The theme with path {value} is already registered for the key {GetKeyString(key)} with similar groupid.");
                return true; //Already added. 
            }
            lock (internalLock)
            {
                _themeList.Add(new ThemeInfo(value,groupId) {});
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
        public bool ChangeTheme(object newThemeKey)
        {
            //This will only change the application level theme change which will also raise the theme changed event at last, which will be consumed by the controls (if they have subscribed to it).
            object oldthemekey = ActiveTheme ?? StartupTheme; //Startup theme is the fall back. If nothing is present, we consider startup theme.
            string _msg = null;
            //Check current theme
            if (oldthemekey == null)
            {
                _msg = "Startuptheme is null. Use SetStartupTheme method to set the initial starting theme. Based on the startup theme, the new themes can be changed and applied.";
                HandleException(_msg, "StartupTheme");
                return false;
            }

            //Does current theme have a registered info value? (in any dictionary)
            if (!IsThemeKeyRegistered(oldthemekey))
            {
                _msg = $@"Startuptheme has to be a valid registered key. Current startup theme {GetKeyString(oldthemekey)} is not a valid key value. Please register the themeinfos using this key or provide a valid startup key.";
                HandleException(_msg);
                return false;
            }
            //The sender can never be null. Do remember to send the assembly as the sender.
            //var _caller = Assembly.GetCallingAssembly(); //this will cause issues as calling can happen from anywhere.
            var _caller = Assembly.GetEntryAssembly(); //Irrespective of where it is getting called, it will always use the .Exe assembly (which will ofcouse contain the App.Xaml or the App.Resources).
            return Validate(newThemeKey,oldthemekey, null,_caller);
        }
        public bool ChangeTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode = ThemeSearchMode.Application)
        {
            return Validate(newThemeKey, oldThemeKey, frameworkElement, targetAssembly, searchMode, false); //Here we will not raise the change events as we give the user option to use the assembly. Internal assembly change should not raise anything.
        }

        #endregion

        #region Core Implementations
        private bool Validate(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode = ThemeSearchMode.Application, bool raiseChangeEvents = true)
        {
            string _msg = null;

            if (targetAssembly == null)
            {
                _msg = "For changing a theme, the assembly detail is mandatory to fetch the registered themes.";
                HandleException(_msg, nameof(targetAssembly));
                return false;
            }

            if (!IsKeyValid(oldThemeKey)) return false;
            if (!IsKeyValid(newThemeKey)) return false;

            //Does current theme have a registered info value? (any dictionary)
            if (!IsThemeKeyRegistered(oldThemeKey))
            {
                _msg = $@"OldThemeKey is not valid. It doesn't have any registered value. Key: {GetKeyString(oldThemeKey)} ";
                HandleException(_msg);
                return false;
            }

            if (!IsThemeKeyRegistered(newThemeKey))
            {
                _msg = $@"Key : {GetKeyString(newThemeKey)} is not registered in any internal, external or global theme dictionary. Please use a registered key.";
                HandleException(_msg, nameof(newThemeKey));
                return false;
            }

            if (raiseChangeEvents)
            {
                //Then it means we need to change and also raise the change events.
                //So compare with existing before raising event.
                if (oldThemeKey == newThemeKey)
                {
                    if (EnableNotifications)
                    {
                        _ds?.SendToast("No change", $@"Old theme key and new theme key are same. Nothing to change. Key {GetKeyString(newThemeKey)}");
                    }
                    return false;
                }
            }

            if (!PrepareThemeChangeData(newThemeKey, oldThemeKey, frameworkElement, targetAssembly, searchMode)) return false;

            if (raiseChangeEvents)
            {
                //Set the new theme.
                ActiveTheme = newThemeKey; //This will raise event and trigger the other controls to change their own themes.
                ThemeChanged?.Invoke(this, (newThemeKey, oldThemeKey));
            }

            return true;
        }
        private bool PrepareThemeChangeData(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode)
        {
            //For an external theme to change, the old and new theme keys should be present (preference to targetassembly and then the global).
            //Each assembly might have registered it's own version of theme, if not we try to get the global version.
            //Since we are here at this point, we are sure that the theme is already registered in some dictionary.

            List<ThemeInfo> oldThemeInfos = FetchThemes(oldThemeKey, targetAssembly);
            if (oldThemeInfos == null || oldThemeInfos.Count == 0)
            {
                Debug.WriteLine($@"Infos not found for {GetKeyString(oldThemeKey)} in any dictionary.");
                return false;
            }

            List<ThemeInfo> newThemeInfos = FetchThemes(newThemeKey, targetAssembly);
            if (newThemeInfos == null || newThemeInfos.Count == 0)
            {
                Debug.WriteLine($@"Infos not found for {GetKeyString(newThemeKey)} in any dictionary.");
                return false;
            }

            if(oldThemeInfos.Count != newThemeInfos.Count)
            {
                Debug.WriteLine($@"The dictionary count has a mismatch between {GetKeyString(newThemeKey)} and {GetKeyString(oldThemeKey)}.");
            }

            //Get old theme and the new theme info and change them.
            ThemeChangeData _changeData = new ThemeChangeData() { OldThemes = oldThemeInfos, NewThemes = newThemeInfos, SearchMode = searchMode, Sender = frameworkElement, Themekey = newThemeKey };

            return Process(_changeData);
        }
        private bool Process(ThemeChangeData changeData, bool  overwriteAtRoot = false)
        {
            try
            {
                if (changeData.Sender == null && changeData.SearchMode == ThemeSearchMode.FrameworkElement)
                {
                    var _msg = "The (sender) framework element is null. But the search mode is for frameworke element. Cannot change proceed further.";
                    //We cannot get Frameworkelement resources since it is null, do not change anything. return.
                    Debug.WriteLine(_msg);

                    if (EnableNotifications)
                    {
                        _ds?.SendToast("Null frameworkelement", _msg);
                        return false;
                    }
                }

                var _rootDictionary = GetRootDictionary(changeData.SearchMode, changeData.Sender);

                //After we get the root dictionary, we can either loop through, find the final target and replace it. Or, we can even 
                return FindAndReplace(ref _rootDictionary, changeData, overwriteAtRoot);
            }
            catch (Exception ex)
            {
                HandleException(ex.ToString());
                return false;
            }
        }
        private bool FindAndReplace(ref ResourceDictionary rootDictionary, ThemeChangeData changeData, bool overwriteAtRoot)
        {
            if (rootDictionary == null) return false; //Sometimes when the object is getting loaded, the RD might not have been loaded and it might result in null

            if (rootDictionary.MergedDictionaries == null || rootDictionary.MergedDictionaries.Count == 0) return false;

            List<ThemeTracker> trackers = new List<ThemeTracker>();

            if (!GetTrackers(ref rootDictionary, changeData, ref trackers, overwriteAtRoot)) return false;

            //We got a tracker.
            if (!Replace(trackers, changeData)) return false;

            SetRootDictionary(changeData.SearchMode, changeData.Sender);

            return true;
        }
        private bool Replace(List<ThemeTracker> trackers,ThemeChangeData changeData)
        {
            if (tracker == null) return false;

            if (!tracker.IsTarget)
            {
                //Go down the tree
                var child = tracker.Child;
                return Replace(ref child, changeData);
            }

            //When you reach the target, check if the parent contains, the old theme and remove it.

            var _oldRD = tracker.Parent?.RD.MergedDictionaries.FirstOrDefault(p => p.Source == changeData.OldThemes.Path);
            if (_oldRD != null)
            {
                //iF THIS IS Null, then it means that we are overwritting at root level.
                tracker.Parent?.RD.MergedDictionaries.Remove(_oldRD); //Do not remove directly using the tracker as it could contain the cached values (which would have been old). We only use the cache for traversing the tree.
            }
           
            tracker.Parent?.RD.MergedDictionaries.Insert(0, new ResourceDictionary() { Source = changeData.NewThemes.Path });
            return true;
        }
        #endregion

        #region Helpers
        private bool IsThemeInfoValid(ThemeInfo info)
        {
            return (info.Path != null);
        }
        private List<ThemeInfo> FetchThemes(object themeKey, Assembly targetAssembly)
        {
            List<ThemeInfo> ThemeInfos = new List<ThemeInfo>();
            //Get from externaldic 
            if (_externalThemes.TryGetValue(themeKey, out var ext_infoList))
            {
                var _externalData = ext_infoList.Where(p => p.SourceAssembly == targetAssembly)?.Cast<ThemeInfo>().ToList();
                if (_externalData != null && _externalData.Count > 0)
                {
                    ThemeInfos.AddRange(_externalData);
                }
            }

            //Get from globalDic
            if (_globalThemes.TryGetValue(themeKey, out var glb_infoList))
            {
                var _glbData = glb_infoList;
                if (_glbData != null && _glbData.Count > 0)
                {
                    ThemeInfos.AddRange(_glbData);
                }
            }

            return ThemeInfos;
        }
        private void HandleException(string message, string paramName = null, bool forceThrow = false)
        {
            if (ErrorHandling == ExceptionHandling.Throw || forceThrow)
            {
                if (!string.IsNullOrWhiteSpace(paramName))
                {
                    throw new ArgumentNullException(paramName, message);
                }
                throw new ArgumentException(message);
            }

            switch (ErrorHandling)
            {
                case ExceptionHandling.OutputDiagnostics:
                    Debug.WriteLine(message);
                    break;
                case ExceptionHandling.ShowToast:
                    _ds?.SendToast("Theme Exception", message, NotificationIcon.Error, display_seconds: 3);
                    break;
            }
        }
        private bool IsKeyValid(object key)
        {
            if (key == null)
            {
                HandleException("The theme key cannot be empty. Please provide a valid key for processing.", nameof(key));
                return false;
            }

            return true;
        }
        private string GetKeyString(object key)
        {
            if (key == null) return null;
            if (key is string _key_str)
            {
                return _key_str.ToLower();
            }
            return key.AsString()?.ToLower() ?? key.ToString();
        }
        private ResourceDictionary GetRootDictionary(ThemeSearchMode searchType, object sender)
        {
            //We expect either FrameWorkElement or Application
            ResourceDictionary resource = null;
            if (searchType == ThemeSearchMode.FrameworkElement)
            {
                if (sender != null)
                {
                    resource = ((FrameworkElement)sender).Resources;
                }
            }
            else
            {
                if (Application.Current != null)
                {
                    resource = Application.Current.Resources;
                }
            }

            return resource;
        }
        private void SetRootDictionary(ThemeSearchMode searchType, object sender)
        {
            ResourceDictionary _rootdic = null;
            if (searchType == ThemeSearchMode.FrameworkElement)
            {
                if (sender != null)
                {
                    _rootdic = ((FrameworkElement)sender).Resources;
                    ((FrameworkElement)sender).Resources = null;
                    ((FrameworkElement)sender).Resources = _rootdic;
                }
            }
            else
            {
                if (Application.Current != null)
                {
                    _rootdic = Application.Current.Resources;
                    Application.Current.Resources = null;
                    Application.Current.Resources = _rootdic;
                }
            }
        }
        private bool GetTracker(ResourceDictionary rootDic, Uri oldThemeURI, ref ThemeTracker tracker, bool overwriteAtRoot)
        {
            //Our goal is to find which resource dictionary ends with old theme uri.
            tracker.RD = rootDic; //Root dic goes to the root tracker's RD
            ThemeTracker childTracker = new ThemeTracker();
            childTracker.Parent = tracker; //this is the child's parent.

            //Check if child direclty matches? || Probably WILL NEVER HAPPEN.
            if (rootDic.Source != null && rootDic.Source == oldThemeURI)
            {
                //Found matching uri.
                tracker.IsTarget = true;
                return true;
            }

            //Check if any of the merged dictionaries of the child matches.
            var matchFound = rootDic.MergedDictionaries.FirstOrDefault(p => p.Source != null && p.Source == oldThemeURI);

            //If overwrite at root level, then it should be merged to the rootDic's merged dictionaries.
            //This will ensure that all the cascading themes will follow it up.
            //This is kind of like a override mechanism.
            if (matchFound != null || overwriteAtRoot)
            {
                //if matchfound is not null, then we actually have this at root level.
                //If match is not found but we overwrite it, we, proceed.
                childTracker.RD = matchFound; //This could be null.
                childTracker.IsTarget = true;
                tracker.Child = childTracker;
                return true;
            }

            //Loop through each of the merged dictionaries.
            foreach (var rDic in rootDic.MergedDictionaries)
            {
                if (GetTracker(rDic, oldThemeURI, ref childTracker, overwriteAtRoot))
                {
                    //If we manage the get the value.
                    tracker.Child = childTracker;
                    return true;
                }
            }

            return false;
        }
        private bool GetTrackers(ref ResourceDictionary rootDic, ThemeChangeData changeData, ref List<ThemeTracker> trackers, bool overwriteAtRoot)
        {
            //Find multiple trackers for the oldthemeinfo inside the changedata.
            //Our goal is to find which resource dictionary ends with old theme uri.
            tracker.RD = rootDic; //Root dic goes to the root tracker's RD
            ThemeTracker childTracker = new ThemeTracker();
            childTracker.Parent = tracker; //this is the child's parent.

            //Check if child direclty matches? || Probably WILL NEVER HAPPEN.
            if (rootDic.Source != null && rootDic.Source == oldThemeURI)
            {
                //Found matching uri.
                tracker.IsTarget = true;
                return true;
            }

            //Check if any of the merged dictionaries of the child matches.
            var matchFound = rootDic.MergedDictionaries.FirstOrDefault(p => p.Source != null && p.Source == oldThemeURI);

            //If overwrite at root level, then it should be merged to the rootDic's merged dictionaries.
            //This will ensure that all the cascading themes will follow it up.
            //This is kind of like a override mechanism.
            if (matchFound != null || overwriteAtRoot)
            {
                //if matchfound is not null, then we actually have this at root level.
                //If match is not found but we overwrite it, we, proceed.
                childTracker.RD = matchFound; //This could be null.
                childTracker.IsTarget = true;
                tracker.Child = childTracker;
                return true;
            }

            //Loop through each of the merged dictionaries.
            foreach (var rDic in rootDic.MergedDictionaries)
            {
                if (GetTracker(rDic, oldThemeURI, ref childTracker, overwriteAtRoot))
                {
                    //If we manage the get the value.
                    tracker.Child = childTracker;
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}

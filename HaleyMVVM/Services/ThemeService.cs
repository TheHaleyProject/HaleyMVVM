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
        public bool ThrowExceptionsOnFailure { get; set; }
        public bool EnableTrackerCache { get; set; }
        public InternalThemeData HaleyThemes { get; private set; }
        #endregion

        #region Attributes
        private object _activeTheme;
        private IDialogService _ds = new DialogService();

        #region Dictionaries
        private ConcurrentDictionary<object, List<ThemeInfoEx>> _externalThemes = new ConcurrentDictionary<object, List<ThemeInfoEx>>();
        private ConcurrentDictionary<object, ThemeInfo> _globalThemes = new ConcurrentDictionary<object, ThemeInfo>();
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
            ThrowExceptionsOnFailure = true;
            EnableTrackerCache = true;
            StartupTheme = null;
            ActiveTheme = null; 
        }
        #endregion

        #region Registrations
        public bool AttachHaleyThemes(InternalThemeData internal_themedata)
        {
            if (HaleyThemes != null)
            {
                bool overwriteData = false;
                var _msg = "Internal themes is already present. Do you wish to override the existing values?.";
                if (_ds != null)
                {
                   var _notifyres = _ds.Warning("Internal Themes Override", _msg, DialogMode.Confirmation);
                    overwriteData = (_notifyres.DialogResult.HasValue && _notifyres.DialogResult.Value);
                }
                else
                {
                    var _res = MessageBox.Show(_msg, "Internal Themes Override", MessageBoxButton.YesNo);
                    overwriteData = (_res == MessageBoxResult.Yes);
                }
                if (!overwriteData) return false;
            }
            HaleyThemes = internal_themedata;
            return true;
        }
        public List<ThemeInfo> GetThemeInfos(object key, ThemeDictionary dicType)
        {
            try
            {
                if (!IsKeyValid(key)) return null;

                switch (dicType)
                {
                    case ThemeDictionary.Global:
                        if (_globalThemes.ContainsKey(key))
                        {
                            _globalThemes.TryGetValue(key, out var globalThemedata);
                            return new List<ThemeInfo>() { globalThemedata };
                        }
                        break;
                    case ThemeDictionary.External:
                        if (_externalThemes.ContainsKey(key))
                        {
                            _externalThemes.TryGetValue(key, out var themeData);
                            return themeData?.Cast<ThemeInfo>()?.ToList();
                        }
                        break;
                    case ThemeDictionary.Internal:
                        if (HaleyThemes != null && HaleyThemes.Themes.ContainsKey(key))
                        {
                            HaleyThemes.Themes.TryGetValue(key, out var _internalMode);
                            HaleyThemes.InfoDic.TryGetValue(_internalMode, out var internalThemeData);
                            return new List<ThemeInfo>() { internalThemeData };
                        }
                        break;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<object> GetThemes(ThemeDictionary dicType)
        {
            try
            {
                switch (dicType)
                {
                    case ThemeDictionary.Global:
                        return _globalThemes?.Keys.ToList();
                    case ThemeDictionary.External:
                        return _externalThemes?.Keys.ToList();
                    case ThemeDictionary.Internal:
                        if (HaleyThemes != null)
                        {
                            return HaleyThemes?.Themes.Keys.ToList();
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
        public void SetStartupTheme(object startupKey)
        {
            if (StartupTheme != null)
            {
                var _msg = "Startup theme can be set only when the active theme is empty. Cannot set twice.";
                HandleException(_msg,nameof(startupKey),true); //Force throw exception.
                return;
            }
            StartupTheme = startupKey; //This becomes our startup theme.
        }
        public bool IsThemeKeyRegistered(object key)
        {
            try
            {
                if (!IsKeyValid(key)) return false;
                //The key should be present in any one of the dictionary.
                bool registered = false;
                registered = IsThemeKeyRegistered(key, ThemeDictionary.Global);
                if (registered) return true;
                registered = IsThemeKeyRegistered(key, ThemeDictionary.Internal);
                if (registered) return true;
                registered = IsThemeKeyRegistered(key, ThemeDictionary.External);
                return registered;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsThemeKeyRegistered(object key,ThemeDictionary dicType)
        {
            if (!IsKeyValid(key)) return false;
            switch (dicType)
            {
                case ThemeDictionary.Global:
                    return (_globalThemes.ContainsKey(key));
                case ThemeDictionary.External:
                    return (_externalThemes.ContainsKey(key));
                case ThemeDictionary.Internal:
                    if (HaleyThemes == null || HaleyThemes.Themes == null) return false;
                    return (HaleyThemes.Themes.ContainsKey(key));
            }
            return false;
        }
        public bool RegisterGlobal(object key, ThemeInfo value)
        {
            if (!IsKeyValid(key)) return false;
            if (value == null || !IsThemeInfoValid(value))
            {
                HandleException("ThemeInfo cannot be null. Path of themeinfo needs a proper value.", nameof(value));
                return false;
            }
            if (_globalThemes.ContainsKey(key))
            {
                var _msg = $@"A theme is already registered for the key {GetKeyString(key)}. Please provide unique values.";
                HandleException(_msg);
                return false;
            }

            return _globalThemes.TryAdd(key, value);
        }
        public bool Register(object key, ThemeInfo value)
        {
            var asmbly = Assembly.GetCallingAssembly();
            return Register(key, value, asmbly);
        }
        public bool Register(object key, ThemeInfo value, Assembly targetAssembly)
        {
            if (!IsKeyValid(key)) return false;
            if (value == null || !IsThemeInfoValid(value))
            {
                HandleException("ThemeInfo cannot be null. Path of themeinfo needs a proper value.",nameof(value));
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

            if (themeData.Any(p => p.SourceAssembly == targetAssembly))
            {
                var _msg = $@"A theme is already registered for the assembly {targetAssembly.GetName().Name} against key {GetKeyString(key)}. An assembly should have unique themes for each key.";
                HandleException(_msg);
                return false;
            }

            themeData.Add(new ThemeInfoEx(value.Name, value.Path) { SourceAssembly = targetAssembly,StoredDB = ThemeDictionary.External});

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
        public bool ChangeTheme(object newThemeKey, bool showNotifications = false)
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
            return changeTheme(newThemeKey,oldthemekey, null,_caller,showNotifications: showNotifications);
        }

        public bool ChangeTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode = ThemeSearchMode.Application, bool showNotifications = false)
        {
            return changeTheme(newThemeKey, oldThemeKey, frameworkElement, targetAssembly, searchMode, false, showNotifications); //Here we will not raise the change events as we give the user option to use the assembly. Internal assembly change should not raise anything.
        }

        #endregion

        #region Helpers
        private bool IsThemeInfoValid(ThemeInfo info)
        {
            return (info.Path != null);
        }
        private bool changeTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode = ThemeSearchMode.Application, bool raiseChangeEvents = true, bool showNotifications = false)
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
                    if (_ds != null)
                    {
                        _ds.SendToast("No change", $@"Old theme key and new theme key are same. Nothing to change. Key {GetKeyString(newThemeKey)}");
                        return false;
                    }
                }
            }

            //EACH DLL MIGHT HAVE INTERNAL AND ALSO EXTERNAL THEME IN THEIR MERGED DICTIONARIES.
            bool themeChanged = false;

            if (ChangeExternalTheme(newThemeKey, oldThemeKey, frameworkElement, targetAssembly, searchMode, showNotifications))
            {
                themeChanged = true;
            }

            if (ChangeHaleyThemes(newThemeKey, oldThemeKey, frameworkElement, targetAssembly, searchMode, showNotifications))
            {
                themeChanged = true;
            }

            if (raiseChangeEvents && themeChanged)
            {
                //Set the new theme.
                ActiveTheme = newThemeKey; //This will raise event and trigger the other controls to change their own themes.
                ThemeChanged?.Invoke(this, (newThemeKey, oldThemeKey));
            }

            return true;
        }
        private bool ChangeExternalTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode, bool showNotifications)
        {
            //For an external theme to change, the old and new theme keys should be present (preference to targetassembly and then the global).
            //Each assembly might have registered it's own version of theme, if not we try to get the global version.
            //Since we are here at this point, we are sure that the theme is already registered in some dictionary.

            ThemeInfo oldThemeInfo = null;
            ThemeInfo newThemeInfo = null;

            //PROCESS OLD THEME INFO
            if (_externalThemes.TryGetValue(oldThemeKey,out var oldInfoList))
            {
                oldThemeInfo = oldInfoList.FirstOrDefault(p => p.SourceAssembly == targetAssembly);
            }

            if (oldThemeInfo == null)
            {
                //Try to check in global.
                _globalThemes.TryGetValue(oldThemeKey, out oldThemeInfo);
            }

            if (oldThemeInfo == null)
            {
                Debug.WriteLine($@"Info not found for {GetKeyString(oldThemeKey)} in external and global dictionaries.");
                return false;
            }

            //PROCESS NEW THEME INFO
            if (_externalThemes.TryGetValue(newThemeKey, out var newInfoList))
            {
                newThemeInfo = newInfoList.FirstOrDefault(p => p.SourceAssembly == targetAssembly);
            }

            if (newThemeInfo == null)
            {
                //Try to check in global.
                _globalThemes.TryGetValue(newThemeKey, out newThemeInfo);
            }

            if (newThemeInfo == null)
            {
                Debug.WriteLine($@"Info not found for {GetKeyString(newThemeKey)} in external and global dictionaries.");
                return false;
            }

            if (oldThemeInfo.Path == null || newThemeInfo.Path == null)
            {
                Debug.WriteLine("For changing the internal theme, the themeinfo Paths should not be null.");
                return false;
            }

            if (oldThemeInfo.Path == newThemeInfo.Path)
            {
                Debug.WriteLine($@"Internal theme paths are same, Nothing to change.{newThemeInfo.Path}");
                return false;
            }

            //Get old theme and the new theme info and change them.
            ThemeChangeData _changeData = new ThemeChangeData() { OldTheme = oldThemeInfo, NewTheme = newThemeInfo, SearchMode = searchMode, Sender = frameworkElement, RaiseNotifications = showNotifications ,Themekey = newThemeKey};

            return changeTheme(_changeData);
        }
        private bool ChangeHaleyThemes(object newThemeKey,object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode, bool showNotifications)
        {
            do
            {
                if (HaleyThemes == null)
                {
                    Debug.WriteLine("Internal themes data is empty.");
                    break;
                }
                if (!HaleyThemes.Themes.ContainsKey(newThemeKey))
                {
                    Debug.WriteLine($@"Internal theme settings doesn't have any info associated with the key {GetKeyString(newThemeKey)}");
                    break;
                }
                if (!HaleyThemes.Themes.ContainsKey(oldThemeKey))
                {
                    Debug.WriteLine($@"Internal theme settings doesn't have any info associated with the key {GetKeyString(oldThemeKey)}");
                    break;
                }

                //Get Old Info
                HaleyThemes.Themes.TryGetValue(oldThemeKey, out var oldMode);
                HaleyThemes.InfoDic.TryGetValue(oldMode, out var OldThemeInfo);

                //Get New Info
                HaleyThemes.Themes.TryGetValue(newThemeKey, out var newMode);
                HaleyThemes.InfoDic.TryGetValue(newMode, out var NewThemeInfo);

                if (OldThemeInfo == null)
                {
                    Debug.WriteLine($@"Themeinfo for the key {GetKeyString(oldThemeKey)} is empty. Cannot change anything.");
                    break;
                }

                if (NewThemeInfo == null)
                {
                    Debug.WriteLine($@"Themeinfo for the key {GetKeyString(newThemeKey)} is empty. Cannot change anything.");
                    break;
                }

                if (OldThemeInfo.Path == null || NewThemeInfo.Path == null)
                {
                    Debug.WriteLine("For changing the internal theme, the themeinfo Paths should not be null.");
                    break;
                }

                if (OldThemeInfo.Path == NewThemeInfo.Path)
                {
                    Debug.WriteLine($@"Internal theme paths are same, Nothing to change.{NewThemeInfo.Path}");
                    break;
                }

                //Get old theme and the new theme info and change them.
                ThemeChangeData _changeData = new ThemeChangeData() {OldTheme = OldThemeInfo,NewTheme = NewThemeInfo,SearchMode = searchMode,Sender = frameworkElement,RaiseNotifications = showNotifications, Themekey = newThemeKey };

                return changeTheme(_changeData,true);

            } while (false);
            return false;
        }
       
        private void HandleException(string message, string paramName = null,bool forceThrow = false)
        {
            if (ThrowExceptionsOnFailure || forceThrow)
            {
                if (!string.IsNullOrWhiteSpace(paramName))
                {
                    throw new ArgumentNullException(paramName, message);
                }
                throw new ArgumentException(message);
            }
            Debug.WriteLine(message);
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
        #endregion

        #region Core Implementations
        private bool changeTheme(ThemeChangeData changeData, bool  overwriteAtRoot = false)
        {
            try
            {
                if (changeData.Sender == null && changeData.SearchMode == ThemeSearchMode.FrameworkElement)
                {
                    var _msg = "The (sender) framework element is null. But the search mode is for frameworke element. Cannot change proceed further.";
                    //We cannot get Frameworkelement resources since it is null, do not change anything. return.
                    Debug.WriteLine(_msg);

                    if (changeData.RaiseNotifications)
                    {
                        _ds?.SendToast("Null frameworkelement", _msg);
                        return false;
                    }
                }

                //There is a possiblity the changeData could already have a ThemeTracker value.

                var _rootDictionary = getRootDictionary(changeData.SearchMode, changeData.Sender);

                //After we get the root dictionary, we can either loop through, find the final target and replace it. Or, we can even 
                return findAndReplace(ref _rootDictionary, changeData, overwriteAtRoot);
            }
            catch (Exception ex)
            {
                if (changeData.RaiseNotifications)
                {
                    _ds.Error("Error", ex.ToString());
                }
                return false;
            }
        }
        private bool findAndReplace(ref ResourceDictionary rootDictionary, ThemeChangeData changeData, bool overwriteAtRoot)
        {
            if (rootDictionary == null) return false; //Sometimes when the object is getting loaded, the RD might not have been loaded and it might result in null

            if (rootDictionary.MergedDictionaries == null || rootDictionary.MergedDictionaries.Count == 0) return false;

            ThemeTracker tracker = null;

            if (changeData.OldTheme is ThemeInfoEx oldThemeEx)
            {
                //For global themes we will not have the tracking option.
                if ( oldThemeEx.IsTracked && EnableTrackerCache)
                {
                    //Meaning,we know the hierarchy. We already have a ThemeTracker. Use it to check and replace.
                    tracker = oldThemeEx.Tracker;
                }
            }

            if (tracker== null)
            {
                tracker = new ThemeTracker() { Parent = null };
                if (!getTracker(rootDictionary, changeData.OldTheme.Path, ref tracker, overwriteAtRoot)) return false;
            }
            
            //We got a tracker.
            if (!replaceTheme(ref tracker, changeData)) return false;

            setRootDictionary(changeData.SearchMode, changeData.Sender);

            if (EnableTrackerCache)
            {
                //Since it is successfull, Set this tracker, so that we don't search again and again.
                if (changeData.NewTheme is ThemeInfoEx newThemeEx)
                {
                    var _themeId = newThemeEx.Id;
                    if (newThemeEx.StoredDB == ThemeDictionary.External)
                    {
                        _externalThemes.TryGetValue(changeData.Themekey, out var infoList);
                        var _target = infoList.FirstOrDefault(p => p.Id == newThemeEx.Id);
                        if (_target != null)
                        {
                            _target.IsTracked = true;
                            _target.Tracker = tracker;
                        }
                    }
                }
            }

         return true;
        }
        private ResourceDictionary getRootDictionary(ThemeSearchMode searchType,object sender)
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

        private void setRootDictionary(ThemeSearchMode searchType, object sender)
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

        private bool getTracker(ResourceDictionary rootDic, Uri oldThemeURI, ref ThemeTracker tracker,bool overwriteAtRoot)
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
                if(getTracker(rDic, oldThemeURI, ref childTracker,overwriteAtRoot))
                {
                    //If we manage the get the value.
                    tracker.Child = childTracker;
                    return true;
                }
            }

            return false;
        }

        private bool replaceTheme(ref ThemeTracker tracker,ThemeChangeData changeData)
        {
            if (tracker == null) return false;

            if (!tracker.IsTarget)
            {
                //Go down the tree
                var child = tracker.Child;
                return replaceTheme(ref child, changeData);
            }

            //When you reach the target, check if the parent contains, the old theme and remove it.

            var _oldRD = tracker.Parent?.RD.MergedDictionaries.FirstOrDefault(p => p.Source == changeData.OldTheme.Path);
            if (_oldRD != null)
            {
                //iF THIS IS Null, then it means that we are overwritting at root level.
                tracker.Parent?.RD.MergedDictionaries.Remove(_oldRD); //Do not remove directly using the tracker as it could contain the cached values (which would have been old). We only use the cache for traversing the tree.
            }
           
            tracker.Parent?.RD.MergedDictionaries.Insert(0, new ResourceDictionary() { Source = changeData.NewTheme.Path });
            return true;
        }
        #endregion
    }
}

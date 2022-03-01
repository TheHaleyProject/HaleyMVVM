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
        public InternalThemeData InternalThemes { get; private set; }
        #endregion

        #region Attributes
        private object _activeTheme;
        private IDialogService _ds = new DialogService();

        #region Dictionaries
        private ConcurrentDictionary<object, List<ThemeInfo>> _externalThemes = new ConcurrentDictionary<object, List<ThemeInfo>>();
        private ConcurrentDictionary<object, ThemeInfoBase> _globalThemes = new ConcurrentDictionary<object, ThemeInfoBase>();
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
            StartupTheme = null;
            ActiveTheme = null; 
        }
        #endregion

        #region Registrations
        public bool AttachInternalTheme(InternalThemeData internal_themedata)
        {
            if (InternalThemes != null)
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
            InternalThemes = internal_themedata;
            return true;
        }
        public List<ThemeInfoBase> GetThemeInfos(object key, ThemeDictionary dicType)
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
                            return new List<ThemeInfoBase>() { globalThemedata };
                        }
                        break;
                    case ThemeDictionary.External:
                        if (_externalThemes.ContainsKey(key))
                        {
                            _externalThemes.TryGetValue(key, out var themeData);
                            return themeData?.Cast<ThemeInfoBase>()?.ToList();
                        }
                        break;
                    case ThemeDictionary.Internal:
                        if (InternalThemes != null && InternalThemes.Themes.ContainsKey(key))
                        {
                            InternalThemes.Themes.TryGetValue(key, out var _internalMode);
                            InternalThemes.InfoDic.TryGetValue(_internalMode, out var internalThemeData);
                            return new List<ThemeInfoBase>() { internalThemeData };
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
                        if (InternalThemes != null)
                        {
                            return InternalThemes?.Themes.Keys.ToList();
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
            _activeTheme = startupKey; //This becomes our startup theme.
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
                    if (InternalThemes == null || InternalThemes.Themes == null) return false;
                    return (InternalThemes.Themes.ContainsKey(key));
            }
            return false;
        }
        public bool RegisterGlobal(object key, ThemeInfoBase value)
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
        public bool Register(object key, ThemeInfoBase value)
        {
            var asmbly = Assembly.GetCallingAssembly();
            return Register(key, value, asmbly);
        }
        public bool Register(object key, ThemeInfoBase value, Assembly targetAssembly)
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
                _externalThemes.TryAdd(key, new List<ThemeInfo>());
            }

            _externalThemes.TryGetValue(key, out var themeData);

            if (themeData == null)
            {
                themeData = new List<ThemeInfo>();
            }

            if (themeData.Any(p => p.Source == targetAssembly))
            {
                var _msg = $@"A theme is already registered for the assembly {targetAssembly.GetName().Name} against key {GetKeyString(key)}. An assembly should have unique themes for each key.";
                HandleException(_msg);
                return false;
            }

            themeData.Add(new ThemeInfo(value.Name, value.Path) { Source = targetAssembly });

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
            var _caller = Assembly.GetCallingAssembly();
            return ChangeTheme(newThemeKey,oldthemekey, null,_caller, showNotifications: showNotifications);
        }
        public bool ChangeTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode priority = ThemeSearchMode.Application, bool raiseChangeEvents = true, bool showNotifications = false)
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
            if(ChangeInternalTheme(newThemeKey,oldThemeKey,frameworkElement,targetAssembly,priority,showNotifications))
            {
                themeChanged = true;
            }
            
            if(ChangeExternalTheme(newThemeKey, oldThemeKey, frameworkElement, targetAssembly, priority, showNotifications))
            {
                themeChanged = true;
            }

            if (raiseChangeEvents && themeChanged)
            {
                //Set the new theme.
                ActiveTheme = newThemeKey; //This will raise event and trigger the other controls to change their own themes.
                ThemeChanged?.Invoke(this, (newThemeKey,oldThemeKey));
            }

            return true;
        }
        #endregion

        #region Helpers
        private bool IsThemeInfoValid(ThemeInfoBase info)
        {
            return (info.Path != null);
        }
        private bool ChangeExternalTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode priority, bool showNotifications)
        {
            //For an external theme to change, the old and new theme keys should be present (preference to targetassembly and then the global).
            //Each assembly might have registered it's own version of theme, if not we try to get the global version.
            //Since we are here at this point, we are sure that the theme is already registered in some dictionary.

            ThemeInfoBase oldThemeInfo = null;
            ThemeInfoBase newThemeInfo = null;

            //PROCESS OLD THEME INFO
            if (_externalThemes.TryGetValue(oldThemeKey,out var oldInfoList))
            {
                oldThemeInfo = oldInfoList.FirstOrDefault(p => p.Source == targetAssembly);
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
                newThemeInfo = newInfoList.FirstOrDefault(p => p.Source == targetAssembly);
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
            ThemeChangeData _changeData = new ThemeChangeData() { OldTheme = oldThemeInfo, NewTheme = newThemeInfo, Priority = priority, Sender = frameworkElement, RaiseNotifications = showNotifications };

            return ChangeTheme(_changeData);
        }
        private bool ChangeInternalTheme(object newThemeKey,object oldThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode priority, bool showNotifications)
        {
            do
            {
                if (InternalThemes == null)
                {
                    Debug.WriteLine("Internal themes data is empty.");
                    break;
                }
                if (!InternalThemes.Themes.ContainsKey(newThemeKey))
                {
                    Debug.WriteLine($@"Internal theme settings doesn't have any info associated with the key {GetKeyString(newThemeKey)}");
                    break;
                }
                if (!InternalThemes.Themes.ContainsKey(oldThemeKey))
                {
                    Debug.WriteLine($@"Internal theme settings doesn't have any info associated with the key {GetKeyString(oldThemeKey)}");
                    break;
                }

                //Get Old Info
                InternalThemes.Themes.TryGetValue(oldThemeKey, out var oldMode);
                InternalThemes.InfoDic.TryGetValue(oldMode, out var OldThemeInfo);

                //Get New Info
                InternalThemes.Themes.TryGetValue(newThemeKey, out var newMode);
                InternalThemes.InfoDic.TryGetValue(newMode, out var NewThemeInfo);

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
                ThemeChangeData _changeData = new ThemeChangeData() {OldTheme = OldThemeInfo,NewTheme = NewThemeInfo,Priority = priority,Sender = frameworkElement,RaiseNotifications = showNotifications };

                return ChangeTheme(_changeData);

            } while (false);
            return false;
        }
        private bool ChangeTheme(ThemeChangeData changeData)
        {
            return true;
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
        private bool _changeTheme(DependencyObject sender, ThemeRD newtheme, ThemeSearchMode priority, bool compare_with_active_theme, bool raise_notification, bool include_haley_dictionaries = false, bool is_internal_call = false)
        {
            //Preliminary verifications
            if (newtheme == null || newtheme?.Path == null)
            {
                if (raise_notification)
                {
                    _ds.SendToast("Error", $@"The URI path for locating the theme  Dictionary is required.", NotificationIcon.Warning);
                }
                return false;
            }

            if (newtheme.PreviousThemePath == null)
            {
                //if active theme path is null, try to get it from the active theme property.
                if (ActiveTheme == null)
                {
                    if (raise_notification)
                    {
                        _ds.SendToast("Error", $@"Active theme is null. Please set StartupTheme or provide PreviousThemePath value to replace.", NotificationIcon.Warning);
                    }
                    return false;
                }

                newtheme.PreviousThemePath = ActiveTheme.Path;
            }
            //COMPARE WITH ACTUAL OLD THEME
            if (ActiveTheme != null && compare_with_active_theme)
            {
                //Basically, old theme's new uri is the actual value. So, actual value and newtheme's new value should not be equal.
                //If we try to set the same theme again, do not respond.
                if (ActiveTheme.Path == newtheme.Path)
                {
                    if (raise_notification)
                    {
                        _ds.SendToast("Conflict", $@"The theme ""{newtheme.Path}"" is the current theme. Nothing to change. Please check", NotificationIcon.Warning);
                    }
                    return false;
                }
            }

            //COMPARE WITH USER PROVIDED REPLACE THEME
            //if old and new theme are same, don't do anything.
            if (newtheme.Path.Equals(newtheme.PreviousThemePath))
            {
                if (raise_notification)
                {
                    _ds.Warning("Conflict", $@"The both ActiveThemePath and New Theme path are same. Nothing to change. Please check");
                }
                return false;
            }

            //SET ATTRIBUTES ONLY AFTER ABOVE VALIDATIONS.
            _raise_notification = raise_notification; //May change each time.
            _priority = priority;
            _source_theme = newtheme;
            _sender = sender;
            _includeHaley = include_haley_dictionaries;

            if (_changeTheme())
            {
                //If we are changing for Haley Internal, we do not change other values
                if (!is_internal_call)
                {
                    if (ActiveTheme != null)
                    {
                        OldTheme = new ThemeRD(ActiveTheme.Path, ActiveTheme.PreviousThemePath, ActiveTheme.BaseDictionaryPath);
                    }

                    var modified_theme = new ThemeRD(newtheme.Path, newtheme.PreviousThemePath, newtheme.BaseDictionaryPath) { };

                    if (ActiveTheme.Path != modified_theme.Path && ActiveTheme.PreviousThemePath != modified_theme.PreviousThemePath)
                    {
                        ActiveTheme = modified_theme;
                        //This will trigger the change and then the controls subscribed to this will also change their theme. But we will not set again.
                    }
                }
                return true;
            }
            return false;
        }
        private bool _changeTheme()
        {
            try
            {
                if (_sender == null)
                {
                    //We cannot get Frameworkelement resources.
                    //So, reset the priroity whatever it was before.
                    _priority = ThemeSearchMode.Application;
                }
                List<ResourceDictionary> _resources = new List<ResourceDictionary>();

                var _elementResources = _getParentResource(ThemeSearchMode.FrameworkElement);
                var _applicationResources = _getParentResource(ThemeSearchMode.Application);

                //Now based on the search priority, we get the resources list and to a list
                switch (_priority)
                {
                    case ThemeSearchMode.FrameworkElement:
                        //First we check the framework element  resources. If we are not able to locate the resource, we move to application level element
                        _resources.Add(_elementResources); //First Prio
                        _resources.Add(_applicationResources); //Second Prio
                        break;
                    case ThemeSearchMode.Application:
                    default:
                        //First we check the application resources. If we are not able to locate the resource, we move to framework element level resources
                        _resources.Add(_applicationResources); //First Prio
                        _resources.Add(_elementResources); //Second Prio

                        break;
                }

                bool _flag = false; //Assuming we did not find and replace theme.

                foreach (var _res in _resources)
                {
                    if (_res == null) continue;
                    if (!_includeHaley && _res.Source != null) //Application level and Framework Element level might not have a source
                    {
                        if (isHaleyDictionary(_res.Source)) continue; //Skipping haley dictionaries
                    }
                    if (_findAndReplace(_res))
                    {
                        if (_priority == ThemeSearchMode.All)
                        {
                            _flag = true; //Reason why we set flag for searchpriority both is that, when we find and replace value in one type (say Application) but we do not find in another (say Framework element), still the end result is positive because we have already replaced in one level.
                            continue;
                        }
                        else
                        {
                            //In case we do not have priority set to both and we managed to find and replace , then we need not proceed further. We just break here.
                            //In case of failure or searchpriroity to both, we proceed further.
                            return true;
                        }
                    }
                }
                return _flag;

            }
            catch (Exception ex)
            {
                if (_raise_notification)
                {
                    _ds.Error("Error", ex.ToString());
                }
                return false;
            }
        }
        private bool _findAndReplace(ResourceDictionary parent_resource)
        {
            if (parent_resource == null) return false; //Sometimes when the object is getting loaded, the RD might not have been loaded and it might result in null

            List<ResourceDictionary> merged_dictionaries = new List<ResourceDictionary>();
            //If we have a specific base dictionary, then it is easy for us. We directly try to find that dictionary. If not, we search all dictionaries.
            if (_source_theme.BaseDictionaryPath != null)
            {
                var base_res = _getBaseDictionary(parent_resource);
                if (base_res != null)
                {
                    merged_dictionaries.Add(base_res);
                }
            }

            //We check the merged dics count and based on that decide whether we need to search all dics or not.
            if (merged_dictionaries.Count == 0)
            {
                if (parent_resource.MergedDictionaries != null)
                {
                    merged_dictionaries = parent_resource.MergedDictionaries?.ToList();
                }
            }

            //Even after this if the merged dictionaries count is zero, we just drop further checking
            if (merged_dictionaries.Count == 0) return false;

            foreach (var _res_dic in merged_dictionaries)
            {
                if (!_includeHaley && _res_dic.Source != null)
                {
                    if (isHaleyDictionary(_res_dic.Source)) continue; //Skipping haley dictionaries
                }
                //try to find and change theme in each dictionary. if we hit positive, we just ignore rest.
                if (_changeTheme(parent_resource, _res_dic))
                {
                    //WE BASICALLY ALTER THE LIST BY ADDING NEW VALUE AND REMOVING EXISTING. SO, AFTER THIS POINT IF WE TRY TO CONTINUE, IT WILL THROW EXCEPTION.
                    return true;
                }
            }
            return false;
        }
        private ResourceDictionary _getParentResource(ThemeSearchMode searchType)
        {
            //We expect either FrameWorkElement or Application
            ResourceDictionary resource = null;
            if (searchType == ThemeSearchMode.FrameworkElement)
            {
                if (_sender != null)
                {
                    resource = ((FrameworkElement)_sender).Resources;
                }
            }
            else
            {
                if (Application.Current != null)
                {
                    resource = Application.Current?.Resources;
                }
            }

            return resource;
        }
        private ResourceDictionary _getBaseDictionary(ResourceDictionary parent_resource)
        {
            ResourceDictionary base_dic = null;
            //We should first validate that basedictionary uri is not empty already before calling this method. However, for safer side, checking again
            if (_source_theme.BaseDictionaryPath == null) return null;
            #region BestOptimum Code
            //Base dictionary processing
            base_dic = parent_resource.MergedDictionaries
                ?.Where(p => p.Source == _source_theme.BaseDictionaryPath)?.FirstOrDefault();

            return base_dic;
            #endregion
        }
        private bool _changeTheme(ResourceDictionary parent_resource, ResourceDictionary base_dictionary)
        {
            ThemeTracker tracker = null;

            var _toplevel_target = parent_resource.MergedDictionaries?.Where(p => p.Source == _source_theme.PreviousThemePath)?.FirstOrDefault(); //Trying to find if theme exists in the parent level itself

            if (_toplevel_target != null)
            {
                tracker = new ThemeTracker(parent_resource, new RDTracker(_toplevel_target, null, true), true);
            }

            if (tracker == null)
            {
                //Loop through all the dictionaries of the resources to find out if it has the particular theme
                tracker = _getOldTheme(base_dictionary);
            }

            if (tracker == null)
            {
                //Should we throw error if we are unable to find old theme????
                //throw new ArgumentException($@"Unable to find the old theme {themeName_to_replace} for replacement");
                return false;
            }

            _replaceTheme(ref tracker);

            if (_toplevel_target != null) return true; //We do not need to change merged dictionaries.

            parent_resource.MergedDictionaries.Insert(0, tracker.Resource); //We are adding it first because, next time when we search for themes, it wil be easy for the algorithm to locate theme at first
            //Remove the base and add it again
            parent_resource.MergedDictionaries.Remove(base_dictionary); //Which is basically the parent of the tracker
            return true;
        }
        private ThemeTracker _getOldTheme(ResourceDictionary target_resource)
        {
            //THIS IS BASICALLY A COMPOSITE LEAF OBJECT
            ThemeTracker tracker = null;
            ResourceDictionary res = null;

            res = target_resource.MergedDictionaries?.Where(p => p.Source == _source_theme.PreviousThemePath)?.FirstOrDefault(); //Trying to find if old theme exists.

            if (res != null)
            {
                tracker = new ThemeTracker(target_resource, new ThemeTracker(res, null, false), true);
                return tracker;
            }

            //Else, loop through all the merged dictionaries and try to find the child.
            foreach (var rd in target_resource.MergedDictionaries)
            {
                if (!_includeHaley && rd.Source != null)
                {
                    if (isHaleyDictionary(rd.Source)) continue; //Skipping haley dictionaries
                }

                var _childtracker = _getOldTheme(rd);
                if (_childtracker != null)
                {
                    //got the first found dictionary.
                    tracker = new ThemeTracker(target_resource, _childtracker, false);
                    //The child of "rd" is the final target. So, this rd is the parent of child tracker.
                    return tracker;
                }
            }
            return tracker; //This could even be null.
        }
        private void _replaceTheme(ref ThemeTracker tracker)
        {
            if (!tracker.is_last)
            {
                var _child = tracker.Child;
                _replaceTheme(ref _child);
            }

            //When you reach the last value, replace it.
            tracker.Resource.MergedDictionaries.Remove(tracker.Child.Resource); //Remove old dictionary
            tracker.Resource.MergedDictionaries
                .Insert(0, new ResourceDictionary() { Source = _source_theme.Path });
        }

        #endregion
    }
}

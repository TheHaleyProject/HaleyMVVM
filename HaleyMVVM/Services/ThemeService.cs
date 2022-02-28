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
        public object ActiveTheme
        {
            get { return _activeTheme; }
            private set
            {
                _activeTheme = value;
            }
        }
        public bool ThrowExceptionsOnFailure { get; set; }
        #endregion

        #region Attributes
        private object _activeTheme;
        private IDialogService _ds = new DialogService();
        private InternalThemeData _internalThemes;
        //private const string _hm_absolute = "pack://application:,,,/Haley.MVVM;component/";
        //private const string _hm_relative = "Haley.MVVM;component/";

        #region InternalThemeLocations
        //private const string _hw_absolute = "pack://application:,,,/Haley.WPF;component/";
        //private const string _hw_relative = "Haley.WPF;component/";
        //private const string _hw_themePrefix = "pack://application:,,,/Haley.WPF;component/Dictionaries/ThemeColors/Theme";
        //private static Uri _hw_themeRoot = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/haleyRD.xaml", UriKind.RelativeOrAbsolute);
        //private static Uri _hw_themeParent = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/haleyBase.xaml", UriKind.RelativeOrAbsolute);
        #endregion

        #region Dictionaries
        private ConcurrentDictionary<string, List<ExternalThemeInfo>> _externalThemes = new ConcurrentDictionary<string, List<ExternalThemeInfo>>();
        private ConcurrentDictionary<Assembly, (ThemeChangeHandler handler, bool beforeChange)> _changeHandlers = new ConcurrentDictionary<Assembly, (ThemeChangeHandler handler, bool beforeChange)>();
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
        private ThemeService() { ThrowExceptionsOnFailure = true; }

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
                var _msg = $@"Call back for the assembly {targetAssembly.GetName().Name} is already registered. An assembly can have only one call back.";
                HandleException(_msg);
                return false;
            }
            return _changeHandlers.TryAdd(targetAssembly, (ChangeCallBack, callBeforeChange));
        }
        public bool AttachInternalTheme(InternalThemeData internal_themedata)
        {
            _internalThemes = internal_themedata;
            return true;
        }
        public List<ThemeInfo> GetThemeInfos(object key)
        {
            try
            {
                if (!getKey(key, out var _key)) return null;

                if(_externalThemes.ContainsKey(_key))
                {
                    _externalThemes.TryGetValue(_key, out var themeData);
                    return themeData?.Cast<ThemeInfo>()?.ToList();
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
               return _externalThemes?.Keys.ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }
        public void SetStartupTheme(object startupKey)
        {
            if (_activeTheme != null)
            {
                var _msg = "Startup theme can be set only when the active theme is empty. Cannot set twice.";
                HandleException(_msg);
                return;
            }
            _activeTheme = startupKey; //This becomes our startup theme.
        }
        public bool IsThemeKeyRegistered(object key)
        {
            try
            {
                if (!getKey(key, out var _key)) return false;
                //The key should be present in either External Themes or internal themes
                bool registered = false;

                registered = _externalThemes.ContainsKey(_key);
                if (!registered && _internalThemes.InfoDic != null)
                {
                    //Check if any internal registration has been made.
                    registered = _internalThemes.InfoDic.ContainsKey(key);
                }
                return registered;
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
            if (!getKey(key, out var _key)) return false;
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (targetAssembly == null)
            {
                throw new ArgumentNullException(nameof(targetAssembly));
            }
            if (!_externalThemes.ContainsKey(_key))
            {
                _externalThemes.TryAdd(_key, new List<ExternalThemeInfo>());
            }

            _externalThemes.TryGetValue(_key, out var themeData);

            if (themeData == null)
            {
                themeData = new List<ExternalThemeInfo>();
            }

            if (themeData.Any(p => p.Source == targetAssembly))
            {
                var _msg = $@"A theme is already registered for the assembly {targetAssembly.GetName().Name} against key {_key}. An assembly should have unique themes for each key.";
                HandleException(_msg);
                return false;
            }

            themeData.Add(new ExternalThemeInfo(value.Name, value.Path) { Source = targetAssembly });

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

            string _msg = null;
            //Check current theme
            if (_activeTheme == null)
            {
                _msg = "Startuptheme is null. Use SetStartupTheme method to set the initial starting theme. Based on the startup theme, the new themes can be changed and applied.";
                HandleException(_msg, "StartupTheme");
                return false;
            }

            //Does current theme have a registered info value?
            if (!IsThemeKeyRegistered(_activeTheme))
            {
                _msg = $@"Startuptheme has to be a valid registered key. Current startup theme {_activeTheme.AsString()} is not a valid key value. Please register the themeinfos using this key or provide a valid startup key.";
                HandleException(_msg);
                return false;
            }
            //The sender can never be null. Do remember to send the assembly as the sender.
            var _caller = Assembly.GetCallingAssembly();
            return ChangeTheme(newThemeKey,ActiveTheme, null,_caller, showNotifications: showNotifications);
        }
        public bool ChangeTheme(object newThemeKey, object oldThemeKey, object frameworkElement, Assembly targetAssembly, SearchPriority priority = SearchPriority.Application, bool raiseChangeEvents = true, bool showNotifications = false)
        {
            string _msg = null;
            //Check current theme
            if (oldThemeKey == null)
            {
                _msg = "Oldtheme key cannot be null. To change a theme, both new and old key values are required.";
                HandleException(_msg, "OldThemeKey");
                return false;
            }

            //Does current theme have a registered info value?
            if (!IsThemeKeyRegistered(oldThemeKey))
            {
                _msg = $@"OldThemeKey is not valid. It doesn't have any registered value. Key: {oldThemeKey.AsString()} ";
                HandleException(_msg);
                return false;
            }

            if (targetAssembly == null)
            {
                _msg = "For changing a theme, the assembly detail is mandatory to fetch the registered themes.";
                HandleException(_msg, nameof(targetAssembly));
                return false;
            }

            if (!getKey(newThemeKey, out var _newKey)) return false;

            if (!IsThemeKeyRegistered(_newKey))
            {
                _msg = $@"Key : {_newKey} is not registered against any internal or external theme infos.";
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
                        _ds.SendToast("No change", $@"Old theme key and new theme key are same. Nothing to change. Key {_newKey}");
                        return false;

                    }
                }    
            }

            //EACH DLL MIGHT HAVE INTERNAL AND ALSO EXTERNAL THEME IN THEIR MERGED DICTIONARIES.

            ChangeInternalTheme(newThemeKey,oldThemeKey);
            //Change external theme  

            if (raiseChangeEvents)
            {
                //Set the new theme.
                ActiveTheme = newThemeKey; //This will raise event and trigger the other controls to change their own themes.
                ThemeChanged?.Invoke(this, (newThemeKey,oldThemeKey));
            }

            return true;
        }
        #endregion

        #region Helpers
        private void ChangeInternalTheme(object newTheme,object oldTheme)
        {
            do
            {
                getKey(newTheme, out var _newthemeKey); //New theme key
                getKey(oldTheme, out var _oldThemeKey); //Old theme key.

                if (_internalThemes == null)
                {
                    Debug.WriteLine("Internal themes data is empty.");
                    break;
                }
                if (!_internalThemes.Themes.ContainsKey(newTheme))
                {
                    Debug.WriteLine($@"Internal theme settings doesn't have any info associated with the key {_newthemeKey}");
                    break;
                }
                if (!_internalThemes.Themes.ContainsKey(oldTheme))
                {
                    Debug.WriteLine($@"Internal theme settings doesn't have any info associated with the key {_oldThemeKey}");
                    break;
                }

                //Get Old Info
                _internalThemes.Themes.TryGetValue(oldTheme, out var oldMode);
                _internalThemes.InfoDic.TryGetValue(oldMode, out var OldThemeInfo);

                //Get New Info
                _internalThemes.Themes.TryGetValue(newTheme, out var newMode);
                _internalThemes.InfoDic.TryGetValue(newMode, out var NewThemeInfo);

                if (OldThemeInfo == null)
                {
                    Debug.WriteLine($@"Themeinfo for the key {_oldThemeKey} is empty. Cannot change anything.");
                    break;
                }

                if (NewThemeInfo == null)
                {
                    Debug.WriteLine($@"Themeinfo for the key {_newthemeKey} is empty. Cannot change anything.");
                    break;
                }

                if (OldThemeInfo.Path == null || NewThemeInfo.Path == null)
                {
                    Debug.WriteLine("For changing the internal theme, the themeinfo Paths should not be null.");
                    break;
                }

                if (OldThemeInfo.Path == NewThemeInfo.Path)
                {
                    Debug.WriteLine("Internal themes are same, Cannot change.");
                }

                //Get old theme and the new theme info and change them.
                ThemeChangeData _changeData = new ThemeChangeData();
                _changeData.OldTheme = _internalThemes.InfoDic.TryGetValue(ActiveTheme,)
            } while (false);
        }
        private void HandleException(string message, string paramName = null)
        {
            if (ThrowExceptionsOnFailure)
            {
                if (!string.IsNullOrWhiteSpace(paramName))
                {
                    throw new ArgumentNullException(paramName, message);
                }
                throw new ArgumentException(message);
            }
            Debug.WriteLine(message);
        }
        private bool getKey(object key, out string keyStr)
        {
            keyStr = null;
            if (key == null)
            {
                HandleException("The theme key cannot be empty. Please provide a valid key for processing.", nameof(key));
                return false;
            }

            if (key is string _key_str)
            {
                keyStr = _key_str.ToLower();
            }
            keyStr =  key.AsString()?.ToLower() ?? key.ToString();
            return true;
        }
        private bool changeInternalTheme(InternalThemeMode mode, bool showNotifications = false)
        {
            if (InternalTheme.Value == mode)
            {
                if (showNotifications)
                {
                    Debug.WriteLine("Same Internal Mode", $@"There is no change in mode. Current mode is {mode.ToString()}");
                }
                return false; //We already are at same mode.
            }

            if (InternalTheme == null)
            {
                return false;
            }

            var _new_uri = _getInternalURI(mode);
            var _old_uri = _getInternalURI(InternalTheme.Value);

            if (_new_uri == null || _old_uri == null) return false; //Don't proceed if internal uri is not fetchables.
            ThemeChangeData _changeData = new ThemeChangeData() {OldTheme =  }
            ThemeRD internal_new_theme = new ThemeRD(_new_uri, _old_uri, _hw_themeRoot) { };
            if (_changeTheme(null, internal_new_theme, SearchPriority.Application, false, false, true, is_internal_call: true))
            {
                InternalTheme = mode;
                return true;
            }
            return false;
        }
        private void _getCurrentInternalTheme()
        {
            //this is just or processing and finding out the mode
            try
            {
                //From Base, we need to get the first theme that matches a pattern
                ResourceDictionary _base = new ResourceDictionary() { Source = _hw_themeParent };
                var theme_URI = _base?.MergedDictionaries?.FirstOrDefault(_md => (_md.Source?.OriginalString.StartsWith(_hw_themePrefix)).Value).Source?.OriginalString;
                var name_with_ex = theme_URI.Substring(_hw_themePrefix.Length);
                var name_plain = name_with_ex.Substring(0, name_with_ex.Length - 5);
                foreach (var item in Enum.GetValues(typeof(InternalThemeMode)))
                {
                    InternalThemeMode _theme = (InternalThemeMode)item;
                    if (name_plain.Equals(_theme.ToString()))
                    {
                        InternalTheme = _theme;
                        return;
                    }
                }
                InternalTheme = InternalThemeMode.Normal;
            }
            catch (Exception)
            {
                InternalTheme = InternalThemeMode.Normal;
            }
        }
        private bool _changeTheme(DependencyObject sender, ThemeRD newtheme, SearchPriority priority, bool compare_with_active_theme, bool raise_notification, bool include_haley_dictionaries = false, bool is_internal_call = false)
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
        private Uri _getInternalURI(InternalThemeMode mode)
        {
            Uri actual = new Uri($@"{_hw_themePrefix}{mode.ToString()}.xaml", UriKind.RelativeOrAbsolute);
            return actual;
        }
        private bool isHaleyDictionary(Uri _tocheck)
        {
            if (_tocheck == null) return false;
            if (
                 _tocheck.OriginalString.ToLower().StartsWith(_hw_absolute.ToLower()) ||
                 _tocheck.OriginalString.ToLower().StartsWith(_hw_relative.ToLower()) ||
                 _tocheck.OriginalString.ToLower().StartsWith(_hm_absolute.ToLower()) ||
                 _tocheck.OriginalString.ToLower().StartsWith(_hm_relative.ToLower())
                )
            {
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
                    _priority = SearchPriority.Application;
                }
                List<ResourceDictionary> _resources = new List<ResourceDictionary>();

                var _elementResources = _getParentResource(SearchPriority.FrameworkElement);
                var _applicationResources = _getParentResource(SearchPriority.Application);

                //Now based on the search priority, we get the resources list and to a list
                switch (_priority)
                {
                    case SearchPriority.FrameworkElement:
                        //First we check the framework element  resources. If we are not able to locate the resource, we move to application level element
                        _resources.Add(_elementResources); //First Prio
                        _resources.Add(_applicationResources); //Second Prio
                        break;
                    case SearchPriority.Application:
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
                        if (_priority == SearchPriority.All)
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
        private ResourceDictionary _getParentResource(SearchPriority searchType)
        {
            //We expect either FrameWorkElement or Application
            ResourceDictionary resource = null;
            if (searchType == SearchPriority.FrameworkElement)
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

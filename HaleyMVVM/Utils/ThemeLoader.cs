using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Haley.Models;
using Haley.Services;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Utils;
using System.Windows.Data;
using System.Collections.Concurrent;

namespace Haley.Utils
{
    public class ThemeLoader :DependencyObject
    {
        #region Public Properties
        public ThemeMode InternalTheme { get; private set; }
        public event EventHandler<Theme> ActiveThemeChanged;

        #endregion

        #region ATTRIBUTES
        internal Theme OldTheme { get; private set; }

        private Theme activeTheme;

        internal Theme ActiveTheme
        {
            get { return activeTheme; }
            private set 
            {
                activeTheme = value;
                ActiveThemeChanged?.Invoke(this, activeTheme);
            }
        }

        private bool _raise_notification;
        private SearchPriority _priority;
        private Theme _source_theme;
        private DependencyObject _sender;
        private IDialogService _ds = new DialogService();
        private bool _includeHaley;
        private ConcurrentDictionary<string, Theme> _themeDic = new ConcurrentDictionary<string, Theme>();

        private const string _hw_absolute = "pack://application:,,,/Haley.WPF;component/";
        private const string _hm_absolute = "pack://application:,,,/Haley.MVVM;component/";
        private const string _hw_relative = "Haley.WPF;component/";
        private const string _hm_relative = "Haley.MVVM;component/";
        private const string _hw_theme_start = "pack://application:,,,/Haley.WPF;component/Dictionaries/ThemeColors/Theme";
        #endregion

        #region HaleyThemes
        private static Uri _hw_RD = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/haleyRD.xaml",UriKind.RelativeOrAbsolute);
        private static Uri _hw_base = new Uri("pack://application:,,,/Haley.WPF;component/Dictionaries/haleyBase.xaml", UriKind.RelativeOrAbsolute);
        #endregion

        #region Constructor
        public static ThemeLoader Singleton = new ThemeLoader();
        public static ThemeLoader getSingleton()
        {
            if (Singleton == null) Singleton = new ThemeLoader();
            return Singleton;
        }
        public static void Clear()
        {
            Singleton = new ThemeLoader();
        }
        private ThemeLoader() { _getCurrentInternalTheme(); }
        #endregion

        public bool ChangeTheme(Theme newtheme,bool showNotifications = false)
        {
            return ChangeTheme(null, newtheme, SearchPriority.Application,raise_notification:showNotifications);
        }

        public bool ChangeTheme(DependencyObject sender, Theme newtheme, SearchPriority priority = SearchPriority.Application, bool compare_with_active_theme=true,bool raise_notification =false)
        {
            return _changeTheme(sender, newtheme, priority, compare_with_active_theme, raise_notification);
        }
        public bool ChangeInternalTheme(ThemeMode mode, bool showNotifications = false)
        {
            if (InternalTheme == mode)
            {
                if (showNotifications)
                {
                    _ds.SendToast("Same Internal Mode", $@"There is no change in mode. Current mode is {mode.ToString()}");
                }
                return false; //We already are at same mode.
            }

            var _new_uri = _getInternalURI(mode);
            var _old_uri = _getInternalURI(InternalTheme);

            if (_new_uri == null || _old_uri== null) return false; //Don't proceed if internal uri is not fetchables.

            Theme internal_new_theme = new Theme(_new_uri,_old_uri,_hw_RD) {};
            if ( _changeTheme(null, internal_new_theme, SearchPriority.Application, false, false, true,is_internal_call:true))
            {
                InternalTheme = mode;
                return true;
            }
            return false;
        }

        public Theme RegisterThemeURI(string key,Uri theme_uri)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty");
            if (!_themeDic.ContainsKey(key))
            {
                _themeDic.TryAdd(key, new Theme(theme_uri, null));
            }
            _themeDic.TryGetValue(key, out var _reslt);
            return _reslt;
        }

        public bool IsThemeRegistered(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be empty");
            return _themeDic.ContainsKey(key);
        }

        public Theme GetTheme(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            _themeDic.TryGetValue(key, out var _reslt);
            return _reslt;
        }

        public bool ChangeThemeByKey(string source_key, string target_key = null,bool showNotifications = false)
        {
            if (string.IsNullOrWhiteSpace(source_key)) return false;
            if (!_themeDic.ContainsKey(source_key)) return false;
            Theme new_theme = null;
            _themeDic.TryGetValue(source_key, out new_theme);
            if (new_theme == null) return false;

            if (!string.IsNullOrWhiteSpace(target_key))
            {
                if (!_themeDic.ContainsKey(target_key)) return false;
                _themeDic.TryGetValue(target_key, out var target_theme);

                new_theme.PreviousThemePath = target_theme.Path;
            }
            return ChangeTheme(new_theme,showNotifications);
        }

        public bool SetStartupTheme(string source_key)
        {
            if (string.IsNullOrWhiteSpace(source_key)) return false;
            if (!_themeDic.ContainsKey(source_key)) return false;
            if (ActiveTheme != null) return false;
            _themeDic.TryGetValue(source_key, out var new_theme);
            ActiveTheme = new_theme; //This will not contain any previouspath (as it is registered using the URI only). So, this will not trigger the ThemeAP prop change.
            return true;

        }

        #region Helpers
        private void _getCurrentInternalTheme()
        {
            //this is just or processing and finding out the mode
            try
            {
                //From Base, we need to get the first theme that matches a pattern
                ResourceDictionary _base = new ResourceDictionary() { Source = _hw_base };
                var theme_URI = _base?.MergedDictionaries?.FirstOrDefault(_md => (_md.Source?.OriginalString.StartsWith(_hw_theme_start)).Value).Source?.OriginalString;
                var name_with_ex = theme_URI.Substring(_hw_theme_start.Length);
                var name_plain = name_with_ex.Substring(0, name_with_ex.Length - 5);
                foreach (var item in Enum.GetValues(typeof(ThemeMode)))
                {
                    ThemeMode _theme = (ThemeMode)item;
                    if (name_plain.Equals(_theme.ToString()))
                    {
                        InternalTheme = _theme;
                        return;
                    }
                }
                InternalTheme = ThemeMode.Normal;
            }
            catch (Exception)
            {
                InternalTheme = ThemeMode.Normal;
            }
        }
        private bool _changeTheme(DependencyObject sender, Theme newtheme, SearchPriority priority, bool compare_with_active_theme,bool raise_notification,bool include_haley_dictionaries = false, bool is_internal_call = false)
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
                        OldTheme = new Theme(ActiveTheme.Path, ActiveTheme.PreviousThemePath, ActiveTheme.BaseDictionaryPath);
                    }

                    var modified_theme = new Theme(newtheme.Path, newtheme.PreviousThemePath, newtheme.BaseDictionaryPath) { };

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
        private Uri _getInternalURI(ThemeMode mode)
        {
            Uri actual = new Uri($@"{_hw_theme_start}{mode.ToString()}.xaml", UriKind.RelativeOrAbsolute);
            return actual;
        }
       
        private bool _isHaleyDictionary(Uri _tocheck)
        {
            if (_tocheck == null) return false;
            if (
                 _tocheck.OriginalString.ToLower().StartsWith(_hw_absolute.ToLower()) ||
                 _tocheck.OriginalString.ToLower().StartsWith(_hw_relative.ToLower()) ||
                 _tocheck.OriginalString.ToLower().StartsWith(_hm_absolute.ToLower())||
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
                        if (_isHaleyDictionary(_res.Source)) continue; //Skipping haley dictionaries
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
                    if (_isHaleyDictionary(_res_dic.Source)) continue; //Skipping haley dictionaries
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
            RDTracker tracker = null;

            var _toplevel_target = parent_resource.MergedDictionaries?.Where(p => p.Source == _source_theme.PreviousThemePath)?.FirstOrDefault(); //Trying to find if theme exists in the parent level itself

            if (_toplevel_target != null)
            {
                tracker = new RDTracker(parent_resource, new RDTracker(_toplevel_target, null,true), true);
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

            parent_resource.MergedDictionaries.Insert(0, tracker.resource); //We are adding it first because, next time when we search for themes, it wil be easy for the algorithm to locate theme at first
            //Remove the base and add it again
            parent_resource.MergedDictionaries.Remove(base_dictionary); //Which is basically the parent of the tracker
            return true;
        }
        private RDTracker _getOldTheme(ResourceDictionary target_resource)
        {
            //THIS IS BASICALLY A COMPOSITE LEAF OBJECT
            RDTracker tracker = null;
            ResourceDictionary res = null;

            res = target_resource.MergedDictionaries?.Where(p => p.Source == _source_theme.PreviousThemePath)?.FirstOrDefault(); //Trying to find if old theme exists.

            if (res != null)
            {
                tracker = new RDTracker(target_resource, new RDTracker(res, null, false), true);
                return tracker;
            }

            //Else, loop through all the merged dictionaries and try to find the child.
            foreach (var rd in target_resource.MergedDictionaries)
            {
                if (!_includeHaley && rd.Source != null)
                {
                    if (_isHaleyDictionary(rd.Source)) continue; //Skipping haley dictionaries
                }

                var _childtracker = _getOldTheme(rd);
                if (_childtracker != null)
                {
                    //got the first found dictionary.
                    tracker = new RDTracker(target_resource, _childtracker, false);
                    //The child of "rd" is the final target. So, this rd is the parent of child tracker.
                    return tracker;
                }
            }
            return tracker; //This could even be null.
        }
        private void _replaceTheme(ref RDTracker tracker)
        {
            if (!tracker.is_last)
            {
                var _child = tracker.child;
                _replaceTheme(ref _child);
            }

            //When you reach the last value, replace it.
            tracker.resource.MergedDictionaries.Remove(tracker.child.resource); //Remove old dictionary
            tracker.resource.MergedDictionaries
                .Insert(0,new ResourceDictionary() { Source = _source_theme.Path });
        }
        #endregion
    }
}
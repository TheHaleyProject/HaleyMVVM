using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Haley.Models;
using Haley.MVVM.Services;
using Haley.Abstractions;
using Haley.Enums;
using Haley.Utils;
using System.Windows.Data;


namespace Haley.Utils
{
    public class ThemeLoader :DependencyObject
    {
        #region Public Properties
        public Theme old_theme { get; private set; }

        public Theme active_theme
        {
            get { return (Theme)GetValue(active_themeProperty); }
            //set { SetValue(active_themeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for active_theme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty active_themeProperty =
            DependencyProperty.Register("active_theme", typeof(Theme), typeof(ThemeLoader), new PropertyMetadata(null));

        public ThemeMode current_internal_mode { get; private set; }

        #endregion


        #region ATTRIBUTES
        private bool _raise_notification;
        private SearchPriority _priority;
        private Theme _newtheme;
        private DependencyObject _sender;
        private IDialogService _ds = new DialogService();
        private bool _includeHaley;
       
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

        public bool changeTheme(Theme newtheme)
        {
            return changeTheme(null, newtheme, SearchPriority.Application);
        }

        public bool changeTheme(DependencyObject sender, Theme newtheme, SearchPriority priority = SearchPriority.Application, bool compare_with_active_theme=true,bool raise_notification =false)
        {
            return _changeTheme(sender, newtheme, priority, compare_with_active_theme, raise_notification);
        }
        public bool changeInternalTheme(ThemeMode mode)
        {
            if (current_internal_mode == mode) return false; //We already are at same mode.
            var _new_uri = _getInternalURI(mode);
            var _old_uri = _getInternalURI(current_internal_mode);

            if (_new_uri == null || _old_uri== null) return false; //Don't proceed if internal uri is not fetchables.

            Theme internal_new_theme = new Theme(_new_uri,_old_uri,_hw_RD) {};
            if ( _changeTheme(null, internal_new_theme, SearchPriority.Application, false, false, true,true))
            {
                current_internal_mode = mode;
                return true;
            }
            return false;
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
                        current_internal_mode = _theme;
                        return;
                    }
                }
                current_internal_mode = ThemeMode.Normal;
            }
            catch (Exception)
            {
                current_internal_mode = ThemeMode.Normal;
            }
        }
        private bool _changeTheme(DependencyObject sender, Theme newtheme, SearchPriority priority, bool compare_with_active_theme,bool raise_notification,bool include_haley_dictionaries = false, bool is_internal_cal = false)
        {
            //Priliminary verifications
            if (newtheme == null || newtheme?.new_theme_uri == null || newtheme?.old_theme_uri == null)
            {
                if (raise_notification)
                {
                    _ds.send("Error", $@"The theme URIs cannot be empty. Please fill both new and old theme values.");
                }
                return false;
            }
            //COMPARE WITH ACTUAL OLD THEME
            if (active_theme != null && compare_with_active_theme)
            {
                //Basically, old theme's new uri is the actual value. So, actual value and newtheme's new value should not be equal.
                //If we try to set the same theme again, do not respond.
                if (active_theme.new_theme_uri == newtheme.new_theme_uri)
                {
                    if (raise_notification)
                    {
                        _ds.send("Conflict", $@"The theme ""{newtheme.new_theme_uri}"" is the current theme. Nothing to change. Please check");
                    }
                    return false;
                }
            }

            //COMPARE WITH USER PROVIDED REPLACE THEME
            //if old and new theme are same, don't do anything.
            if (newtheme.new_theme_uri.Equals(newtheme.old_theme_uri))
            {
                if (raise_notification)
                {
                    _ds.send("Conflict", $@"The theme ""{newtheme.new_theme_uri}"" is the current theme. Nothing to change. Please check");
                }
                return false;
            }

            //SET ATTRIBUTES ONLY AFTER ABOVE VALIDATIONS.
            _raise_notification = raise_notification; //May change each time.
            _priority = priority;
            _newtheme = newtheme;
            _sender = sender;
            _includeHaley = include_haley_dictionaries;

            if (_changeTheme())
            {
                //If we are changing for Haley Internal, we do not change other values
                if (!is_internal_cal)
                {
                    if (active_theme != null)
                    {
                        old_theme = new Theme(active_theme.new_theme_uri, active_theme.old_theme_uri, active_theme.base_dictionary_uri);
                    }

                    var modified_theme = new Theme(newtheme.new_theme_uri, newtheme.old_theme_uri, newtheme.base_dictionary_uri) { };
                    //SetProp(ref _active_theme, modified_theme); //Set prop to trigger property changed.
                    SetValue(active_themeProperty, modified_theme);
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
                        if (_elementResources != null)
                        {
                            _resources.Add(_elementResources);
                        }
                        _resources.Add(_applicationResources);
                        break;
                    case SearchPriority.Application:
                    default:
                        //First we check the application resources. If we are not able to locate the resource, we move to framework element level resources
                        _resources.Add(_applicationResources);
                        if (_elementResources != null)
                        {
                            _resources.Add(_elementResources);
                        }
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
                        if (_priority == SearchPriority.Both)
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
                    _ds.send("Error", ex.ToString());
                }
                return false;
            }
        }
        private bool _findAndReplace(ResourceDictionary parent_resource)
        {
            if (parent_resource == null) return false; //Sometimes when the object is getting loaded, the RD might not have been loaded and it might result in null

            List<ResourceDictionary> merged_dictionaries = new List<ResourceDictionary>();
            //If we have a specific base dictionary, then it is easy for us. We directly try to find that dictionary. If not, we search all dictionaries.
            if (_newtheme.base_dictionary_uri != null)
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
                resource = Application.Current.Resources;
            }

            return resource;
        }
        private ResourceDictionary _getBaseDictionary(ResourceDictionary parent_resource)
        {
            ResourceDictionary base_dic = null;
            //We should first validate that basedictionary uri is not empty already before calling this method. However, for safer side, checking again
            if (_newtheme.base_dictionary_uri == null) return null;
            #region BestOptimum Code
            //Base dictionary processing
            base_dic = parent_resource.MergedDictionaries
                ?.Where(p => p.Source == _newtheme.base_dictionary_uri)?.FirstOrDefault();

            return base_dic;
            #endregion
        }
        private bool _changeTheme(ResourceDictionary parent_resource, ResourceDictionary base_dictionary)
        {
            //Loop through all the dictionaries of the resources to find out if it has the particular theme
            var tracker = _getOldTheme(base_dictionary);


            if (tracker == null)
            {
                //Should we throw error if we are unable to find old theme????
                //throw new ArgumentException($@"Unable to find the old theme {themeName_to_replace} for replacement");
                return false;
            }

            _replaceTheme(ref tracker);

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

            res = target_resource.MergedDictionaries?.Where(p => p.Source == _newtheme.old_theme_uri)?.FirstOrDefault(); //Trying to find if old theme exists.

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
                .Add(new ResourceDictionary() { Source = _newtheme.new_theme_uri });
        }
        #endregion
    }
}
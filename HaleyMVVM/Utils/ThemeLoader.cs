using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Haley.Models;

namespace Haley.Utils
{
    public static class ThemeLoader
    {
        public static void changeTheme(DependencyObject sender, Theme newtheme)
        {
            changeTheme(sender, newtheme.theme_PackURI, newtheme.theme_to_replace, newtheme.base_dictionary_name);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="new_themeURI">The New URI for the theme.</param>
        /// <param name="theme_to_replace">The Name of Old theme which needs to be replaced</param>
        /// <param name="base_dictionary_name">The parent dictinary which contains the Theme</param>
        public static void changeTheme(
            DependencyObject sender,
            string new_themeURI,
            string theme_to_replace,
            string base_dictionary_name = null)
        {
            try
            {
                //if old and new theme are same, don't do anything.
                if(new_themeURI.EndsWith(theme_to_replace + ".xaml"))
                    return;

                ResourceDictionary resource = null;
                if(sender == null)
                {
                    throw new ArgumentException("Dependency object cannot be null for changing the theme");
                }

                if(sender is UserControl)
                {
                    resource = ((UserControl)sender).Resources;
                } else if(sender is Window)
                {
                    resource = ((Window)sender).Resources;
                }

                if(resource == null)
                    return; //Sometimes when the object is getting loaded, the RD might not have been loaded and it might result in null

                var base_dictionary = getBaseDictionary(resource, base_dictionary_name);

                //Base dictionary cannot be null. Because if an application is using it then it means that it has some dictionary. Try application resources
                if (base_dictionary == null)
                {
                    base_dictionary = getBaseDictionary(Application.Current.Resources, base_dictionary_name);
                }
                //TODO: Do we need to create new dictionary? or try to get the application resources? to change the theme?
                //At present, if the base is not present, stop processing.
                if(base_dictionary == null)
                {
                    throw new ArgumentException($@"Unable to get the base dictionary from {sender.GetType().FullName}");
                }
                ;

                _changeTheme(resource, base_dictionary, new_themeURI, theme_to_replace);
            } catch(Exception)
            {
                throw;
            }
        }

        private static ResourceDictionary getBaseDictionary(
            ResourceDictionary Resource,
            string base_dictionary_name = null)
        {
            ResourceDictionary base_dic = null;
            #region BestOptimum Code
            //If base dictionary name is not available, then use the first dictonary that is present.
            if(string.IsNullOrEmpty(base_dictionary_name))
            {
                //Base dictionary processing
                base_dic = Resource.MergedDictionaries?.FirstOrDefault();
            } else
            {
                //Base dictionary processing
                base_dic = Resource.MergedDictionaries
                    ?.Where(p => p.Source.OriginalString.EndsWith("/"+base_dictionary_name + ".xaml"))?.FirstOrDefault();
            }

            return base_dic;
            #endregion
        }

        private static void _changeTheme(
            ResourceDictionary Resources,
            ResourceDictionary base_dictionary,
            string new_theme_URI,
            string theme_to_replace)
        {
            //Loop through all the dictionaries of the resources to find out if it has the particular theme
            var tracker = _getOldTheme(base_dictionary, theme_to_replace);


            if(tracker == null)
            {
                //Should we throw error if we are unable to find old theme????
                //throw new ArgumentException($@"Unable to find the old theme {theme_to_replace} for replacement");
                return;
            }

            _replaceTheme(ref tracker, new_theme_URI);

            Resources.MergedDictionaries.Insert(0, tracker.resource);
            //Remove the base and add it again
            Resources.MergedDictionaries.Remove(base_dictionary); //Which is basically the parent of the tracker
            
        }

        private static RDTracker _getOldTheme(ResourceDictionary Resource, string theme_to_replace)
        {
            RDTracker tracker = null;
            ResourceDictionary res = null;

            res = Resource.MergedDictionaries?.Where(p => p.Source.OriginalString.EndsWith("/"+theme_to_replace + ".xaml"))?.FirstOrDefault(
                );

            if(res != null)
            {
                tracker = new RDTracker(Resource, new RDTracker(res, null, false), true);
                return tracker;
            }

            //Else, loop through all the merged dictionaries and try to find the child.
            foreach(var rd in Resource.MergedDictionaries)
            {
                var _childtracker = _getOldTheme(rd, theme_to_replace);
                if(_childtracker != null)
                {
                    //got the first found dictionary.
                    tracker = new RDTracker(Resource, _childtracker, false);
                    //The child of "rd" is the final target. So, this rd is the parent of child tracker.
                    return tracker;
                }
            }
            return tracker; //This could even be null.
        }

        private static void _replaceTheme(ref RDTracker tracker, string newPackURI)
        {
            if(!tracker.is_last)
            {
                var _child = tracker.child;
                _replaceTheme(ref _child, newPackURI);
            }

            //When you reach the last value, replace it.
            tracker.resource.MergedDictionaries.Remove(tracker.child.resource); //Remove old dictionary
            tracker.resource.MergedDictionaries
                .Add(new ResourceDictionary() { Source = new Uri(newPackURI, UriKind.RelativeOrAbsolute) });
        }
    }
}
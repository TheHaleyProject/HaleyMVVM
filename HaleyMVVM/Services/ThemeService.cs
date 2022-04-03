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
    public sealed class ThemeService : IThemeService
    {
        #region Properties
        public event EventHandler<object> ThemeChanged;
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
        public ThemeReplaceMode ReplaceMode { get; set; }
        #endregion

        #region Attributes
        InternalThemeProvider InternalThemes;
        private object internalLock = new object(); //This internal lock is a REENTRANT (for samethread, meaning it can be locked multiple times inside the same thread).
        private object _activeTheme;
        private IDialogService _ds = new DialogService();
        private bool _internalThemeInitialized = false;
        private List<string> _failedGroups = new List<string>();
        private bool _registrationsValidated = false;

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
            ReplaceMode = ThemeReplaceMode.IgnoreFindReplaceAndCreateCopy;
        }
        #endregion

        #region Public Methods
        public bool BuildAndFillMissing()
        {
            try
            {
                ClearFailedGroups();

                //Fill missing themes with cross reference.
                var allKeys = GetAllThemeKeys();

                FillMissingThemes(ThemeRegistrationMode.Independent, allKeys); //IMPORTANT: INDEPENDENT FIRST BECAUSE ASSEMBLY BASED USES INDEPENDENT FOR CROSS REFERENCE.
                FillMissingThemes(ThemeRegistrationMode.AssemblyBased, allKeys);

                _registrationsValidated = true;
            }
            catch (Exception ex)
            {
                HandleException(ex.ToString());
                _registrationsValidated = false;
            }
            return _registrationsValidated;

        }
        public List<ThemeInfo> GetAllRegisteredThemeInfos()
        {
            var _allAsmGroups = GetAllGroupInfo(ThemeRegistrationMode.AssemblyBased);
            var _allIndpndGroups = GetAllGroupInfo(ThemeRegistrationMode.Independent);

            List<ThemeInfo> allInfos = new List<ThemeInfo>();
            allInfos.AddRange(_allAsmGroups.AllValues);
            allInfos.AddRange(_allIndpndGroups.AllValues);
            return allInfos;
        }
        public List<ThemeInfo> GetThemeInfos(object key, ThemeRegistrationMode dicType)
        {
            try
            {
                if (!IsKeyValid(key)) return null;

                switch (dicType)
                {
                    case ThemeRegistrationMode.Independent:
                        if (_globalThemes.ContainsKey(key))
                        {
                            _globalThemes.TryGetValue(key, out var globalThemedata);
                            return globalThemedata;
                        }
                        break;
                    case ThemeRegistrationMode.AssemblyBased:
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
        public List<object> GetThemes(ThemeRegistrationMode dicType)
        {
            try
            {
                switch (dicType)
                {
                    case ThemeRegistrationMode.Independent:
                        return _globalThemes?.Keys.ToList();
                    case ThemeRegistrationMode.AssemblyBased:
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
        public List<object> GetAllThemeKeys()
        {
            List<object> _allKeys = new List<object>();
            _allKeys.AddRange(GetThemes(ThemeRegistrationMode.AssemblyBased)?? new List<object>());
            _allKeys.AddRange(GetThemes(ThemeRegistrationMode.Independent)?? new List<object>());

            return _allKeys.Distinct().ToList();
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
        public bool SetupInternalTheme(Func<InternalThemeProvider> provider)
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
                registered = IsThemeKeyRegistered(key, ThemeRegistrationMode.Independent);
                if (registered) return true;
                registered = IsThemeKeyRegistered(key, ThemeRegistrationMode.AssemblyBased);
                return registered;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsThemeKeyRegistered(object key,ThemeRegistrationMode dicType)
        {
            if (!IsKeyValid(key)) return false;
            switch (dicType)
            {
                case ThemeRegistrationMode.Independent:
                    return (_globalThemes.ContainsKey(key));
                case ThemeRegistrationMode.AssemblyBased:
                    return (_externalThemes.ContainsKey(key));
            }
            return false;
        }
        /// <summary>
        /// Register a theme group.
        /// </summary>
        /// <param name="builder">Use any one of the three themeBuilders: AssemblyThemeBuilder, IndependentThemeBuilder, InternalThemeBuilder</param>
        /// <returns></returns>
        public string RegisterGroup(ThemeBuilderBase builder)
        {
           if (builder is InternalThemeBuilder intrnlbldr)
            {
                return RegisterInternal(intrnlbldr.GetThemeGroup() as Dictionary<object,InternalThemeMode>);
            }

           if (builder is IndependentThemeBuilder indpntbldr)
            {
                if (builder is AssemblyThemeBuilder asmbldr)
                {
                    if (asmbldr.Target == null)
                    {
                        //Get the calling assembly which will be the one before the method (so directly the caller)
                        asmbldr.Target = Assembly.GetCallingAssembly();
                    }
                    //Assembly builder is an extension of Independent builder
                    return RegisterAssembly(asmbldr.GetThemeGroup() as Dictionary<object, Uri>, asmbldr.Target,asmbldr.IndependentGroupReferenceId);
                }
                //Independent builder
                return RegisterIndependent(indpntbldr.GetThemeGroup() as Dictionary<object, Uri>);
            }
            return null;
        }
        #endregion

        #region Registration Helpers
        private string AttachGroupIds(object themeDicObject, Func<string, bool> regDelegate)
        {
            if (themeDicObject == null)
            {
                HandleException($@"Dictionary values cannot be empty", "ThemeGroup");
                return null;
            }

            string Gid = Guid.NewGuid().ToString();
            if (regDelegate.Invoke(Gid))
            {
                _registrationsValidated = false; //Whenever we successfully register something, it should be validated.
                return Gid;
            }
            _failedGroups.Add(Gid); //Can be used later to remove.
            return null;
        }
        private string RegisterInternal(Dictionary<object, InternalThemeMode> themeGroup)
        {
            //The internal themes should be initialized.
            if (!_internalThemeInitialized)
            {
                HandleException($@"Internal themes are not yet initialized. Please use the method {nameof(SetupInternalTheme)} to setup the internal themes.", "Internal Themes");
                return null;
            }

            return AttachGroupIds(themeGroup, (Gid) =>
            {
                foreach (var kvp in themeGroup)
                {
                    if (!RegisterInternal(kvp.Key, kvp.Value, Gid)) //Register under same group id.
                    {
                        return false; //Break and return because we failed to register.
                    }
                }
                return true;
            });
        }
        private bool RegisterInternal(object key, InternalThemeMode mode, string groupId)
        {
            //The internal themes should be initialized.
            if (!_internalThemeInitialized)
            {
                HandleException($@"Internal themes are not yet initialized. Please use the method {nameof(SetupInternalTheme)} to setup the internal themes.", "Internal Themes");
                return false;
            }

            if (!InternalThemes.Location.ContainsKey(mode))
            {
                HandleException($@"Internal themes doesn't have any registered themeinfo related to the key {mode.ToString()}", "Internal Themes");
                return false;
            }

            if (!InternalThemes.Location.TryGetValue(mode, out var _internalURI)) return false;

            RegisterIndependent(key, _internalURI, groupId,true); //For internal add copy at root.

            return true;
        }
        private string RegisterIndependent(Dictionary<object, Uri> themeGroup)
        {
            return AttachGroupIds(themeGroup, (Gid) =>
            {
                foreach (var kvp in themeGroup)
                {
                    if (!RegisterIndependent(kvp.Key, kvp.Value, Gid)) //Register under same group id.
                    {
                        return false; //Break and return because we failed to register.
                    }
                }
                return true;
            });
        }
        private bool RegisterIndependent(object key, Uri value, string groupId,bool addCopyAtRoot = false)
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
            if (_themeList.Any(p => p.Path == value && p.GroupId == groupId)) //is alresady present.
            {
                Debug.WriteLine($@"The theme with path {value} is already registered for the key {GetKeyString(key)} with similar groupid.");
                return true; //Already added. 
            }
            lock (internalLock)
            {
                _themeList.Add(new ThemeInfo(value, groupId) {AddCopyAtRoot = addCopyAtRoot });
            }
            return true;
        }
        private string RegisterAssembly(Dictionary<object, Uri> themeGroup, Assembly targetAssembly,string independentGroupReferenceId)
        {
            return AttachGroupIds(themeGroup, (Gid) =>
            {
                foreach (var kvp in themeGroup)
                {
                    if (!RegisterAssembly(kvp.Key, kvp.Value, targetAssembly, Gid, independentGroupReferenceId)) //Register under same group id.
                    {
                        return false; //Break and return because we failed to register.
                    }
                }
                return true;
            });
        }
        private bool RegisterAssembly(object key, Uri value, Assembly targetAssembly, string groupId, string independentGroupReferenceId)
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
                themeData.Add(new ThemeInfoEx(value,groupId) { SourceAssembly = targetAssembly,CrossReferenceId = independentGroupReferenceId});
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
            //The sender can never be null. Do remember to send the assembly as the sender.
            //var _caller = Assembly.GetCallingAssembly(); //this will cause issues as calling can happen from anywhere.
            //Irrespective of where it is getting called, it will always use the .Exe assembly (which will ofcouse contain the App.Xaml or the App.Resources).
            var _caller = Assembly.GetEntryAssembly(); 
            return Validate(newThemeKey, null,_caller);
        }
        public bool ChangeTheme(object newThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode = ThemeSearchMode.Application)
        {
            return Validate(newThemeKey, frameworkElement, targetAssembly, searchMode, false); //Here we will not raise the change events as we give the user option to use the assembly. Internal assembly change should not raise anything.
        }
        public void SyncAllWithActiveTheme()
        {
            ThemeChanged?.Invoke(this, ActiveTheme??StartupTheme); //This will basically force all the synchronize with the active.
        }
        #endregion

        #region Core Implementations
        private bool Validate(object newThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode = ThemeSearchMode.Application, bool raiseChangeEvents = true)
        {
            if (!_registrationsValidated)  BuildAndFillMissing(); //For all new registrations, we need to validate if the counts match across all groups. We also remove the failed groups.

            string _msg = null;

            if (targetAssembly == null)
            {
                _msg = "For changing a theme, the assembly detail is mandatory to fetch the registered themes.";
                HandleException(_msg, nameof(targetAssembly));
                return false;
            }

            if (!IsKeyValid(newThemeKey)) return false;
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
                if (ActiveTheme == newThemeKey)
                {
                    if (EnableNotifications)
                    {
                        _ds?.SendToast("No change", $@"Old theme key and new theme key are same. Nothing to change. Key {GetKeyString(newThemeKey)}");
                    }
                    return false;
                }
            }

            if (!GenerateAndProcessChangeData(newThemeKey, frameworkElement, targetAssembly, searchMode)) return false;

            if (raiseChangeEvents)
            {
                //Set the new theme.
                ActiveTheme = newThemeKey; //This will raise event and trigger the other controls to change their own themes.
                ThemeChanged?.Invoke(this, newThemeKey);
            }

            return true;
        }
        private bool GenerateAndProcessChangeData(object newThemeKey, object frameworkElement, Assembly targetAssembly, ThemeSearchMode searchMode)
        {
            //For an external theme to change, the old and new theme keys should be present (preference to targetassembly and then the global).
            //Each assembly might have registered it's own version of theme, if not we try to get the global version.
            //Since we are here at this point, we are sure that the theme is already registered in some dictionary.

            //IF A CROSS GROUP REFERENCE IS PRESENT, AND THE CROSS REF DATA IS VALID AND PRESENT IN THE GLOBAL, THEN WE WILL NOT GET ANY NULL VALUES. ELSE, WE WILL GET NULL NUEWTHEMEINFOS OR OLDTHEMEINFOS. 

            List<ThemeInfo> newThemeInfos = FetchThemes(newThemeKey, targetAssembly);
            if (newThemeInfos == null || newThemeInfos.Count == 0)
            {
                Debug.WriteLine($@"Infos not found for {GetKeyString(newThemeKey)} in any dictionary.");
                return false;
            }

            //Get old theme and the new theme info and change them.
            ThemeChangeData _changeData = new ThemeChangeData() { NewThemes = newThemeInfos, SearchMode = searchMode, Sender = frameworkElement, Themekey = newThemeKey };

            return Process(_changeData);
        }
        private bool Process(ThemeChangeData changeData)
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
                var groupDatas = GetGroupChangeDatas(changeData);
                //After we get the root dictionary, we can either loop through, find the final target and replace it. Or, we can even 
                if (!InitiateChangeTheme(_rootDictionary, groupDatas))
                {
                    Debug.WriteLine("Overall run of FindandReplace has returned negative. There could be one or more theme changes that could have failed. Please check.");
                }
                return true;
            }
            catch (Exception ex)
            {
                HandleException(ex.ToString());
                return false;
            }
        }
        private bool InitiateChangeTheme(ResourceDictionary rootDictionary, List<GroupChangeData> groupDatas)
        {
            if (rootDictionary == null) return false; //Sometimes when the object is getting loaded, the RD might not have been loaded and it might result in null

            if (rootDictionary.MergedDictionaries == null || rootDictionary.MergedDictionaries.Count == 0) return false;
            bool IsSuccess = true;

            //Generate a new copy of the merged dictionaries.
            List<ResourceDictionary> AllMergedDictionaries = new List<ResourceDictionary>();

            foreach (var originalBaseDic in rootDictionary.MergedDictionaries)
            {
                AllMergedDictionaries.Add(new ResourceDictionary() { Source = originalBaseDic.Source });
            }

            if(ReplaceMode == ThemeReplaceMode.FindReplace)
            {
                IsSuccess = FindAndReplace(AllMergedDictionaries, groupDatas);
            }

            //Irrespective of whether we found and replace, we handle copies at root
            HandleRootCopies(AllMergedDictionaries, groupDatas);

            List<ResourceDictionary> oldItems = rootDictionary.MergedDictionaries.ToList();

            foreach (var baseDic in AllMergedDictionaries)
            {
                rootDictionary.MergedDictionaries.Insert(0,baseDic);
            }

            foreach (var oldDic in oldItems)
            {
                rootDictionary.MergedDictionaries.Remove(oldDic);
            }

            return IsSuccess;
        }
        private bool FindAndReplace(List<ResourceDictionary> AllMergedDictionaries,List<GroupChangeData> groupDatas)
        {
            bool IsSuccess = true;
            foreach (var baseDictionary in AllMergedDictionaries)
            {

                //CURRENTLY THE BELOW APPROACH SEARCHES ONLY FOR THE FIRST AVAILABLE ITEM AND THEN CHANGES IT. 
                //TODO: THERE IS A POSSIBLITY THAT SAME THEME MAY BE PRESENT IN DIFFERENT MERGED DICTIONARIES AND IT ALSO NEEDS TO BE CHANGED.
                foreach (var gData in groupDatas)
                {
                    //If the missing themes are autogenerated,then there is a huge possiblility that we might create a theme which is not matching with the startup theme. 
                    //For instance, we haven't setup startuptheme value (which the user has to know) for a particular group . Then we try to fill, there is a possibility that it might even take a non present theme and place it there.

                    ThemeTracker tracker = new ThemeTracker() { IsRoot = true };
                    //Give priority to find and replace.
                    //If the root matches, we ignore at this stage. because we need to come back again and process rootoverwrite.
                    //Get tracker will ignore if basedictionary directly matches the old theme (as we set IsRoot = true above). Because we will replace that later.

                    if (gData.PossibleOldThemes == null || gData.PossibleOldThemes.Count == 0) continue; //Nothing to 

                    if (!GetTracker(baseDictionary, gData.PossibleOldThemes.Select(p=>p.Path).ToList(), ref tracker))
                    {
                        Debug.WriteLine($@"Unable to prepare a ThemeTracker for key {gData.Themekey.AsString()} with new path: {gData.NewTheme.Path} with searchMode {gData.SearchMode} for assembly {gData.Sender}");
                        continue;
                    }

                    //We got a tracker.
                    if (!Replace(ref tracker, gData))
                    {
                        IsSuccess = false;
                        Debug.WriteLine($@"Unable to Replace the theme for key {gData.Themekey.AsString()} with new path: {gData.NewTheme.Path}");
                        continue;
                    }
                }
            }
            return IsSuccess;
        }
        private void HandleRootCopies(List<ResourceDictionary> AllMergedDictionaries, List<GroupChangeData> groupDatas)
        {
            //Now loop through the mergeddictionaries and change direct child.
            foreach (var gData in groupDatas)
            {
                Action addDelegate = () => { AllMergedDictionaries.Insert(0, new ResourceDictionary() { Source = gData.NewTheme.Path }); };
                Action removeDelegate = () => { AllMergedDictionaries.RemoveAll(p => gData.PossibleOldThemes.Any(q => q.Path == p.Source && q.Path != gData.NewTheme.Path)); };

                //Before adding newTheme, get the list of items to remove.
                var _hasOldRds = AllMergedDictionaries.Any(p => gData.PossibleOldThemes.Any(q => q.Path == p.Source));

                //IF OLD DATA FOUND, WE WILL REPLACE IT.
                if (_hasOldRds)
                {
                    //replace with new data and continue further.
                    addDelegate.Invoke();
                    removeDelegate.Invoke();
                    continue; //Donot go again and add below.
                }

                //Check if we need to do root copy.
                if (gData.AddCopyAtRoot || ReplaceMode==ThemeReplaceMode.FindReplaceAndCreateCopy || ReplaceMode == ThemeReplaceMode.IgnoreFindReplaceAndCreateCopy)
                {
                    //Irrespective of the presence of the dictionary at root level, we add it. (after theme swtiches, it will be automatically deducted and replaced above).
                    addDelegate.Invoke();
                }
            }
        }
        private bool Replace(ref ThemeTracker tracker, GroupChangeData groupData)
        {
            if (tracker == null) return false;

            if (!tracker.IsTarget)
            {
                //Go down the tree
                var child = tracker.Child;
                return Replace(ref child, groupData);
            }

            //Before adding newTheme, get the list of items to remove.
            var _hasOldRds = tracker.Parent?.RD.MergedDictionaries.Any(p => groupData.PossibleOldThemes.Any(q => q.Path == p.Source));

            tracker.Parent?.RD.MergedDictionaries.Insert(0, new ResourceDictionary() { Source = groupData.NewTheme.Path }); //Insert at the beginning.
           
            if (_hasOldRds.HasValue || _hasOldRds.Value)
            {
                bool isPresent = true;
                do
                {
                    var _toremove = tracker.Parent?.RD.MergedDictionaries.FirstOrDefault(merged=> 
                    groupData.PossibleOldThemes.Any(oldT=>oldT.Path ==merged.Source) &&
                    merged.Source != groupData.NewTheme.Path);
                    isPresent = (_toremove != null);
                    if (isPresent)
                    {
                        tracker.Parent?.RD.MergedDictionaries.Remove(_toremove);
                    }
                    //Do not remove directly using the tracker as it could contain the cached values (which would have been old). We only use the cache for traversing the tree. 
                } while (isPresent);
               
            }

            return true;
        }
        #endregion

        #region Helpers
        private List<ThemeInfo> GetThemeInfos(List<ThemeInfo> allRegisteredInfos, string groupId, Uri toIgnore)
        {
            try
            {
                //Irrespective of the Key, try to find all themeinfos sharing same group Id.
                if (groupId == null)
                {
                    HandleException("GroupId cannot be null, unable to get all themeinfos.");
                    return null;
                }

                if (allRegisteredInfos == null)
                {
                    HandleException("All registered Infos cannot be null, unable to get all themeinfos.");
                    return null;
                }

                List<ThemeInfo> result = new List<ThemeInfo>();
                result = allRegisteredInfos.Where(p => p.GroupId == groupId && p.Path != toIgnore)?.ToList();

                return result;
            }
            catch (Exception ex)
            {
                HandleException(ex.ToString());
                return null;
            }
        }
        private List<GroupChangeData> GetGroupChangeDatas(ThemeChangeData input)
        {
            List<GroupChangeData> result = new List<GroupChangeData>();
            try
            {
                var allRegisteredThemes = GetAllRegisteredThemeInfos(); 

                foreach (var newTheme in input.NewThemes.Distinct())
                {
                    if (newTheme == null || string.IsNullOrWhiteSpace(newTheme.GroupId)) continue;
                    var otherThemes = GetThemeInfos(allRegisteredThemes, newTheme.GroupId, newTheme.Path);
                    //Each gId should have values in old and new themes (this would already have been taken care by the fill method)
                    GroupChangeData _data = new GroupChangeData()
                    {
                        SearchMode = input.SearchMode,
                        Sender = input.Sender,
                        Themekey = input.Themekey,
                        GroupId = newTheme.GroupId,
                        PossibleOldThemes =otherThemes,
                        NewTheme = newTheme
                    };

                    //A group itself gets registered with the prop
                    _data.AddCopyAtRoot = newTheme.AddCopyAtRoot; //Property of all theme in a group willbe same.
                    result.Add(_data);
                }
                return result;
            }
            catch (Exception ex)
            {
                HandleException(ex.ToString());
                return result;
            }
            
        }
        private void ClearFailedGroups()
        {
            //Remove failed groups 
            foreach (var fGid in _failedGroups)
            {
                //Remove the Global Theme groups.
                foreach (var gDic in _globalThemes)
                {
                    gDic.Value.RemoveAll(p => p.GroupId == fGid);
                }

                //Remove external themes
                foreach (var eDic in _externalThemes)
                {
                    eDic.Value.RemoveAll(p => p.GroupId == fGid);
                }
            }
            _failedGroups.Clear(); //Clear the list.
        }
        private (List<string> AllGids, List<ThemeInfo> AllValues) GetAllGroupInfo(ThemeRegistrationMode mode)
        {
            List<string> groupIds = new List<string>();
            List<ThemeInfo> values = new List<ThemeInfo>();
            switch (mode)
            {
                case ThemeRegistrationMode.Independent:
                    _globalThemes.Values?.ToList().ForEach(p => values.AddRange(p)); //Get all the global values.
                    break;
                case ThemeRegistrationMode.AssemblyBased:
                    _externalThemes.Values?.ToList().ForEach(p => values.AddRange(p));
                    break;
            }
            groupIds = values?.Select(p => p.GroupId).ToList() ?? new List<string>();
            return (groupIds.Distinct().ToList(), values);
        }
        private void FillMissingThemes(ThemeRegistrationMode mode, List<object> allKeys)
        {
            var allGroupInfo = GetAllGroupInfo(mode);

            //All group ids should have one value against each key.
            foreach (var key in allKeys)
            {
                List<string> _currentGids = null;
                //Try to see if the dictionary values has all the keys.
                if(mode == ThemeRegistrationMode.AssemblyBased)
                {
                    if (!_externalThemes.ContainsKey(key))
                    {
                        _externalThemes.TryAdd(key, new List<ThemeInfoEx>());
                    }

                    _externalThemes.TryGetValue(key, out var currentList);
                    _currentGids = currentList?.Select(p => p.GroupId).ToList() ?? new List<string>();
                    var _idstoAdd = allGroupInfo.AllGids.Except(_currentGids).ToList();
                    foreach (var id in _idstoAdd)
                    {
                        //Get default
                        var defaultItem = allGroupInfo.AllValues.FirstOrDefault(p => p.GroupId == id) as ThemeInfoEx;

                        //Try to get global theme
                        _globalThemes.TryGetValue(key, out var globalList);
                        ThemeInfoEx toadd = null;

                        if(defaultItem.CrossReferenceId != null)
                        {
                            var _toadd = globalList.FirstOrDefault(p => p.GroupId == defaultItem.CrossReferenceId);
                            if (_toadd != null)
                            {
                                toadd = new ThemeInfoEx(_toadd.Path, _toadd.GroupId) { SourceAssembly = defaultItem.SourceAssembly,CrossReferenceId = defaultItem.CrossReferenceId };
                            }
                        }
                        
                        if (toadd == null) toadd = defaultItem;
                        
                        currentList.Add(toadd);
                    }
                }
                else
                {
                    if (!_globalThemes.ContainsKey(key))
                    {
                        _globalThemes.TryAdd(key, new List<ThemeInfo>());
                    }
                        _globalThemes.TryGetValue(key, out var currentList);
                    _currentGids = currentList?.Select(p => p.GroupId).ToList() ?? new List<string>();
                    //the currentlist Group Ids should match all keys. Any missing GroupIds should be added.
                    var _idstoAdd = allGroupInfo.AllGids.Except(_currentGids).ToList();

                    foreach (var id in _idstoAdd)
                    {
                        //For this groupid, get the first matching value from the values and add it to the ditionary.
                        var _toadd = allGroupInfo.AllValues.FirstOrDefault(p => p.GroupId == id);
                        currentList.Add(_toadd);
                    }
                }
            }
        }
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
        private bool GetTracker(ResourceDictionary parentDic, List<Uri> oldThemes, ref ThemeTracker tracker)
        {
            //TRACKER SHOULD NOT CONSIDER THE CURRENT 
            //Our goal is to find which resource dictionary ends with old theme uri.
            tracker.RD = parentDic; //Root dic goes to the root tracker's RD
            ThemeTracker childTracker = new ThemeTracker();
            childTracker.Parent = tracker; //this is the child's parent.

            //Check if child direclty matches?
            if (!tracker.IsRoot && parentDic.Source != null && oldThemes.Any(p=> p == parentDic.Source))
            {
                //Found matching uri.
                tracker.IsTarget = true;
                return true;
            }

            //Check if any of the merged dictionaries of the child matches.
            var matchFound = parentDic.MergedDictionaries.FirstOrDefault(p => p.Source != null && oldThemes.Any(s => s == parentDic.Source));

            //If overwrite at root level, then it should be merged to the rootDic's merged dictionaries.
            //This will ensure that all the cascading themes will follow it up.
            //This is kind of like a override mechanism.
            if (matchFound != null)
            {
                //if matchfound is not null, then we actually have this at root level.
                //If match is not found but we overwrite it, we, proceed.
                childTracker.RD = matchFound; //This could be null.
                childTracker.IsTarget = true;
                tracker.Child = childTracker;
                return true;
            }

            //Loop through each of the merged dictionaries.
            foreach (var rDic in parentDic.MergedDictionaries)
            {
                if (GetTracker(rDic, oldThemes, ref childTracker))
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

using Haley.Abstractions;
using Haley.Enums;
using Haley.Models;
using Haley.MVVM;
using Haley.WPF.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Haley.Services {

    public class DialogService : IDialogServiceEx {

        #region Attributes

        private IThemeService _ts;
        private SolidColorBrush _accentColor;
        private SolidColorBrush _accentForeground;
        private Brush _toastBackground;
        private SolidColorBrush _toastForeground;
        private SolidColorBrush _toastBorder = new SolidColorBrush(Colors.White);
        private Brush _background;
        private Color? _glowColor = null;
        private double _glowRadius = 3.0;
        private bool _topMost = true;
        private bool _showInTaskBar = false;
        private bool _windowBackgroundBlur = false;
        private Brush _foreground;
        private Brush _contentBackground;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
        private ConcurrentDictionary<DialogServiceProp, object> _themeDic = new ConcurrentDictionary<DialogServiceProp, object>();

        #endregion Attributes

        #region Properties

        public SolidColorBrush AccentColor {
            get { return _accentColor; }
            set { _accentColor = value; }
        }

        public SolidColorBrush AccentForeground {
            get { return _accentForeground; }
            set { _accentForeground = value; }
        }

        public Brush ToastBackground {
            get { return _toastBackground; }
            set { _toastBackground = value; }
        }

        public Brush Foreground {
            get { return _foreground; }
            set { _foreground = value; }
        }

        public Brush Background {
            get { return _background; }
            set { _background = value; }
        }

        public bool EnableBackgroundBlur {
            get { return _windowBackgroundBlur; }
            set { _windowBackgroundBlur = value; }
        }

        public SolidColorBrush ToastForeground {
            get { return _toastForeground; }
            set { _toastForeground = value; }
        }

        public Color? GlowColor {
            get { return _glowColor; }
            set { _glowColor = value; }
        }

        public double GlowRadius {
            get { return _glowRadius; }
            set { _glowRadius = value; }
        }

        public Brush ContentBackground {
            get { return _contentBackground; }
            set { _contentBackground = value; }
        }

        public bool TopMost {
            get { return _topMost; }
            set { _topMost = value; }
        }

        public bool ShowInTaskBar {
            get { return _showInTaskBar; }
            set { _showInTaskBar = value; }
        }

        public WindowStartupLocation StartupLocation {
            get { return _startupLocation; }
            set { _startupLocation = value; }
        }

        #endregion Properties

        #region ThemeManagement

        public bool SubscribeThemeService(IThemeService service) {
            if (_ts != null) {
                UnsubscribeThemeService();
            }
            _ts = service; //Replace with new service.
            _ts.ThemeChanged += HandleThemeChange;

            return true;
        }

        public bool UnsubscribeThemeService() {
            if (_ts == null) return false;
            _ts.ThemeChanged -= HandleThemeChange;
            return true;
        }

        public void ClearCurrentTheme() {
            AccentForeground = AccentColor = ToastForeground = null;
            Foreground = ContentBackground = ToastBackground = Background = null;
        }

        public void RebaseTheme(bool defaultTheme = false) {
            if (Application.Current == null) return;
            var rootDic = Application.Current.Resources;
            if (rootDic == null) return;
            if (defaultTheme) {
                ResetDefaultTheme();
            }
            //Now, basedon the existing themeservice, fetch and rebase theme values.
            foreach (var kvp in _themeDic) {
                try {
                    var value = rootDic[kvp.Value];
                    this.GetType().GetProperty(kvp.Key.ToString()).SetValue(this, value);
                } catch (Exception) {
                    continue;
                }
            }
        }

        void ResetDefaultTheme() {
            var themeDic = new Dictionary<DialogServiceProp, object>();
            themeDic.Add(DialogServiceProp.AccentColor, "def_accent_primary");
            themeDic.Add(DialogServiceProp.AccentForeground, "def_content_inverted");
            themeDic.Add(DialogServiceProp.Background, "def_background_primary");
            themeDic.Add(DialogServiceProp.Foreground, "def_content");
            _themeDic?.Clear();
            _themeDic = new ConcurrentDictionary<DialogServiceProp, object>(themeDic);
        }

        private void HandleThemeChange(object sender, object e) {
            //Now basedon the theme change, fetch few values and store them.
            RebaseTheme(false);
        }

        public void RegisterThemeKey(DialogServiceProp propname, object resourceKey) {
            if (resourceKey == null) return;
            if (!_themeDic.ContainsKey(propname)) {
                _themeDic.TryAdd(propname, null);
            }
            _themeDic.TryGetValue(propname, out var current);
            _themeDic.TryUpdate(propname, resourceKey, current);
        }

        public void UnregisterThemeKey(DialogServiceProp propname) {
            _themeDic.TryRemove(propname, out _);
        }

        public void RegisterThemeKeys(Dictionary<DialogServiceProp, object> keydictionary) {
            foreach (var kvp in keydictionary) {
                try {
                    if (kvp.Value == null) continue;
                    RegisterThemeKey(kvp.Key, kvp.Value);
                } catch (Exception) {
                    continue;
                }
            }
        }

        public void UnregisterThemeKeys(List<DialogServiceProp> propList) {
            foreach (var propname in propList) {
                try {
                    UnregisterThemeKey(propname);
                } catch (Exception) {
                    continue;
                }
            }
        }

        public void UnregisterAllTheme() {
            _themeDic.Clear(); //unregister all
        }

        #endregion ThemeManagement

        #region Public Methods

        public bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7) {
            DisplayType _type = DisplayType.ToastInfo;
            var _wndw = _getNotificationWindow(title, message, icon, _type, hideIcon, false, true);

            if (!_windowBackgroundBlur) {
                //If windowbackgroundblur is true, then it would already been set in getnotificationwindow method.
                _wndw.SetCurrentValue(WindowBlurAP.IsEnabledProperty, true); //For toast we use blur.
            }
            _wndw.AutoClose = autoClose;
            _wndw.BorderBrush = _toastBorder;
            _wndw.BorderThickness = new Thickness(0.4);
            return Notification.SendToast(_wndw, display_seconds);
        }

        public INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false, bool blurOtherWindows = false) {
            //First get the type of notification.
            DisplayType _type = DisplayType.ShowInfo;
            switch (mode) {
                case DialogMode.Notification:
                    _type = DisplayType.ShowInfo;
                    break;

                case DialogMode.Confirmation:
                    _type = DisplayType.GetConfirmation;
                    break;

                case DialogMode.GetInput:
                    _type = DisplayType.GetInput;
                    break;
            }

            var _wndw = _getNotificationWindow(title, message, icon, _type, hideIcon);

            return Notification.ShowDialog(_wndw, blurOtherWindows);
        }

        public INotification ShowCustomView(string title, object templateOrControl = null, bool blurOtherWindows = false) {
            //First get the type of notification.
            if (templateOrControl == null) return null;

            var _wndw = _getNotificationWindowForTemplate(title, templateOrControl);
            return Notification.ShowDialog(_wndw, blurOtherWindows);
        }

        public INotification Info(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false) {
            return ShowDialog(title, message, NotificationIcon.Info, mode, blurOtherWindows: blurOtherWindows);
        }

        public INotification Warning(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false) {
            return ShowDialog(title, message, NotificationIcon.Warning, mode, blurOtherWindows: blurOtherWindows);
        }

        public INotification Error(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false) {
            return ShowDialog(title, message, NotificationIcon.Error, mode, blurOtherWindows: blurOtherWindows);
        }

        public INotification Success(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false) {
            return ShowDialog(title, message, NotificationIcon.Success, mode, blurOtherWindows: blurOtherWindows);
        }

        #endregion Public Methods

        #region Container Methods

        public INotification ShowContainerView(string title, object key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null) {
            UserControl _view = null;
            try {
                if (container != null) {
                    //No fall back. If container doesn't have the control with key, DO NOT FALL BACK TO DEFAULT CONTAINER.
                    //Containerstore resolve the controls to get the control
                    _view = container.GenerateViewFromKey(key, InputViewModel, mode) as UserControl;
                } else {
                    //Containerstore resolve the controls to get the control
                    _view = ContainerStore.Controls.GenerateViewFromKey(key, InputViewModel, mode) as UserControl;
                }
            } catch (Exception ex) {
                string _msg = $@"No UserControl is associated with the key - {key.ToString()}" + Environment.NewLine + ex.ToString();
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false);
                return Notification.ShowDialog(_infoWndw, blurOtherWindows);
            }

            if (_view == null) {
                string _msg = $@"No UserControl is associated with the key - {key.ToString()}";
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false);
                return Notification.ShowDialog(_infoWndw, blurOtherWindows);
            }

            var _wndw = _getNotificationWindowForContainer(title, _view);
            return Notification.ShowContainerView(_wndw, blurOtherWindows); //notification will fetch the viewmodel and add it to INotification result.
        }

        public INotification ShowContainerView<ViewOrVMType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null) where ViewOrVMType : class {
            //either this should be from ihaleyvm (for viewmodels) or it should be an usercontrol
            if (typeof(IHaleyVM).IsAssignableFrom(typeof(ViewOrVMType)) || typeof(UserControl).IsAssignableFrom(typeof(ViewOrVMType))) {
                //this is a viewmodel input
                string _key = typeof(ViewOrVMType).ToString();
                return ShowContainerView(title, _key, InputViewModel, mode, blurOtherWindows, container);
            } else {
                throw new ArgumentException("Container view excepts a type of usercontrol or a type that implements IHaleyVM");
            }
        }

        #endregion Container Methods

        #region Private Methods

        private Notification _getNotificationBaseWindow(string title, DisplayType type, bool? showInTaskBar = null, bool? topMost = null) {
            Notification _newWindow = new Notification();

            //only if the colors are not null, set them. Else, let it use the default colors.
            if (AccentColor != null) {
                _newWindow.AccentColor = AccentColor;
            }

            if (AccentForeground != null) {
                _newWindow.AccentForeground = AccentForeground;
            }

            if (ToastBackground != null) {
                _newWindow.ToastBackground = ToastBackground;
            }

            if (ToastForeground != null) {
                _newWindow.ToastForeground = ToastForeground;
            }

            if (Background != null) {
                _newWindow.Background = Background; //Else it would be default white.
            }

            if (Foreground != null) {
                _newWindow.Foreground = Foreground;
            }

            if (ContentBackground != null) {
                _newWindow.ContentBackground = ContentBackground;
            }

            //If we try to set the background blur as dependency property, then even for disabled status, the window background blur will be set. So only call when necessary.
            if (_windowBackgroundBlur) {
                //Set attached property.
                _newWindow.SetCurrentValue(WindowBlurAP.IsEnabledProperty, true);//We will not use the same window (as it will get disposed). Also, we will not change during run time.
            } else {
                if (type != DisplayType.ToastInfo) {
                    _newWindow.Margin = new Thickness(20.0);
                    _newWindow.BorderThickness = new Thickness(0.5);
                    _newWindow.BorderBrush = Brushes.Gray;
                    if (_glowColor.HasValue) {
                        _newWindow.BorderBrush = new SolidColorBrush(_glowColor.Value);
                    }
                }
            }

            _newWindow.GlowColor = _glowColor ?? Colors.Gray;
            _newWindow.GlowRadius = _glowRadius;

            _newWindow.Title = title;
            _newWindow.Type = type;
            _newWindow.ShowInTaskbar = showInTaskBar == null ? _showInTaskBar : showInTaskBar.Value;
            _newWindow.Topmost = topMost == null ? _topMost : topMost.Value;
            _newWindow.WindowStartupLocation = _startupLocation;
            return _newWindow;
        }

        private Notification _getNotificationWindowForContainer(string title, UserControl container_view, bool? showInTaskBar = null, bool? topMost = null) {
            var _newWindow = _getNotificationBaseWindow(title, DisplayType.ContainerView, showInTaskBar, topMost);
            _newWindow.ContainerView = container_view;
            return _newWindow;
        }

        private Notification _getNotificationWindow(string title, string message, NotificationIcon icon, DisplayType type, bool hideIcon, bool? showInTaskBar = null, bool? topMost = null) {
            var _newWindow = _getNotificationBaseWindow(title, type, showInTaskBar, topMost);
            //Set base properties
            _newWindow.Message = message;
            _newWindow.NotificationIcon = icon;
            _newWindow.ShowNotificationIcon = !hideIcon;
            return _newWindow;
        }

        private Notification _getNotificationWindowForTemplate(string title, object template, bool? showInTaskBar = null, bool? topMost = null) {
            var _newWindow = _getNotificationBaseWindow(title, DisplayType.CustomView, showInTaskBar, topMost);

            if (typeof(UserControl).IsAssignableFrom(template.GetType())) {
                //we are dealing with usercontrol. prepare a datatemplate with this usercontrol
                _newWindow.CustomView = template as UserControl;
                _newWindow.UseCustomView = true;
            } else {
                _newWindow.CustomViewTemplate = template as DataTemplate;
                _newWindow.UseCustomView = false;
            }
            return _newWindow;
        }

        #endregion Private Methods
    }
}
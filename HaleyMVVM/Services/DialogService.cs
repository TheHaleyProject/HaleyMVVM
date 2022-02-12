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
using System.Windows.Controls;
using Haley.MVVM;

namespace Haley.Services
{
    public class DialogService : IDialogServiceEx
    {
        #region Attributes
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
        #endregion

        #region Properties
        public SolidColorBrush AccentColor
        {
            get { return _accentColor; }
            set { _accentColor = value; }
        }
        public SolidColorBrush AccentForeground
        {
            get { return _accentForeground; }
            set { _accentForeground = value; }
        }
        public Brush ToastBackground
        {
            get { return _toastBackground; }
            set { _toastBackground = value; }
        }
        public Brush Foreground
        {
            get { return _foreground; }
            set { _foreground = value; }
        }
        public Brush Background
        {
            get { return _background; }
            set { _background = value; }
        }
        public bool EnableBackgroundBlur
        {
            get { return _windowBackgroundBlur; }
            set { _windowBackgroundBlur = value; }
        }
        public SolidColorBrush ToastForeground
        {
            get { return _toastForeground; }
            set { _toastForeground = value; }
        }
        public Color? GlowColor
        {
            get { return _glowColor; }
            set { _glowColor = value; }
        }
        public double GlowRadius
        {
            get { return _glowRadius; }
            set { _glowRadius = value; }
        }
        public Brush ContentBackground
        {
            get { return _contentBackground; }
            set { _contentBackground = value; }
        }
        public bool TopMost
        {
            get { return _topMost; }
            set { _topMost = value; }
        }

        public bool ShowInTaskBar
        {
            get { return _showInTaskBar; }
            set { _showInTaskBar = value; }
        }

        public WindowStartupLocation StartupLocation
        {
            get { return _startupLocation; }
            set { _startupLocation = value; }
        }

        #endregion

        #region Public Methods
        public bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7)
        {
            DisplayType _type = DisplayType.ToastInfo;
            var _wndw = _getNotificationWindow(title, message, icon, _type, hideIcon,false,true);

            if (!_windowBackgroundBlur)
            {
                //If windowbackgroundblur is true, then it would already been set in getnotificationwindow method.
                _wndw.SetCurrentValue(WindowBlurAP.IsEnabledProperty, true); //For toast we use blur.
            }
            _wndw.AutoClose = autoClose;
            _wndw.BorderBrush = _toastBorder;
            _wndw.BorderThickness = new Thickness(0.4);
            return Notification.SendToast(_wndw, display_seconds);
        }
        public INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false, bool blurOtherWindows = false)
        {
            //First get the type of notification.
            DisplayType _type = DisplayType.ShowInfo;
            switch (mode)
            {
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

            return Notification.ShowDialog(_wndw,blurOtherWindows);
        }
        public INotification ShowCustomView(string title, DataTemplate template = null, bool blurOtherWindows = false)
        {
            //First get the type of notification.
            if (template == null) return null;
            var _wndw = _getNotificationWindow(title, template);
            return Notification.ShowDialog(_wndw, blurOtherWindows);
        }
        public INotification Info(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Info,mode, blurOtherWindows: blurOtherWindows);
        }
        public INotification Warning(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Warning,mode, blurOtherWindows: blurOtherWindows);
        }
        public INotification Error(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Error,mode, blurOtherWindows: blurOtherWindows);
        }
        public INotification Success(string title, string message, DialogMode mode = DialogMode.Notification, bool blurOtherWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Success,mode, blurOtherWindows: blurOtherWindows);
        }
        #endregion

        #region Container Methods
        public INotification ShowContainerView(string title, string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null)
        {
            UserControl _view = null;
            try
            {
               if (container != null)
                {
                    //No fall back. If container doesn't have the control with key, DO NOT FALL BACK TO DEFAULT CONTAINER.
                    //Containerstore resolve the controls to get the control
                    _view = container.GenerateViewFromKey(key, InputViewModel, mode) as UserControl;
                }
                else
                {
                    //Containerstore resolve the controls to get the control
                    _view = ContainerStore.Singleton.Controls.GenerateViewFromKey(key, InputViewModel, mode) as UserControl;
                }
               
            }
            catch (Exception ex)
            {
                string _msg = $@"No UserControl is associated with the key - {key}" + Environment.NewLine + ex.ToString();
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false);
                return Notification.ShowDialog(_infoWndw,blurOtherWindows);
            }

            if (_view == null)
            {
                string _msg = $@"No UserControl is associated with the key - {key}";
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false);
                return Notification.ShowDialog(_infoWndw,blurOtherWindows);
            }

            var _wndw = _getNotificationWindow(title, _view);
            return Notification.ShowContainerView(_wndw,blurOtherWindows); //notification will fetch the viewmodel and add it to INotification result.
        }
        public INotification ShowContainerView(string title, Enum @enum, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null)
        {
            string _key = @enum.GetKey();
            return ShowContainerView(title, _key, InputViewModel, mode, blurOtherWindows, container);
        }
        public INotification ShowContainerView<ViewType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null) where ViewType : class
        {
            if (!(typeof(ViewType).BaseType == typeof(UserControl) || typeof(ViewType) == typeof(UserControl)))
            {
                throw new ArgumentException("Container view excepts a type of usercontrol");

            }
            string _key = typeof(ViewType).ToString();
            return ShowContainerView(title, _key, InputViewModel, mode, blurOtherWindows, container);
        }
        public INotification ShowContainerView<VMType>(string title, VMType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurOtherWindows = false, IControlContainer container = null) where VMType : class, IHaleyVM
        {
            string _key = typeof(VMType).ToString();
            return ShowContainerView(title, _key, InputViewModel, mode, blurOtherWindows,container);
        }

        #endregion

        #region Private Methods
        private Notification _getNotificationBaseWindow(string title, DisplayType type,bool? showInTaskBar = null, bool? topMost = null)
        {
            Notification _newWindow = new Notification();

            //only if the colors are not null, set them. Else, let it use the default colors.
            if (_accentColor != null)
            {
                _newWindow.AccentColor = _accentColor;
            }

            if (_accentForeground != null)
            {
                _newWindow.AccentForeground = _accentForeground;
            }

            if (_toastBackground != null)
            {
                _newWindow.ToastBackground = _toastBackground;
            }

            if (_toastForeground != null)
            {
                _newWindow.ToastForeground = _toastForeground;
            }

            if (_background != null)
            {
                _newWindow.Background = _background; //Else it would be default white.
            }

            if (_foreground != null)
            {
                _newWindow.Foreground = _foreground;
            }

            if (_contentBackground != null)
            {
                _newWindow.ContentBackground = _contentBackground;
            }

            //If we try to set the background blur as dependency property, then even for disabled status, the window background blur will be set. So only call when necessary.
            if (_windowBackgroundBlur)
            {
                //Set attached property.
                _newWindow.SetCurrentValue(WindowBlurAP.IsEnabledProperty, true);//We will not use the same window (as it will get disposed). Also, we will not change during run time.
            }
            else
            {
                if (type != DisplayType.ToastInfo)
                {
                    _newWindow.Margin = new Thickness(20.0);
                    _newWindow.BorderThickness = new Thickness(0.5);
                    _newWindow.BorderBrush = Brushes.Gray;
                    if (_glowColor.HasValue)
                    {
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
        private Notification _getNotificationWindow(string title, UserControl container_view, bool? showInTaskBar = null, bool? topMost = null)
        {
            var _newWindow = _getNotificationBaseWindow(title, DisplayType.ContainerView,showInTaskBar,topMost );
            _newWindow.ContainerView = container_view;
            return _newWindow;
        }
        private Notification _getNotificationWindow(string title, string message, NotificationIcon icon, DisplayType type, bool hideIcon, bool? showInTaskBar = null, bool? topMost = null)
        {
            var _newWindow = _getNotificationBaseWindow(title, type,showInTaskBar,topMost );
            //Set base properties
            _newWindow.Message = message;
            _newWindow.NotificationIcon = icon;
            _newWindow.ShowNotificationIcon = !hideIcon;
            return _newWindow;
        }
        private Notification _getNotificationWindow(string title, DataTemplate template, bool? showInTaskBar = null, bool? topMost = null)
        {
            var _newWindow = _getNotificationBaseWindow(title, DisplayType.CustomView, showInTaskBar, topMost);
            _newWindow.CustomViewTemplate = template;
            return _newWindow;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.Controls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;
using Haley.MVVM;

namespace Haley.Services
{
    public class DialogService : IDialogService
    {
        #region Attributes
        private SolidColorBrush _accentColor;
        private SolidColorBrush _accentForeground;
        private SolidColorBrush _toastForeground;
        private SolidColorBrush _toastBorder = new SolidColorBrush(Colors.White);
        private Color _glowColor = Colors.Gray;
        private double _glowRadius = 3.0;
        private Brush _toastBackground;
        private bool _topMost = true;
        private bool _showInTaskBar = false;
        private WindowStartupLocation _startupLocation = WindowStartupLocation.CenterOwner;
        #endregion

        #region Public Methods

        /// <summary>
        /// Set glow and its radius.
        /// </summary>
        /// <param name="glowColor">null, if no change is required.</param>
        /// <param name="glowRadius"></param>
        public void SetGlow(Color? glowColor, double glowRadius = 3.0)
        {
            if (glowColor.HasValue)
            {
                _glowColor = glowColor.Value;
            }
            _glowRadius = glowRadius;

        }
        public void ChangeAccentColors(SolidColorBrush AccentColor = null, SolidColorBrush AccentForeground = null, Brush ToastBackground = null, SolidColorBrush ToastForeground = null)
        {
            _accentColor = AccentColor;
            _accentForeground = AccentForeground;
            _toastForeground = ToastForeground;
            _toastBackground = ToastBackground;
        }

        public void ChangeSettings(bool? topMost = null, bool? showInTaskBar = null, DialogStartupLocation startupLocation = DialogStartupLocation.CenterParent)
        {
            if (topMost != null) _topMost = topMost.Value;
            if (showInTaskBar != null) _showInTaskBar = showInTaskBar.Value;

            switch (startupLocation)
            {
                case DialogStartupLocation.CenterParent:
                    _startupLocation = WindowStartupLocation.CenterOwner;
                    break;
                case DialogStartupLocation.CenterScreen:
                    _startupLocation = WindowStartupLocation.CenterScreen;
                    break;
            }
        }

        public bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7)
        {
            DisplayType _type = DisplayType.ToastInfo;
            var _wndw = _getNotificationWindow(title, message, icon, _type, hideIcon,false,true);
            _wndw.AutoClose = autoClose;
            _wndw.BorderBrush = _toastBorder;
            _wndw.BorderThickness = new Thickness(0.4);
            return Notification.SendToast(_wndw, display_seconds);
        }
       
        public INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false, bool blurWindows = false)
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

            return Notification.ShowDialog(_wndw,blurWindows);
        }

        public INotification ShowCustomView(string title, DataTemplate template = null, bool blurWindows = false)
        {
            //First get the type of notification.
         
            if (template == null) return null;
            var _wndw = _getNotificationWindow(title, template);
            return Notification.ShowDialog(_wndw, blurWindows);
        }

        public INotification Info(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Info,mode, blurWindows: blurWindows);
        }

        public INotification Warning(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Warning,mode, blurWindows: blurWindows);
        }

        public INotification Error(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Error,mode, blurWindows: blurWindows);
        }

        public INotification Success(string title, string message, DialogMode mode = DialogMode.Notification, bool blurWindows = false)
        {
            return ShowDialog(title, message, NotificationIcon.Success,mode, blurWindows: blurWindows);
        }
        #endregion

        #region Container Methods
        public INotification ShowContainerView(string title, string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false)
        {
            UserControl _view = null;
            try
            {
                //Containerstore resolve the controls to get the control
                _view = (UserControl)ContainerStore.Singleton.Controls.GenerateView(key, InputViewModel, mode);
            }
            catch (Exception ex)
            {
                string _msg = $@"No UserControl is associated with the key - {key}" + Environment.NewLine + ex.ToString();
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false);
                return Notification.ShowDialog(_infoWndw,blurWindows);
            }

            if (_view == null)
            {
                string _msg = $@"No UserControl is associated with the key - {key}";
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false);
                return Notification.ShowDialog(_infoWndw,blurWindows);
            }

            var _wndw = _getNotificationWindow(title, _view);
            return Notification.ShowContainerView(_wndw,blurWindows); //notification will fetch the viewmodel and add it to INotification result.
        }
        public INotification ShowContainerView(string title, Enum @enum, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false)
        {
            string _key = @enum.GetKey();
            return ShowContainerView(title, _key, InputViewModel, mode, blurWindows);
        }
        public INotification ShowContainerView<ViewType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false) where ViewType : UserControl
        {
            string _key = typeof(ViewType).ToString();
            return ShowContainerView(title, _key, InputViewModel, mode, blurWindows);
        }
        public INotification ShowContainerView<VMType>(string title, VMType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool blurWindows = false) where VMType : class, IHaleyVM
        {
            string _key = typeof(VMType).ToString();
            return ShowContainerView(title, _key, InputViewModel, mode, blurWindows);
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

            _newWindow.GlowColor = _glowColor;
            _newWindow.GlowRadius = _glowRadius;

            _newWindow.Title = title;
            _newWindow.Type = type;
            _newWindow.ShowInTaskbar = showInTaskBar == null ? _showInTaskBar : showInTaskBar.Value;
            _newWindow.Topmost = topMost == null? _topMost : topMost.Value;
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

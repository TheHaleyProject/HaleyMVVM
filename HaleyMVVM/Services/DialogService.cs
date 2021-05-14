using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.BaseControls;
using Haley.Utils;
using System.Windows;
using System.Windows.Controls;

namespace Haley.MVVM.Services
{
    public class DialogService : IDialogService
    {
        #region Attributes
        private SolidColorBrush _accentColor;
        private SolidColorBrush _accentForeground;
        private SolidColorBrush _toastForeground;
        private Brush _toastBackground;
        #endregion

        [Obsolete("Soon to be deprecated. Use ShowDialog or SendToast")]
        public bool send(string title,string message, DialogMode mode = DialogMode.Notification)
        {
            var _info = ShowDialog(title, message, mode: mode,hideIcon:true);
            if (!_info.DialogResult.HasValue) return false;
            return _info.DialogResult.Value;
        }

        [Obsolete("Soon to be deprecated. Use ShowDialog or SendToast")]
        public bool receive(string title, string message, out string user_input)
        {
            user_input = null;
            var _info = ShowDialog(title, message, mode: DialogMode.GetInput, hideIcon: true);
            if (!_info.DialogResult.HasValue) return false;
            if (_info.DialogResult.Value)
            {
                user_input = _info.UserInput;
            }
            return false;
        }
     
        public void ChangeAccentColors(SolidColorBrush AccentColor = null, SolidColorBrush AccentForeground = null, Brush ToastBackground = null, SolidColorBrush ToastForeground = null)
        {
            _accentColor = AccentColor;
            _accentForeground = AccentForeground;
            _toastForeground = ToastForeground;
            _toastBackground = ToastBackground;
        }

        private Notification _getNotificationBaseWindow(string title, DisplayType type, bool topMost, bool showInTaskBar)
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
            _newWindow.Title = title;
            _newWindow.Type = type;
            _newWindow.ShowInTaskbar = showInTaskBar;
            _newWindow.Topmost = topMost;
            return _newWindow;
        }

        private Notification _getNotificationWindow(string title, UserControl container_view, DisplayType type,  bool topMost, bool showInTaskBar)
        {
            var _newWindow = _getNotificationBaseWindow(title,type,topMost,showInTaskBar);
            _newWindow.ContainerView = container_view;
            return _newWindow;
        }

        private Notification _getNotificationWindow(string title, string message, NotificationIcon icon , DisplayType type, bool hideIcon, bool topMost, bool showInTaskBar)
        {
            var _newWindow = _getNotificationBaseWindow(title, type, topMost, showInTaskBar);
            //Set base properties
            _newWindow.Message = message;
            _newWindow.NotificationIcon = icon;
            _newWindow.ShowNotificationIcon = !hideIcon;
            return _newWindow;
        }

        public INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false, bool topMost = true, bool showInTaskBar = false)
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

            var _wndw = _getNotificationWindow(title,message,icon,_type,hideIcon,topMost,showInTaskBar);
            return Notification.ShowDialog(_wndw);
        }

        public bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7)
        {
            DisplayType _type = DisplayType.ToastInfo;
            var _wndw = _getNotificationWindow(title, message, icon, _type, hideIcon,true,false);
            _wndw.AutoClose = autoClose;
            return Notification.SendToast(_wndw,display_seconds);
        }

        public INotification ShowContainerView(string title, string key, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false)
        {
            UserControl _view = null;
            try
            {
                //Containerstore resolve the controls to get the control
                _view = (UserControl)ContainerStore.Singleton.controls.generateView(key, InputViewModel, mode);
            }
            catch (Exception ex)
            {
                string _msg = $@"No UserControl is associated with the key - {key}" + Environment.NewLine + ex.ToString();
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false, topMost, showInTaskBar);
                return Notification.ShowDialog(_infoWndw);
            }
           
            if (_view == null)
            {
                string _msg = $@"No UserControl is associated with the key - {key}";
                var _infoWndw = _getNotificationWindow(title, _msg, NotificationIcon.Error, DisplayType.ShowInfo, false, topMost, showInTaskBar);
               return Notification.ShowDialog(_infoWndw);
            }

            var _wndw = _getNotificationWindow(title, _view, DisplayType.ContainerView, topMost, showInTaskBar);
            return Notification.ShowContainerView(_wndw); //notification will fetch the viewmodel and add it to INotification result.
        }
        public INotification ShowContainerView(string title, Enum @enum, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false)
        {
            string _key = @enum.getKey();
            return ShowContainerView(title,_key, InputViewModel, mode,topMost,showInTaskBar);
        }
        public INotification ShowContainerView<ViewType>(string title, object InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false) where ViewType : class, IHaleyControl
        {
            string _key = typeof(ViewType).ToString();
            return ShowContainerView(title,_key, InputViewModel, mode, topMost, showInTaskBar);
        }
        public INotification ShowContainerView<VMType>(string title, VMType InputViewModel = null, ResolveMode mode = ResolveMode.AsRegistered, bool topMost = true, bool showInTaskBar = false) where VMType : class, IHaleyControlVM
        {
            string _key = typeof(VMType).ToString();
            return ShowContainerView(title,_key, InputViewModel, mode, topMost, showInTaskBar);
        }
    }
}

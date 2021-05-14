using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using Haley.Abstractions;
using Haley.Enums;
using Haley.WPF.BaseControls;

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

        private Notification _getNotificationWindow(string title, string message, NotificationIcon icon , NotificationType type, bool hideIcon, bool topMost, bool showInTaskBar)
        {
            Notification _newWindow = new Notification();

            //only if the colors are not null, set them. Else, let it use the default colors.
            if (_accentColor!= null)
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

            //Set base properties
            _newWindow.Title = title;
            _newWindow.Message = message;
            _newWindow.NotificationIcon = icon;
            _newWindow.ShowNotificationIcon = !hideIcon;
            _newWindow.Type = type;
            _newWindow.ShowInTaskbar = showInTaskBar;
            _newWindow.Topmost = topMost;

            return _newWindow;
        }

        public INotification ShowDialog(string title, string message, NotificationIcon icon = NotificationIcon.Info, DialogMode mode = DialogMode.Notification, bool hideIcon = false, bool topMost = true, bool showInTaskBar = false)
        {
            //First get the type of notification.
            NotificationType _type = NotificationType.ShowInfo;
            switch (mode)
            {
                case DialogMode.Notification:
                    _type = NotificationType.ShowInfo;
                    break;
                case DialogMode.Confirmation:
                    _type = NotificationType.GetConfirmation;
                    break;
                case DialogMode.GetInput:
                    _type = NotificationType.GetInput;
                    break;
            }

            var _wndw = _getNotificationWindow(title,message,icon,_type,hideIcon,topMost,showInTaskBar);
            return Notification.ShowDialog(_wndw);
        }

        public bool SendToast(string title, string message, NotificationIcon icon = NotificationIcon.Info, bool hideIcon = false, bool autoClose = true, int display_seconds = 7)
        {
            NotificationType _type = NotificationType.ToastInfo;
            var _wndw = _getNotificationWindow(title, message, icon, _type, hideIcon,true,false);
            _wndw.AutoClose = autoClose;
            return Notification.SendToast(_wndw,display_seconds);
        }
    }
}

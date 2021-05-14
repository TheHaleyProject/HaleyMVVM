using System;
using System.Linq;
using System.Windows;
using Haley.Models;
using System.Windows.Controls;
using Haley.Enums;
using System.Windows.Media;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Haley.Abstractions;
using System.Collections.ObjectModel;
using Haley.Utils;
using Haley.MVVM;
using System.Collections.Concurrent;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Haley.WPF.BaseControls
{
    //Notification is not a control. It is a window that will be displayed to user as a separate dialog or as a toast in the desktop.
    public sealed class Notification : Window, INotification
    {
        #region Attributes
        private static SolidColorBrush _baseAccent = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF324862");
        private static SolidColorBrush _baseAccentForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("Yellow");
        private static SolidColorBrush _baseToastAccent = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF2F3542");
        private static SolidColorBrush _baseToastForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("White");

        private const string UIEheader = "PART_header";

        private int _displayDuration = 5;
        private int _timerCount;
        private DispatcherTimer _autoCloseTimer;
        #endregion

        #region UIElements
        private FrameworkElement _header;
        #endregion

        #region Constructors
        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Notification), new FrameworkPropertyMetadata(typeof(Notification)));
        }

        public Notification()
        {
            //Assign an ID
            Id = Guid.NewGuid().ToString();

            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, _closeAction));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, _closeAllToasts));

        }
        #endregion

        #region Overridden Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _header = GetTemplateChild(UIEheader) as FrameworkElement;

            _eventSubscription();
        }
        #endregion

        #region Public Methods
        public static INotification ShowDialog(Notification input)
        {
            if (input.Type == NotificationType.ToastInfo)
            {
                // We should not have toast info type. If by some mistake, it was set, it has to be changed to notification.
                input.Type = NotificationType.ShowInfo;
            }
            var result = input.ShowDialog();
            return (INotification)input;
        }

        public static bool SendToast(Notification input, int display_seconds = 7)
        {
            var _appTitle = AppDomain.CurrentDomain?.FriendlyName;
            if (input.AppName == null) input.AppName = _appTitle;
            input.Type = NotificationType.ToastInfo; //Just to re-assure that the type is of toast 
            input.Opacity = 0; //First start with 0 opacity so it is not shown in screen
            input.Show();

            //var desktopArea = SystemParameters.WorkArea;
            //var leftStart = desktopArea.Width - input.ActualWidth;
            //var topStart = desktopArea.Height - input.ActualHeight;
            var leftStart = SystemParameters.PrimaryScreenWidth - input.ActualWidth;
            var topStart = SystemParameters.PrimaryScreenHeight - input.ActualHeight;

            input.Left = leftStart;
            input.Top = SystemParameters.PrimaryScreenHeight; //If we directly set the top start value, then it will just appear. So we start with the border value
            input.Opacity = 1; //Set opacity so that it becomes visible.

            //Start an animation to move the window from bottom. 
            //our target should reach the top start value.
            var _animation =  new DoubleAnimation(topStart, new Duration(TimeSpan.FromMilliseconds(1500)))
            {
                EasingFunction = new PowerEase { EasingMode = EasingMode.EaseInOut }
            };

            input.BeginAnimation(TopProperty, _animation);

            if (input.AutoClose)
            {
                input._displayDuration = display_seconds;
                input._autoClose(); //Initiate autoclose for the input notification window.
            }
            return true;
        }
        #endregion

        #region Common Properties
        public string Id { get; private set; }
        #endregion

        #region Dependency Properties
        public bool AutoClose
        {
            get { return (bool)GetValue(AutoCloseProperty); }
            set { SetValue(AutoCloseProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AutoClose.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoCloseProperty =
            DependencyProperty.Register(nameof(AutoClose), typeof(bool), typeof(Notification), new PropertyMetadata(true));
        public int CountDown
        {
            get { return (int)GetValue(CountDownProperty); }
            set { SetValue(CountDownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CountDown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountDownProperty =
            DependencyProperty.Register(nameof(CountDown), typeof(int), typeof(Notification), new FrameworkPropertyMetadata(0));

        public string AppName
        {
            get { return (string)GetValue(AppNameProperty); }
            set { SetValue(AppNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppNameProperty =
            DependencyProperty.Register(nameof(AppName), typeof(string), typeof(Notification), new PropertyMetadata(null));

        public NotificationType Type
        {
            get { return (NotificationType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(NotificationType), typeof(Notification), new PropertyMetadata(NotificationType.ShowInfo));
        
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(Notification), new PropertyMetadata(null));

        public NotificationIcon NotificationIcon
        {
            get { return (NotificationIcon)GetValue(NotificationIconProperty); }
            set { SetValue(NotificationIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotificationIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationIconProperty =
            DependencyProperty.Register(nameof(NotificationIcon), typeof(NotificationIcon), typeof(Notification), new PropertyMetadata(NotificationIcon.Info));

        public bool ShowNotificationIcon
        {
            get { return (bool)GetValue(ShowNotificationIconProperty); }
            set { SetValue(ShowNotificationIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowNotificationIcon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowNotificationIconProperty =
            DependencyProperty.Register(nameof(ShowNotificationIcon), typeof(bool), typeof(Notification), new PropertyMetadata(true));

        public SolidColorBrush AccentColor
        {
            get { return (SolidColorBrush)GetValue(AccentColorProperty); }
            set { SetValue(AccentColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentColorProperty =
            DependencyProperty.Register(nameof(AccentColor), typeof(SolidColorBrush), typeof(Notification), new FrameworkPropertyMetadata(_baseAccent));

        public SolidColorBrush AccentForeground
        {
            get { return (SolidColorBrush)GetValue(AccentForegroundProperty); }
            set { SetValue(AccentForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AccentForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AccentForegroundProperty =
            DependencyProperty.Register(nameof(AccentForeground), typeof(SolidColorBrush), typeof(Notification), new FrameworkPropertyMetadata(_baseAccentForeground));

        public SolidColorBrush ToastForeground
        {
            get { return (SolidColorBrush)GetValue(ToastForegroundProperty); }
            set { SetValue(ToastForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToastForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToastForegroundProperty =
            DependencyProperty.Register(nameof(ToastForeground), typeof(SolidColorBrush), typeof(Notification), new PropertyMetadata(_baseToastForeground));

        public Brush ToastBackground
        {
            get { return (Brush)GetValue(ToastBackgroundProperty); }
            set { SetValue(ToastBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToastBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToastBackgroundProperty =
            DependencyProperty.Register(nameof(ToastBackground), typeof(Brush), typeof(Notification), new PropertyMetadata(_baseToastAccent));

        public string UserInput
        {
            get { return (string)GetValue(UserInputProperty); }
            set { SetValue(UserInputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UserInput.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UserInputProperty =
            DependencyProperty.Register(nameof(UserInput), typeof(string), typeof(Notification), new FrameworkPropertyMetadata(null,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region Private Methods
        private void _autoClose()
        {
            _autoCloseTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) //count happens every one second.
            };
            _autoCloseTimer.Tick += _autoCloseTimer_Tick;
            _autoCloseTimer.Start();
        }

        private void _autoCloseTimer_Tick(object sender, EventArgs e)
        {
            if (IsMouseOver)
            {
                _timerCount = 0; //Whenever mouse is moved over, timer is reset.
                return;
            }

            _timerCount++;
            this.SetCurrentValue(CountDownProperty, (_displayDuration - _timerCount));
            if (_timerCount >= _displayDuration) this.Close();
        }

        void _eventSubscription()
        {
            if (_header != null)
            {
                _header.PreviewMouseLeftButtonDown += _header_PreviewMouseLeftButtonDown;
            }
        }

        private void _header_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        void _closeAction(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (Type != NotificationType.ToastInfo)
                {
                    //Close can be raised from either any place.
                    bool dialogresult = (bool)e.Parameter;
                    this.DialogResult = dialogresult;
                }
            }
            catch (Exception)
            {
                if (Type != NotificationType.ToastInfo)
                {
                    this.DialogResult = false;
                }
            }
            finally
            {
                this.Close();
            }
        }
        void _closeAllToasts(object sender, ExecutedRoutedEventArgs e)
        {
            //Get all the notifications from app domain and then verify for toastinfo type and close them
            try
            {
                foreach (var wndw in Application.Current.Windows)
                {
                    if (wndw is Notification _notify_wndw)
                    {
                       if (_notify_wndw.Type == NotificationType.ToastInfo)
                        {
                            try
                            {
                                _notify_wndw.Close();
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
        }

        #endregion
    }
}

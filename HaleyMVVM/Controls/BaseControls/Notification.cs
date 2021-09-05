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
using System.Windows.Media.Effects;

namespace Haley.WPF.BaseControls
{
    //Notification is not a control. It is a window that will be displayed to user as a separate dialog or as a toast in the desktop.
    public sealed class Notification : Window, INotification
    {
        #region Attributes
        private static SolidColorBrush _baseAccent = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF324862");
        private static SolidColorBrush _baseAccentForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("Yellow");
        private static SolidColorBrush _baseToastAccent = (SolidColorBrush)new BrushConverter().ConvertFromString("#BF222832");
        private static SolidColorBrush _baseToastForeground = (SolidColorBrush)new BrushConverter().ConvertFromString("White");

        private const string UIEheader = "PART_header";
        private const string UIEcontainerHolder = "PART_ContainerHolder";

        private int _displayDuration = 5;
        private int _timerCount;
        private DispatcherTimer _autoCloseTimer;
        private BlurEffect _wndwBlur = new BlurEffect();
        #endregion

        #region UIElements
        private FrameworkElement _header;
        private ContentControl _containerHolder;
        #endregion

        #region Constructors
        static Notification()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Notification), new FrameworkPropertyMetadata(typeof(Notification)));

        }

        public Notification()
        {
            InitiateWindows();
            //Assign an ID
            Id = Guid.NewGuid().ToString();

            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, _closeAction));
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, _closeAllToasts));

        }

        private void InitiateWindows()
        {
            //If you are working on windows form and using WPF controls using ElementHost, then it might result in error.
            if (null == Application.Current)
            {
                new Application();
                //Only when we are dealing with no current WPF application (in other words, when working in Windows form)
                Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            }
        }
        #endregion

        #region Overridden Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _header = GetTemplateChild(UIEheader) as FrameworkElement;
            _containerHolder = GetTemplateChild(UIEcontainerHolder) as ContentControl;
            _initiation();
        }
        #endregion

        #region Public Methods

        private static void _showDialog(ref Notification input, bool blurWindows)
        {
            var _wndw = input as Window;
            if (_wndw != null && _wndw?.WindowStartupLocation == WindowStartupLocation.CenterOwner)
            {
                var mainWndw = Application.Current.MainWindow;

                //if nothing pans out below, we might not have any owner window.
                if (mainWndw != null && mainWndw.IsVisible)
                {
                    _wndw.Owner = Application.Current.MainWindow;
                }
                else
                {
                    foreach (Window otherwindow in Application.Current.Windows)
                    {
                        if (otherwindow.IsActive == true)
                        {
                            _wndw.Owner = otherwindow;
                            break;
                        }
                    }
                }
            }
            if (blurWindows) input.BlurWindows(true); //Show blur
            var result = input.ShowDialog();
            if (blurWindows) input.BlurWindows(false); //Deactiavate blur after dialog is closed.
        }

        public static INotification ShowDialog(Notification input,bool blurWindows = false)
        {
            if (input.Type == DisplayType.ToastInfo || input.Type == DisplayType.ContainerView)
            {
                // We should not have toast info type. If by some mistake, it was set, it has to be changed to notification.
                input.Type = DisplayType.ShowInfo;
            }
            _showDialog(ref input, blurWindows);
            return (INotification)input;
        }

        public static INotification ShowContainerView(Notification input,bool blurWindows = false)
        {
            if (input.ContainerView.DataContext is IHaleyVM _dc)
            {
                _dc.ViewModelClosed += (o, e) => { input.Close(); };
            }
            _showDialog(ref input, blurWindows);
                                                       
            //Now get the viewmodel of the container view and add it to result.
            var _vm = input.ContainerView.DataContext;
                input.ContainerViewModel = _vm;
            return (INotification)input;
        }

        public static bool SendToast(Notification input, int display_seconds = 7)
        {
            var _appTitle = AppDomain.CurrentDomain?.FriendlyName;
            if (input.AppName == null) input.AppName = _appTitle;
            input.Type = DisplayType.ToastInfo; //Just to re-assure that the type is of toast 
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
        public object ContainerViewModel { get; set; }
        public UserControl ContainerView { get; set; }
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

        public DisplayType Type
        {
            get { return (DisplayType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(nameof(Type), typeof(DisplayType), typeof(Notification), new PropertyMetadata(DisplayType.ShowInfo));
        
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
        private void BlurWindows(bool activate)
        {
            try
            {
                switch(activate)
                {
                    case true:
                        _wndwBlur.Radius = 8;
                        foreach (Window wndw in Application.Current.Windows)
                        {
                            if (wndw is Notification) continue;
                            wndw.Effect = _wndwBlur;
                        }
                        break;
                    case false:
                        _wndwBlur.Radius = 0; //Remove blur. This effect is already applied to all windows. So they will get updated.
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
        private void _initiation()
        {
            _eventSubscription();
            _containerViewSetup();
        }
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

        void _containerViewSetup()
        {
            if (Type != DisplayType.ContainerView || ContainerView == null || _containerHolder == null) return;

            _containerHolder.Content = ContainerView; //If viewmodel is setup using ContainerStore, then it should fetch that.
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
                if (Type != DisplayType.ToastInfo)
                {
                    //Close can be raised from either any place.
                    bool dialogresult = (bool)e.Parameter;
                    this.DialogResult = dialogresult;
                }
            }
            catch (Exception)
            {
                if (Type != DisplayType.ToastInfo)
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
                       if (_notify_wndw.Type == DisplayType.ToastInfo)
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

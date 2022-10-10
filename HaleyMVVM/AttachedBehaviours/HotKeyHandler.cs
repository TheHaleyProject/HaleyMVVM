//using Accessibility;
using Haley.Events;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Haley.Models {
    public class HotKeyHandler : Behavior<FrameworkElement> {
        #region Attributes
        DispatcherTimer CleanTimer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 1350) }; //1.35 seconds
        ConcurrentDictionary<Key, Key> _pressedKeys = new ConcurrentDictionary<Key, Key>();
        //SemaphoreSlim _repeatKey = new SemaphoreSlim(1); //If we are dealing with repeat key, we allow only one item at a time.
        #endregion

        #region Properties

        public event EventHandler<HotKeyArgs> KeyDown;

        public ICommand KeyDownCommand {
            get { return (ICommand)GetValue(KeyDownCommandProperty); }
            set { SetValue(KeyDownCommandProperty, value); }
        }
        public static readonly DependencyProperty KeyDownCommandProperty =
            DependencyProperty.Register("KeyDownCommand", typeof(ICommand), typeof(HotKeyHandler), new PropertyMetadata(null));
        #endregion

        #region Protected Methods
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += HandleKeyDown;
            AssociatedObject.PreviewKeyUp += HandleKeyUp;
            AssociatedObject.LostFocus += HandleLostFocus;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= HandleKeyDown;
            AssociatedObject.PreviewKeyUp -= HandleKeyUp;
            AssociatedObject.LostFocus -= HandleLostFocus;
            _pressedKeys.Clear();
        }
        #endregion

        #region Private Methods
        private void HandleLostFocus(object sender, RoutedEventArgs e) {
            _pressedKeys.Clear();
        }

        private List<Key> Cleanup(List<Key> keys) {
            List<Key> _toremove = new List<Key>();
            foreach (var key in keys) {
                if (!Keyboard.IsKeyDown(key)) {
                    _toremove.Add(key);
                }
            }
            var result = keys.Except(_toremove).Distinct().ToList();
            return result;
        }

        private async void CleanPressedKeys(int millisecond_delay = 0) {
            await Task.Delay(millisecond_delay);
            var _keys = _pressedKeys.Keys;
            foreach (var _key in _keys) {
                if (!Keyboard.IsKeyDown(_key)) {
                    _pressedKeys.TryRemove(_key, out var _removed);
                }
            }
        }

        private void HandleKeyDown(object sender, KeyEventArgs e) {
            //ADD
            try {
                if (_pressedKeys.ContainsKey(e.Key)) {
                    CleanPressedKeys(100); //to avoid continuous/repeated pressing of buttons. You need to give it a time interval before invoking again.
                    return;
                }
                _pressedKeys.TryAdd(e.Key, e.Key);
                //e.Handled = true; //handle this
                //send a copy of the list before cleaning it up.
                InvokeCommand(e);
                TimerBasedCleanup();
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void TimerBasedCleanup() {
            CleanTimer.Stop();
            CleanTimer.Start();
            //Sometimes, when trying to clean up, the system is very fast to process and we end up assuming that the key is still down and thus not getting removed.
            //to avoid this, run a timer to clean it up (incase, we are still not holding the key).
        }

        private void HandleKeyUp(object sender, KeyEventArgs e) {
            try {
                if (_pressedKeys.ContainsKey(e.Key)) {
                    _pressedKeys.TryRemove(e.Key, out var removed);
                }
                //e.Handled = true; //handle this
            } catch (Exception ex) {
                Debug.WriteLine(ex.ToString());
            }
        }
        private void HandleTimer(object sender, EventArgs e) {
            CleanTimer.Stop(); //so we run only once.
            CleanPressedKeys(); //clean immediately
        }

        private void InvokeCommand(KeyEventArgs e) {
            var _keys = _pressedKeys.Keys.ToList();
            //Don't send the actual list, because it might result in collection modified erro
            var cleanedKeys = Cleanup(_keys); //CLEAN BEFORE RAISING EVENT.
            cleanedKeys.Sort();
            var parameters = new HotKeyArgs(){
                AssociatedObject = AssociatedObject,
                PressedKeys = cleanedKeys,
                SourceArg = e};
            if ((KeyDownCommand != null) && KeyDownCommand.CanExecute(parameters)) {
                KeyDownCommand.Execute(parameters);
            }
            KeyDown?.Invoke(this, parameters);
        }
        #endregion

        public HotKeyHandler() {
            CleanTimer.Tick += HandleTimer;
        }
    }
}

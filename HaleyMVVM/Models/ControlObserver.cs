using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Haley.Abstractions;
using Haley.Events;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Haley.Models
{
    public class ControlObserver
    {
        public UserControl View { get; }
        public IHaleyVM ViewModel { get; }

        void _subscribeAll()
        {
            View.Loaded += _onWindowLoaded;
        }

        private void _onWindowLoaded(object sender, RoutedEventArgs e)
        {
            //loaded event is immediately unsubscribed.
            ViewModel.OnViewLoaded(sender);
            View.Loaded -= _onWindowLoaded;
        }

        public ControlObserver(UserControl subscriberView, IHaleyVM publisherViewModel)
        {
            View = subscriberView;
            ViewModel = publisherViewModel;
            _subscribeAll();
        }
    }
}

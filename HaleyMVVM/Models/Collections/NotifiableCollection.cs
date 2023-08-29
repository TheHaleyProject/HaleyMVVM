using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Haley.Models
{
    //Whenever a collection is changed, like something is added or removed, notifications are raised. However, when the properties inside the objects of the collection are changed, we never get notified. So to solve this issue, we need to find a way to trigger the collectionchanged event whenever a property of the object changes.
    //Base idea is that we register a handler for the propertychanged events of the objects. Whenever this property changes, we trigger the collectionchanged event.
    //We can also setup some delegate methods to perform boolean operations to validate whether to trigger the collectionchanged event or not.

    public sealed class NotifiableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged //NotifiableCollection will accept only items that has INotifyPropertyChanged implemented.
                                         //We are inheriting ObservableCollection Class.
                                         //We are adorning with a sealed modifier so that it will not be inherited by some other class.
    {
        private void _registerCollection()
        {
            //Since we have inherited observablecollection, we can merely make use of its properties.
            CollectionChanged += _collectionChanged;
        }

        /// <summary>
        /// Set a delegate method which takes an object argument and returns a bool. Do remember to set your parameters as well. By default,the parameters are null.
        /// </summary>
        public Trigger trigger { get; set; }
        public delegate bool Trigger(params object[] args);
        #region Collection Adding Methods

        /// <summary>
        /// Can add 
        /// </summary>
        /// <param name="Items"></param>
        public NotifiableCollection(List<T> Items) : this() //You can also use Base(Items) in case you need to make use of the methods present inside the abstract classes. For new initialization
        {
            if (Items == null) return;
            //if (Items.Count == 0 || Items == null) return;
            foreach (var item in Items)
            {
                base.Add(item);
            }
            Items.ForEach(p => { if (p != null) p.PropertyChanged += _propertyChanged; } );
        }

        public NotifiableCollection(IEnumerable<T> Items) : this() //For new initialization
        {
            if (Items == null) return;
            //if (Items.ToList().Count == 0 || Items == null) return;
            foreach (var item in Items)
            {
                base.Add(item);
            }
            Items.ToList().ForEach(p => { if (p != null) p.PropertyChanged += _propertyChanged; });
        }

        public void AddRange(List<T> Items) //This is add range, which will add the items to the existing values.
        {
            if (Items == null) return;
            //if (Items.Count == 0 || Items == null) return;
            foreach (var item in Items)
            {
                base.Add(item);
            }
            Items.ForEach(p => { if (p != null) p.PropertyChanged += _propertyChanged; });
        }

        public new void Add(T Item)
        {
            base.Add(Item); //Add to the base observable collection
            if (Item != null)Item.PropertyChanged += _propertyChanged;
        }

        #endregion

        public NotifiableCollection() : base()
        { _registerCollection(); }

        public NotifiableCollection(Func<object, bool> _trigger) : base()
        { _registerCollection(); trigger = trigger; }

        #region Event Handlers

        private void _collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //By default when a collection changes, the notifypropertychanged will be invoked. So that whichever class implements it will be able to notify. However, earlier we are also registering all the properties of the objects inside the collection to invoke the trigger. We need to turn that off now. Else, even if the items are removed from the collection, we will still get the notifications
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item == null) continue;
                    ((INotifyPropertyChanged)item).PropertyChanged += _propertyChanged;
                }
            }
            if (e.OldItems != null) //Meaning that items have been removed. We need to remove the hanlder for their property changed events.
            {
                foreach (var item in e.OldItems)
                {
                    if (item == null) continue;
                    ((INotifyPropertyChanged)item).PropertyChanged -= _propertyChanged;
                }
            }
        }

        private void _propertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (trigger == null || trigger.Invoke(sender, e) == true) //If the trigger is null or else the trigger returns true, then invoke collection changed.
            {
                NotifyCollectionChangedEventArgs TempArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
                OnCollectionChanged(TempArgs); //Invoking the collection changed event (of the base class), which in turn will be handled by the R_CollectionChanged
            }
        }
        #endregion
    }
}

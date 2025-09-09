using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using NV.CT.CommonAttributeUI.AOPAttribute;

namespace NV.CT.ProtocolManagement.ViewModels.Common
{
    [Serializable]
    public sealed class PeremptoryObservableCollection<T> : ObservableCollection<T>
    where T : INotifyPropertyChanged
    {
        public PeremptoryObservableCollection()
        {
            CollectionChanged += FullObservableCollectionCollectionChanged;
        }
        [UIRoute]
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        }

        public PeremptoryObservableCollection(IEnumerable<T> items) : this()
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        private void FullObservableCollectionCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is not null)
            {
                foreach (object item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += ItemPropertyChanged;
                }
            }
            if (e.OldItems is not null)
            {
                foreach (object item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= ItemPropertyChanged;
                }
            }
        }
        [UIRoute]
        private void ItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args = new(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
            OnCollectionChanged(args);
        }
    }
}

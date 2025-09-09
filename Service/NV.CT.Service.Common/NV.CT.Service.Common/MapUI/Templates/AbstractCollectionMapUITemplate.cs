using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.Common.Models;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public partial class AbstractCollectionMapUITemplate<TModel, TCollection, TValue> : AbstractMapUITemplate<TModel, TCollection>
            where TModel : class, INotifyPropertyChanged
            where TCollection : class, IList<TValue>
    {
        private ObservableCollection<ValueWrapper<TValue>> _values = null!;
        private ValueWrapper<TValue>? _selectedItem;

        /// <summary>
        /// 是否内部操作
        /// </summary>
        protected bool IsInner;

        protected AbstractCollectionMapUITemplate(MapUIDto mapUIDto, TModel instance) : base(mapUIDto, instance)
        {
            Token = $"{typeof(TModel).Name}.{SourcePropertyName}.{DateTime.Now:yyMMddHHmmssfff}";
            SetValues(SourceValues);
            Register();
        }

        public string Token { get; }
        protected TCollection SourceValues => GetSource();

        public ObservableCollection<ValueWrapper<TValue>> Values
        {
            get => _values;
            set
            {
                if (SetProperty(ref _values, value) && !IsInner)
                {
                    IsInner = true;
                    SourceValues.Clear();

                    foreach (var item in value)
                    {
                        SourceValues.Add(item.Value);
                    }

                    IsInner = false;
                }
            }
        }

        public ValueWrapper<TValue>? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private void SetValues(IList<TValue> items)
        {
            IsInner = true;
            Values = items is { Count: > 0 } ? new(SourceValues.Select(i => new ValueWrapper<TValue>(i, Token))) : new();
            SelectedItem = Values.FirstOrDefault();
            IsInner = false;
        }

        private void Register()
        {
            if (SourceValues is INotifyCollectionChanged notify)
            {
                notify.CollectionChanged += SourceCollectionChanged;
            }

            Values.CollectionChanged += WrapperCollectionChanged;
            RegisterPropertyChanged(() => SetValues(SourceValues));
            WeakReferenceMessenger.Default.Register<ValueWrapper<TValue>, string>(this, Token, OnValueWrapperChanged);
        }

        #region Changed

        private void OnValueWrapperChanged(object recipient, ValueWrapper<TValue> changedValue)
        {
            if (IsInner)
            {
                return;
            }

            var index = Values.IndexOf(changedValue);

            if (index < 0)
            {
                return;
            }

            IsInner = true;
            SourceValues[index] = changedValue.Value;
            IsInner = false;
        }

        private void SourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsInner)
            {
                return;
            }

            IsInner = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems != null)
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            Values.Insert(e.NewStartingIndex + i, new((TValue)e.NewItems[i]!, Token));
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems != null)
                    {
                        for (var i = e.OldItems.Count - 1; i >= 0; i--)
                        {
                            Values.RemoveAt(e.OldStartingIndex + i);
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    if (e is { OldItems: not null, NewItems: not null } && e.OldItems.Count == e.NewItems.Count && e.OldStartingIndex == e.NewStartingIndex)
                    {
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            Values[e.OldStartingIndex + i].Value = (TValue)e.NewItems[i]!;
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    if (e is { OldItems: not null, NewItems: not null } && e.OldItems.Count == e.NewItems.Count)
                    {
                        if (e.OldStartingIndex > e.NewStartingIndex)
                        {
                            for (var i = 0; i < e.OldItems.Count; i++)
                            {
                                Values.Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                            }
                        }
                        else if (e.OldStartingIndex < e.NewStartingIndex)
                        {
                            for (var i = e.OldItems.Count - 1; i >= 0; i--)
                            {
                                Values.Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                            }
                        }
                    }
                    else
                    {
                        Values.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Reset:
                {
                    Values.Clear();
                    break;
                }
            }

            IsInner = false;
        }

        private void WrapperCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsInner)
            {
                return;
            }

            IsInner = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems != null)
                    {
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            SourceValues.Insert(e.NewStartingIndex + i, ((ValueWrapper<TValue>)e.NewItems[i]!).Value);
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems != null)
                    {
                        for (var i = e.OldItems.Count - 1; i >= 0; i--)
                        {
                            SourceValues.RemoveAt(e.OldStartingIndex + i);
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    if (e is { OldItems: not null, NewItems: not null } && e.OldItems.Count == e.NewItems.Count && e.OldStartingIndex == e.NewStartingIndex)
                    {
                        for (var i = 0; i < e.OldItems.Count; i++)
                        {
                            SourceValues[e.OldStartingIndex + i] = ((ValueWrapper<TValue>)e.NewItems[i]!).Value;
                        }
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Move:
                {
                    if (e is { OldItems: not null, NewItems: not null } && e.OldItems.Count == e.NewItems.Count)
                    {
                        if (e.OldStartingIndex > e.NewStartingIndex)
                        {
                            for (var i = 0; i < e.OldItems.Count; i++)
                            {
                                Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                            }
                        }
                        else if (e.OldStartingIndex < e.NewStartingIndex)
                        {
                            for (var i = e.OldItems.Count - 1; i >= 0; i--)
                            {
                                Move(e.OldStartingIndex + i, e.NewStartingIndex + i);
                            }
                        }
                    }
                    else
                    {
                        Move(e.OldStartingIndex, e.NewStartingIndex);
                    }

                    void Move(int oldIndex, int newIndex)
                    {
                        if (SourceValues is ObservableCollection<TValue> observable)
                        {
                            observable.Move(oldIndex, newIndex);
                        }
                        else
                        {
                            var item = SourceValues[e.OldStartingIndex];
                            SourceValues.Remove(item);
                            SourceValues.Insert(e.NewStartingIndex, item);
                        }
                    }

                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    SourceValues.Clear();
                    break;
                }
            }

            IsInner = false;
        }

        #endregion

        #region Command

        [RelayCommand]
        private void Add()
        {
            Values.Add(new(default!, Token));
        }

        [RelayCommand]
        private void Delete(ValueWrapper<TValue> item)
        {
            var isSelected = SelectedItem == item;
            Values.Remove(item);

            if (isSelected)
            {
                SelectedItem = Values.FirstOrDefault();
            }
        }

        #endregion
    }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace NV.CT.UI.ViewModel;

[Serializable]
public class ObservableDictionary<TKey, TValue> :
     IDictionary<TKey, TValue>,
     ICollection<KeyValuePair<TKey, TValue>>,
     IEnumerable<KeyValuePair<TKey, TValue>>,
     IDictionary,
     ICollection,
     IEnumerable,
     ISerializable,
     IDeserializationCallback,
     INotifyCollectionChanged,
     INotifyPropertyChanged where TKey : notnull
{
    #region Constructors

    #region public

    public ObservableDictionary()
    {
        _keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>();
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
    {
        _keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>();

        foreach (var entry in dictionary)
            DoAddEntry(entry.Key, entry.Value);
    }

    public ObservableDictionary(IEqualityComparer<TKey> comparer)
    {
        _keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>(comparer);
    }

    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
    {
        _keyedEntryCollection = new KeyedDictionaryEntryCollection<TKey>(comparer);

        foreach (var entry in dictionary)
            DoAddEntry(entry.Key, entry.Value);
    }

    #endregion public

    #region protected

    protected ObservableDictionary(SerializationInfo info, StreamingContext context)
    {
        _siInfo = info;
    }

    #endregion protected

    #endregion constructors

    #region Properties

    #region public

    public IEqualityComparer<TKey> Comparer
    {
        get { return _keyedEntryCollection.Comparer; }
    }

    public int Count
    {
        get { return _keyedEntryCollection.Count; }
    }

    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
        get { return TrueDictionary.Keys; }
    }

    public TValue this[TKey key]
    {
        get { return _keyedEntryCollection.Contains(key) ? (TValue)_keyedEntryCollection[key].Value : default; }
        set { DoSetEntry(key, value); }
    }

    public Dictionary<TKey, TValue>.ValueCollection Values
    {
        get { return TrueDictionary.Values; }
    }

    #endregion public

    #region private

    private Dictionary<TKey, TValue> TrueDictionary
    {
        get
        {
            if (_dictionaryCacheVersion != _version)
            {
                _dictionaryCache.Clear();
                foreach (DictionaryEntry entry in _keyedEntryCollection)
                    _dictionaryCache.Add((TKey)entry.Key, (TValue)entry.Value);
                _dictionaryCacheVersion = _version;
            }
            return _dictionaryCache;
        }
    }

    #endregion private

    #endregion properties

    #region Methods

    #region public

    public void Add(TKey key, TValue value)
    {
        DoAddEntry(key, value);
    }

    public void Clear()
    {
        DoClearEntries();
    }

    public bool ContainsKey(TKey key)
    {
        return _keyedEntryCollection.Contains(key);
    }

    public bool ContainsValue(TValue value)
    {
        return TrueDictionary.ContainsValue(value);
    }

    public IEnumerator GetEnumerator()
    {
        return new Enumerator<TKey, TValue>(this, false);
    }

    public bool Remove(TKey key)
    {
        return DoRemoveEntry(key);
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        bool result = _keyedEntryCollection.Contains(key);
        if (result && _keyedEntryCollection[key].Value is TValue dd)
        {
            value = dd;
        }
        else
        {
            value = default;
        }
        return result;
    }

    #endregion public

    #region protected

    protected virtual bool AddEntry(TKey key, TValue value)
    {
        _keyedEntryCollection.Add(new DictionaryEntry(key, value));
        return true;
    }

    protected virtual bool ClearEntries()
    {
        // check whether there are entries to clear
        bool result = Count > 0;
        if (result)
        {
            // if so, clear the dictionary
            _keyedEntryCollection.Clear();
        }
        return result;
    }

    protected int GetIndexAndEntryForKey(TKey key, out DictionaryEntry entry)
    {
        entry = new DictionaryEntry();
        int index = -1;
        if (_keyedEntryCollection.Contains(key))
        {
            entry = _keyedEntryCollection[key];
            index = _keyedEntryCollection.IndexOf(entry);
        }
        return index;
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        if (CollectionChanged is not null)
            CollectionChanged(this, args);
    }

    protected virtual void OnPropertyChanged(string name)
    {
        if (PropertyChanged is not null)
            PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

    protected virtual bool RemoveEntry(TKey key)
    {
        // remove the entry
        return _keyedEntryCollection.Remove(key);
    }

    protected virtual bool SetEntry(TKey key, TValue value)
    {
        bool keyExists = _keyedEntryCollection.Contains(key);

        // if identical key/value pair already exists, nothing to do
        if (keyExists && Equals(value, _keyedEntryCollection[key].Value))
            return false;

        // otherwise, remove the existing entry
        if (keyExists)
            _keyedEntryCollection.Remove(key);

        // add the new entry
        _keyedEntryCollection.Add(new DictionaryEntry(key, value));

        return true;
    }

    #endregion protected

    #region private

    private void DoAddEntry(TKey key, TValue value)
    {
        if (AddEntry(key, value))
        {
            _version++;

            DictionaryEntry entry;
            int index = GetIndexAndEntryForKey(key, out entry);
            FireEntryAddedNotifications(entry, index);
        }
    }

    private void DoClearEntries()
    {
        if (ClearEntries())
        {
            _version++;
            FireResetNotifications();
        }
    }

    private bool DoRemoveEntry(TKey key)
    {
        DictionaryEntry entry;
        int index = GetIndexAndEntryForKey(key, out entry);

        bool result = RemoveEntry(key);
        if (result)
        {
            _version++;
            if (index > -1)
                FireEntryRemovedNotifications(entry, index);
        }

        return result;
    }

    private void DoSetEntry(TKey key, TValue value)
    {
        DictionaryEntry entry;
        int index = GetIndexAndEntryForKey(key, out entry);

        if (SetEntry(key, value))
        {
            _version++;

            // if prior entry existed for this key, fire the removed notifications
            if (index > -1)
            {
                FireEntryRemovedNotifications(entry, index);

                // force the property change notifications to fire for the modified entry
                _countCache--;
            }

            // then fire the added notifications
            index = GetIndexAndEntryForKey(key, out entry);
            FireEntryAddedNotifications(entry, index);
        }
    }

    private void FireEntryAddedNotifications(DictionaryEntry entry, int index)
    {
        // fire the relevant PropertyChanged notifications
        FirePropertyChangedNotifications();

        // fire CollectionChanged notification
        if (index > -1 && entry.Value is not null)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value), index));
        }
        else
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private void FireEntryRemovedNotifications(DictionaryEntry entry, int index)
    {
        // fire the relevant PropertyChanged notifications
        FirePropertyChangedNotifications();

        // fire CollectionChanged notification
        if (index > -1 && entry.Value is not null)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value), index));
        else
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void FirePropertyChangedNotifications()
    {
        if (Count != _countCache)
        {
            _countCache = Count;
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnPropertyChanged("Keys");
            OnPropertyChanged("Values");
        }
    }

    private void FireResetNotifications()
    {
        // fire the relevant PropertyChanged notifications
        FirePropertyChangedNotifications();

        // fire CollectionChanged notification
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    #endregion private

    #endregion methods

    #region Interfaces

    #region IDictionary<TKey, TValue>

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        DoAddEntry(key, value);
    }

    bool IDictionary<TKey, TValue>.Remove(TKey key)
    {
        return DoRemoveEntry(key);
    }

    bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
    {
        return _keyedEntryCollection.Contains(key);
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
    {
        return TryGetValue(key, out value);
    }

    ICollection<TKey> IDictionary<TKey, TValue>.Keys
    {
        get { return Keys; }
    }

    ICollection<TValue> IDictionary<TKey, TValue>.Values
    {
        get { return Values; }
    }

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get { return (TValue)_keyedEntryCollection[key].Value; }
        set { DoSetEntry(key, value); }
    }

    #endregion IDictionary<TKey, TValue>

    #region IDictionary

    void IDictionary.Add(object key, object? value)
    {
        if (value is not null)
        {
            DoAddEntry((TKey)key, (TValue)value);
        }
        //else
        //{
        //    DoAddEntry((TKey)key, default);
        //}
    }

    void IDictionary.Clear()
    {
        DoClearEntries();
    }

    bool IDictionary.Contains(object key)
    {
        return _keyedEntryCollection.Contains((TKey)key);
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return new Enumerator<TKey, TValue>(this, true);
    }

    bool IDictionary.IsFixedSize
    {
        get { return false; }
    }

    bool IDictionary.IsReadOnly
    {
        get { return false; }
    }

    object? IDictionary.this[object key]
    {
        get { return _keyedEntryCollection[(TKey)key].Value; }

        set
        {
            if (value is TValue tValue)
            {
                DoSetEntry((TKey)key, tValue);
            }
        }
    }

    ICollection IDictionary.Keys
    {
        get { return Keys; }
    }

    void IDictionary.Remove(object key)
    {
        DoRemoveEntry((TKey)key);
    }

    ICollection IDictionary.Values
    {
        get { return Values; }
    }

    #endregion IDictionary

    #region ICollection<KeyValuePair<TKey, TValue>>

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> kvp)
    {
        DoAddEntry(kvp.Key, kvp.Value);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
        DoClearEntries();
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> kvp)
    {
        return _keyedEntryCollection.Contains(kvp.Key);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
    {
        if (array is null)
        {
            throw new ArgumentNullException("CopyTo() failed:  array parameter was null");
        }
        if (index < 0 || index > array.Length)
        {
            throw new ArgumentOutOfRangeException(
                "CopyTo() failed:  index parameter was outside the bounds of the supplied array");
        }
        if (array.Length - index < _keyedEntryCollection.Count)
        {
            throw new ArgumentException("CopyTo() failed:  supplied array was too small");
        }

        foreach (DictionaryEntry entry in _keyedEntryCollection)
        {
            if (entry.Value is not null)
            {
                array[index++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
            }
        }
    }

    int ICollection<KeyValuePair<TKey, TValue>>.Count
    {
        get { return _keyedEntryCollection.Count; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
    {
        get { return false; }
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> kvp)
    {
        return DoRemoveEntry(kvp.Key);
    }

    #endregion ICollection<KeyValuePair<TKey, TValue>>

    #region ICollection

    void ICollection.CopyTo(Array array, int index)
    {
        ((ICollection)_keyedEntryCollection).CopyTo(array, index);
    }

    int ICollection.Count
    {
        get { return _keyedEntryCollection.Count; }
    }

    bool ICollection.IsSynchronized
    {
        get { return ((ICollection)_keyedEntryCollection).IsSynchronized; }
    }

    object ICollection.SyncRoot
    {
        get { return ((ICollection)_keyedEntryCollection).SyncRoot; }
    }

    #endregion ICollection

    #region IEnumerable<KeyValuePair<TKey, TValue>>

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return new Enumerator<TKey, TValue>(this, false);
    }

    #endregion IEnumerable<KeyValuePair<TKey, TValue>>

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion IEnumerable

    #region ISerializable

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info is null)
        {
            throw new ArgumentNullException("info");
        }

        var entries = new Collection<DictionaryEntry>();
        foreach (DictionaryEntry entry in _keyedEntryCollection)
            entries.Add(entry);
        info.AddValue("entries", entries);
    }

    #endregion ISerializable

    #region IDeserializationCallback

    public virtual void OnDeserialization(object? sender)
    {
        if (_siInfo is not null)
        {
            var entries = _siInfo.GetValue("entries", typeof(Collection<DictionaryEntry>));
            if (entries is null)
            {
                return;
            }
            var entriesList = (Collection<DictionaryEntry>)entries;
            foreach (DictionaryEntry entry in entriesList)
            {
                if (entry.Value is null)
                {
                    continue;
                }
                AddEntry((TKey)entry.Key, (TValue)entry.Value);
            }
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is ObservableDictionary<TKey, TValue> dictionary &&
               EqualityComparer<Dictionary<TKey, TValue>.KeyCollection>.Default.Equals(Keys, dictionary.Keys);
    }

    #endregion IDeserializationCallback

    #region INotifyCollectionChanged

   event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
    {
        add { CollectionChanged += value; }
        remove { CollectionChanged -= value; }
    }

    protected virtual event NotifyCollectionChangedEventHandler? CollectionChanged;

    #endregion INotifyCollectionChanged

    #region INotifyPropertyChanged

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add { PropertyChanged += value; }
        remove { PropertyChanged -= value; }
    }

    protected virtual event PropertyChangedEventHandler? PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion interfaces

    #region Protected classes

    #region KeyedDictionaryEntryCollection<T>

    protected class KeyedDictionaryEntryCollection<T> : KeyedCollection<T, DictionaryEntry> where T : notnull
    {
        #region constructors

        #region public

        public KeyedDictionaryEntryCollection()
        {
        }

        public KeyedDictionaryEntryCollection(IEqualityComparer<T> comparer) : base(comparer)
        {
        }

        #endregion public

        #endregion constructors

        #region methods

        #region protected

        protected override T GetKeyForItem(DictionaryEntry entry)
        {
            return (T)entry.Key;
        }

        #endregion protected

        #endregion methods
    }

    #endregion KeyedDictionaryEntryCollection<TKey>

    #endregion protected classes

    #region Public Structures

    #region Enumerator

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct Enumerator<TKey1, TValue1> : IEnumerator<KeyValuePair<TKey1, TValue1>>, IDisposable,
        IDictionaryEnumerator, IEnumerator where TKey1 : notnull
    {
        #region constructors

        internal Enumerator(ObservableDictionary<TKey1, TValue1> dictionary, bool isDictionaryEntryEnumerator)
        {
            _dictionary = dictionary;
            _version = dictionary._version;
            _index = -1;
            _isDictionaryEntryEnumerator = isDictionaryEntryEnumerator;
            _current = new KeyValuePair<TKey1, TValue1>();
        }

        #endregion constructors

        #region properties

        #region public

        public KeyValuePair<TKey1, TValue1> Current
        {
            get
            {
                ValidateCurrent();
                return _current;
            }
        }

        #endregion public

        #endregion properties

        #region methods

        #region public

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            ValidateVersion();
            _index++;
            if (_index < _dictionary._keyedEntryCollection.Count)
            {
                _current = new KeyValuePair<TKey1, TValue1>((TKey1)_dictionary._keyedEntryCollection[_index].Key,
                    (TValue1)_dictionary._keyedEntryCollection[_index].Value);
                return true;
            }
            _index = -2;
            _current = new KeyValuePair<TKey1, TValue1>();
            return false;
        }

        #endregion public

        #region private

        private void ValidateCurrent()
        {
            if (_index == -1)
            {
                throw new InvalidOperationException("The enumerator has not been started.");
            }
            if (_index == -2)
            {
                throw new InvalidOperationException("The enumerator has reached the end of the collection.");
            }
        }

        private void ValidateVersion()
        {
            if (_version != _dictionary._version)
            {
                throw new InvalidOperationException("The enumerator is not valid because the dictionary changed.");
            }
        }

        #endregion private

        #endregion methods

        #region IEnumerator implementation

        object IEnumerator.Current
        {
            get
            {
                ValidateCurrent();
                if (_isDictionaryEntryEnumerator && _current.Key is not null)
                {
                    return new DictionaryEntry(_current.Key, _current.Value);
                }
                return new KeyValuePair<TKey1, TValue1>(_current.Key, _current.Value);
            }
        }

        void IEnumerator.Reset()
        {
            ValidateVersion();
            _index = -1;
            _current = new KeyValuePair<TKey1, TValue1>();
        }

        #endregion IEnumerator implemenation

        #region IDictionaryEnumerator implemenation

        DictionaryEntry IDictionaryEnumerator.Entry
        {
            get
            {
                ValidateCurrent();
                if (_current.Key is not null)
                {
                    return new DictionaryEntry(_current.Key, _current.Value);
                }
                return new DictionaryEntry(string.Empty, _current.Value);
            }
        }

        object IDictionaryEnumerator.Key
        {
            get
            {
                ValidateCurrent();
                return _current.Key;
            }
        }

        object IDictionaryEnumerator.Value
        {
            get
            {
                ValidateCurrent();

                return _current.Value;
            }
        }

        #endregion

        #region fields

        private readonly ObservableDictionary<TKey1, TValue1> _dictionary;
        private readonly int _version;
        private int _index;
        private KeyValuePair<TKey1, TValue1> _current;
        private readonly bool _isDictionaryEntryEnumerator;

        #endregion fields
    }

    #endregion Enumerator

    #endregion public structures

    #region Fields

    private readonly Dictionary<TKey, TValue> _dictionaryCache = new Dictionary<TKey, TValue>();
    [NonSerialized] private readonly SerializationInfo _siInfo;
    private int _countCache;
    private int _dictionaryCacheVersion;
    protected KeyedDictionaryEntryCollection<TKey> _keyedEntryCollection;
    private int _version;

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    #endregion fields
}
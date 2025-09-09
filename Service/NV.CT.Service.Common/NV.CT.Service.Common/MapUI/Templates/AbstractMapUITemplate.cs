using System;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public abstract class AbstractMapUITemplate : ObservableObject
    {
    }

    public abstract class AbstractMapUITemplate<TModel, TValue> : AbstractMapUITemplate where TModel : class, INotifyPropertyChanged
    {
        protected readonly MapUIDto MapUIDto;
        protected readonly TModel Instance;
        protected readonly string SourcePropertyName;
        protected readonly Func<TValue> GetSource;
        protected readonly Action<TValue> SetSource;
        private string _title;
        private bool _isEnabled = true;
        private readonly Func<TModel, bool>? _isEnabledFunc;

        protected AbstractMapUITemplate(MapUIDto mapUIDto, TModel instance)
        {
            MapUIDto = mapUIDto;
            Instance = instance;
            SourcePropertyName = mapUIDto.SourcePropertyName;
            GetSource = () => ((Func<TModel, TValue>)mapUIDto.GetMethod)(instance);
            SetSource = i => (mapUIDto.SetMethod as Action<TModel, TValue>)?.Invoke(instance, i);
            _title = mapUIDto.Title;
            _isEnabledFunc = mapUIDto.IsEnabledMethod as Func<TModel, bool>;
            IsReadOnly = mapUIDto.SetMethod == null;
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public bool IsEnabled
        {
            get => _isEnabledFunc?.Invoke(Instance) ?? _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsReadOnly { get; }

        protected void RegisterPropertyChanged(Action? action)
        {
            Instance.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == SourcePropertyName)
                {
                    action?.Invoke();
                }

                if (MapUIDto.IsEnabledPropertyNames?.Contains(args.PropertyName) == true)
                {
                    OnPropertyChanged(nameof(IsEnabled));
                }
            };
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace NV.CT.Service.Common.Models
{
    public class ValueWrapper<T> : ObservableObject
    {
        private T _value;

        public ValueWrapper(T value)
        {
            _value = value;
            PropertyChanged += (_, _) => WeakReferenceMessenger.Default.Send(this);
        }

        public ValueWrapper(T value, string token)
        {
            _value = value;
            PropertyChanged += (_, _) => WeakReferenceMessenger.Default.Send(this, token);
        }

        public T Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
    }
}

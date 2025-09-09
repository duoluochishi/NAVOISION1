using System.ComponentModel;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public class SimpleMapUITemplate<TModel, TValue> : AbstractMapUITemplate<TModel, TValue> where TModel : class, INotifyPropertyChanged
    {
        public SimpleMapUITemplate(MapUIDto mapUIDto, TModel instance) : base(mapUIDto, instance)
        {
            RegisterPropertyChanged(() => OnPropertyChanged(nameof(Value)));
        }

        public TValue Value
        {
            get => GetSource();
            set => SetProperty(GetSource(), value, SetSource);
        }
    }
}
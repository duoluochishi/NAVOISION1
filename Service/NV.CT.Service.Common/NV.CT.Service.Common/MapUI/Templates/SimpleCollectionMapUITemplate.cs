using System.Collections.Generic;
using System.ComponentModel;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public class SimpleCollectionMapUITemplate<TModel, TCollection, TValue> : AbstractCollectionMapUITemplate<TModel, TCollection, TValue>
            where TModel : class, INotifyPropertyChanged
            where TCollection : class, IList<TValue>
    {
        public SimpleCollectionMapUITemplate(MapUIDto mapUIDto, TModel instance) : base(mapUIDto, instance)
        {
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public class EnumCollectionMapUITemplate<TModel, TCollection, TValue> : SimpleCollectionMapUITemplate<TModel, TCollection, TValue>
            where TModel : class, INotifyPropertyChanged
            where TCollection : class, IList<TValue>
            where TValue : struct, Enum
    {
        public EnumCollectionMapUITemplate(MapUIDto mapUIDto, TModel instance) : base(mapUIDto, instance)
        {
            EnumDic = Enum.GetValues<TValue>()
                          .ToDictionary(i =>
                           {
                               var description = typeof(TValue).GetField(i.ToString())
                                                              ?.GetCustomAttribute<DescriptionAttribute>()
                                                              ?.Description;
                               return string.IsNullOrWhiteSpace(description) ? i.ToString() : description;
                           }, i => i);
        }

        public Dictionary<string, TValue> EnumDic { get; private set; }
    }
}
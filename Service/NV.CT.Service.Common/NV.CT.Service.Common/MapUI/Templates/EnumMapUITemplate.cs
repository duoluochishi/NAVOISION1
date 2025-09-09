using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public class EnumMapUITemplate<TModel, TValue> : SimpleMapUITemplate<TModel, TValue>
            where TModel : class, INotifyPropertyChanged
            where TValue : struct, Enum
    {
        public EnumMapUITemplate(MapUIDto mapUIDto, TModel instance) : base(mapUIDto, instance)
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
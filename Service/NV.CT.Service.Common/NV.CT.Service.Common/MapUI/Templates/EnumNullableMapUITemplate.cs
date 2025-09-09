using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace NV.CT.Service.Common.MapUI.Templates
{
    public class EnumNullableMapUITemplate<TModel, TValue> : SimpleMapUITemplate<TModel, TValue?>
            where TModel : class, INotifyPropertyChanged
            where TValue : struct, Enum
    {
        public EnumNullableMapUITemplate(MapUIDto mapUIDto, TModel instance) : base(mapUIDto, instance)
        {
            var propertyType = typeof(TModel).GetProperty(mapUIDto.SourcePropertyName)?.PropertyType;

            if (propertyType == null)
            {
                throw new ArgumentNullException(mapUIDto.SourcePropertyName);
            }

            if (!propertyType.IsGenericType || propertyType != typeof(TValue?))
            {
                throw new ArgumentException($"{mapUIDto.SourcePropertyName} not {typeof(TValue?).Name} Type");
            }

            EnumDic = new Dictionary<string, TValue?> { { "Null", null } };

            foreach (var value in Enum.GetValues<TValue>())
            {
                var description = typeof(TValue).GetField(value.ToString())
                                               ?.GetCustomAttribute<DescriptionAttribute>()
                                               ?.Description;
                var key = string.IsNullOrWhiteSpace(description) ? value.ToString() : description;
                EnumDic.Add(key, value);
            }
        }

        public Dictionary<string, TValue?> EnumDic { get; private set; }
    }
}
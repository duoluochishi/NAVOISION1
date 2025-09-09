using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using NV.CT.Service.Common.MapUI.Templates;

namespace NV.CT.Service.Common.MapUI
{
    public static class MapUIExtension
    {
        private static readonly ConcurrentDictionary<(Type ModelType, string PropertyName), MapUIDto> CacheDic = new();

        public static IEnumerable<AbstractMapUITemplate> GetMapUITemplates<T>(this T model) where T : INotifyPropertyChanged
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var template = model.GetMapUITemplate(property.Name, true);

                if (template != null)
                {
                    yield return template;
                }
            }
        }

        public static AbstractMapUITemplate GetMapUITemplate<T>(this T model, string propertyName) where T : INotifyPropertyChanged
        {
            return model.GetMapUITemplate(propertyName, false)!;
        }

        private static AbstractMapUITemplate? GetMapUITemplate<T>(this T model, string propertyName, bool isIgnore) where T : INotifyPropertyChanged
        {
            if (!CacheDic.TryGetValue((typeof(T), propertyName), out var cache))
            {
                var property = typeof(T).GetProperty(propertyName);

                if (property == null)
                {
                    if (isIgnore)
                    {
                        return null;
                    }

                    throw new ArgumentException($@"The {propertyName} property does not exist in the {typeof(T).Name} type", propertyName);
                }

                var mapUI = property.GetCustomAttribute<MapUIAttribute>();

                if (mapUI == null)
                {
                    if (isIgnore)
                    {
                        return null;
                    }

                    throw new ArgumentException($@"The {nameof(MapUIAttribute)} does not exist in the {typeof(T).Name}.{propertyName}", propertyName);
                }

                var getMethod = property.GetMethod?.CreateDelegate(typeof(Func<,>).MakeGenericType(typeof(T), property.PropertyType));

                if (getMethod == null)
                {
                    throw new ArgumentException($@"Cannot support unreadable property {typeof(T).Name}.{propertyName}", propertyName);
                }

                var templateType = mapUI.TemplateType;
                var setMethod = property.SetMethod?.IsPublic == true ? property.SetMethod?.CreateDelegate(typeof(Action<,>).MakeGenericType(typeof(T), property.PropertyType)) : null;
                Delegate? isEnabledMethod = null;

                if (templateType is { IsGenericType: true, IsGenericTypeDefinition: true })
                {
                    var typeArgs = new List<Type> { typeof(T), property.PropertyType };

                    if (property.PropertyType.IsGenericType && property.PropertyType.IsAssignableTo(typeof(IList)))
                    {
                        typeArgs.AddRange(property.PropertyType.GenericTypeArguments);
                    }

                    templateType = templateType.MakeGenericType(typeArgs.ToArray());
                }

                if (mapUI.IsEnabledManagerType != null && !string.IsNullOrWhiteSpace(mapUI.IsEnabledManagerFuncName))
                {
                    isEnabledMethod = mapUI.IsEnabledManagerType.GetProperty(mapUI.IsEnabledManagerFuncName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as Delegate;
                }

                cache = new MapUIDto
                {
                    SourcePropertyName = propertyName,
                    Title = mapUI.Title,
                    TemplateType = templateType,
                    GetMethod = getMethod,
                    SetMethod = setMethod,
                    IsEnabledMethod = isEnabledMethod,
                    IsEnabledPropertyNames = mapUI.IsEnabledPropertyNames,
                };
                CacheDic.TryAdd((typeof(T), propertyName), cache);
            }

            var template = Activator.CreateInstance(cache.TemplateType, cache, model) as AbstractMapUITemplate;
            return template!;
        }
    }
}
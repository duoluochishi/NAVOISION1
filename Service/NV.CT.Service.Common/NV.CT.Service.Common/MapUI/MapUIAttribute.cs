using System;

namespace NV.CT.Service.Common.MapUI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class MapUIAttribute : Attribute
    {
        public MapUIAttribute(string title, Type templateType)
        {
            Title = title;
            TemplateType = templateType;
        }

        public MapUIAttribute(Type resourceType, string resourceName, Type templateType) : this(GetResource(resourceType, resourceName), templateType)
        {
        }

        public MapUIAttribute(string title, Type templateType, Type isEnabledManagerType, string isEnabledManagerFuncName, string[] isEnabledPropertyNames) : this(title, templateType)
        {
            IsEnabledManagerType = isEnabledManagerType;
            IsEnabledManagerFuncName = isEnabledManagerFuncName;
            IsEnabledPropertyNames = isEnabledPropertyNames;
        }

        public MapUIAttribute(Type resourceType, string resourceName, Type templateType, Type isEnabledManagerType, string isEnabledManagerFuncName, string[] isEnabledPropertyNames) : this(GetResource(resourceType, resourceName), templateType, isEnabledManagerType, isEnabledManagerFuncName, isEnabledPropertyNames)
        {
        }

        public string Title { get; }
        public Type TemplateType { get; }
        public Type? IsEnabledManagerType { get; }
        public string? IsEnabledManagerFuncName { get; }
        public string[]? IsEnabledPropertyNames { get; }

        private static string GetResource(Type resourceType, string resourceName)
        {
            return resourceType.GetProperty(resourceName)?.GetValue(null) as string ?? string.Empty;
        }
    }
}
using System;

namespace NV.CT.Service.Common.MapUI
{
    public class MapUIDto
    {
        public string SourcePropertyName { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public Type TemplateType { get; init; } = default!;
        public Delegate GetMethod { get; init; } = default!;
        public Delegate? SetMethod { get; init; }
        public Delegate? IsEnabledMethod { get; init; }
        public string[]? IsEnabledPropertyNames { get; init; }
    }
}
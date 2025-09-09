using NV.CT.Service.HardwareTest.Services.Universal.RegionalStaticResource;
using System;
using System.Windows.Markup;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public class RegionalStaticResourceExtension : MarkupExtension
    {
        public string ResourceKey { get; set; } = null!;

        public RegionalStaticResourceExtension()
        {
                
        }

        public RegionalStaticResourceExtension(string resourceKey)
        {
            if (resourceKey == null)
            {
                throw new ArgumentNullException(nameof(resourceKey));
            }

            this.ResourceKey = resourceKey;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(ResourceKey)) 
            {
                throw new ArgumentException($"Invalid Resource Key: [{ResourceKey}]");
            }

            return RegionalStaticResourceService.Instance.GetResource(ResourceKey);
        }
    }
}

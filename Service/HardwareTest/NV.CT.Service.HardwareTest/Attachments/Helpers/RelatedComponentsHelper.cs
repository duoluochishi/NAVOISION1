using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    internal static class RelatedComponentsHelper
    {
        public static uint ParseRelatedComponentsValue(IEnumerable<RelatedComponent> relatedComponents) 
        {
            int tempValue = 0;

            foreach (RelatedComponent relatedComponent in relatedComponents) 
            {
                if (relatedComponent.IsChecked) 
                {
                    tempValue |= (1 << relatedComponent.BitOffset);
                }
            }

            return (uint)tempValue;
        }
    }
}

using NV.CT.FacadeProxy.Common.Models.Collimator;
using NV.CT.Service.HardwareTest.Attachments.LibraryCallers;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    internal static class CollimatorPositionInfoExtension
    {
        public static IEnumerable<CollimatorData> ToAlgorithmModel(this IEnumerable<CollimatorPositionInfo> collimatorPositionInfos) 
        {
            foreach (var item in collimatorPositionInfos)
            {
                yield return new CollimatorData() 
                {
                    frontBlade = (int)item.FrontBlade,
                    rearBlade = (int)item.RearBlade,
                    bowtie = (int)item.Bowtie
                };
            }
        }
    }
}

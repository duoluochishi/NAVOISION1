using NV.CT.Service.HardwareTest.Attachments.LibraryCallers;
using NV.CT.Service.HardwareTest.Models.Components.Collimator;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    internal static class CollimatorCalibrationIterativeResultExtension
    {

        public static void Update(this IEnumerable<CollimatorCalibrationIterativeResult> results, NextIter iterativeResult) 
        {
            for (int i = 0; i < results.Count(); i++)
            {
                var singleResult = results.ElementAt(i);
                singleResult.IterativceCollimatorPosition = iterativeResult.collimatorTargetPosition[i];
                singleResult.IterativceDetectorPosition = iterativeResult.collimatorDectectorPosition[i];
            }
        }

        public static void Reset(this IEnumerable<CollimatorCalibrationIterativeResult> results) 
        {
            foreach (var result in results) 
            {
                result.Reset();
            }
        }

        public static IEnumerable<uint> ToNextRoundTargetPositions(this IEnumerable<CollimatorCalibrationIterativeResult> results)
        {
            return results.Select(t => (uint)t.IterativceCollimatorPosition);
        }
    }
}

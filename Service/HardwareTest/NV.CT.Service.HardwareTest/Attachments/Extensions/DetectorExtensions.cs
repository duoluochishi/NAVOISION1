
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.MPS.Configuration;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class DetectorExtensions
    {
        public static IEnumerable<DetectBoardDto> ToDetectBoardDtos(this IEnumerable<DetectBoardExchangeRecord> detectBoardExchangeRecords)
        {
            foreach (var record in detectBoardExchangeRecords) 
            {
                yield return new DetectBoardDto() 
                {
                    DetectorModuleID = record.DetectorModuleID,
                    InstallTime = record.InstallTime,
                    Using = record.Using,
                    RetireTime = record.RetireTime,
                    SeriesNumber = record.SeriesNumber
                };
            }
        }

        public static IEnumerable<DetectBoardExchangeRecord> ToDetectBoardExchangeRecords(this IEnumerable<DetectBoardDto> detectBoardDtos) 
        {
            foreach (var detectBoardDto in detectBoardDtos)
            {
                yield return new DetectBoardExchangeRecord()
                {
                    DetectorModuleID = detectBoardDto.DetectorModuleID,
                    InstallTime = detectBoardDto.InstallTime,
                    Using = detectBoardDto.Using,
                    RetireTime = detectBoardDto.RetireTime,
                    SeriesNumber = detectBoardDto.SeriesNumber
                };
            }
        }

    }
}

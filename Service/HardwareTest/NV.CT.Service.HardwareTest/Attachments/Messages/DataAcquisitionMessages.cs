using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;
using RawDataHelperWrapper;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Attachments.Messages
{
    public record ImageChainImageSortedMessage(IEnumerable<RawData> sortedImages);

    public record RawDataSetSortedMessage(IEnumerable<RawData> sortedRawDataSet);

    public record RawDataSetLoadedMessage(List<AbstractRawDataInfo> loadedRawDataInfos);

    public record RawDataSetLoadedLoggerMessage(string message, LoggerLevel level = LoggerLevel.Error);

    public enum LoggerLevel
    {
        Info,
        Warn,
        Error
    }

}

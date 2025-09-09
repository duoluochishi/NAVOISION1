using NVCTImageViewerInterop;
using System.Collections.Generic;

namespace NV.CT.Service.Common.Controls.Attachments.Messages
{
    public record LoadRawDataSeriesMessage(List<ImageRawDataInfo> ImageRawDataSeries);
}

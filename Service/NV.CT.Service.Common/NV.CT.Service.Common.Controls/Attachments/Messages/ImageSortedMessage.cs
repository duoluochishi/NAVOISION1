using RawDataHelperWrapper;
using System.Collections.Generic;

namespace NV.CT.Service.Common.Controls.Attachments.Messages
{
    public record ImageSortedMessage(IEnumerable<RawData> sortedImages);
}

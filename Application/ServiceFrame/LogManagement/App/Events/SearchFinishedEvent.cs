using NV.CT.LogManagement.Models;
using Prism.Events;
using System.Collections.Generic;

namespace NV.CT.LogManagement.Events
{
    public class SearchFinishedEvent : PubSubEvent<List<LogLineModel>>
    {
    }
}

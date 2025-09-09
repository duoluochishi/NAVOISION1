using NV.CT.LogManagement.Models;
using Prism.Events;

namespace NV.CT.LogManagement.Events
{
    public class LocationJumpedEvent : PubSubEvent<LogLineModel>
    {
    }
}

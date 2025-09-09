using System;

namespace NV.CT.LogManagement.Events
{
    public class EventAggregator
    {
        private static readonly Lazy<Prism.Events.EventAggregator> _instance = new Lazy<Prism.Events.EventAggregator>(() => new Prism.Events.EventAggregator());

        public static Prism.Events.EventAggregator Instance => _instance.Value;

        private EventAggregator()
        {
        }
    }
}

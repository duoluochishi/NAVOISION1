using NV.CT.Service.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static object locker = new object();

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> values) 
        {
            DispatcherWrapper.Invoke(() => 
            {
                foreach (var item in values)
                {
                    lock (locker) 
                    {
                        collection.Add(item);
                    }                  
                }
            });
        }

        public static void ForEach<T>(this ObservableCollection<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

    }
}

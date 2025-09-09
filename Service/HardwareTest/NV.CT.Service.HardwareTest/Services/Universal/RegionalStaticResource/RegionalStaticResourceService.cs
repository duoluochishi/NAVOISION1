using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace NV.CT.Service.HardwareTest.Services.Universal.RegionalStaticResource
{
    public class RegionalStaticResourceService
    {

        public static RegionalStaticResourceService Instance { get; } = new RegionalStaticResourceService();

        private RegionalStaticResourceService()
        {
            this.regionalResourceDictionary = new();
        }

        #region Fields

        private Dictionary<string, object> regionalResourceDictionary;

        #endregion

        public void AddResourceDictionary(string rosourceDictionaryUrl) 
        {
            ResourceDictionary resourceDictionary = new ResourceDictionary() 
            {
                Source = new Uri(rosourceDictionaryUrl)
            };

            this.AddResourceDictionaryInternal(resourceDictionary);
        }

        public void AddResourceDictionary(ResourceDictionary resourceDictionary) 
        {
            this.AddResourceDictionaryInternal(resourceDictionary);
        }

        private void AddResourceDictionaryInternal(ResourceDictionary resourceDictionary) 
        {
            var resourceDictionaryCollection = resourceDictionary.MergedDictionaries;

            if (resourceDictionaryCollection == null || resourceDictionaryCollection.Count <= 1)
            {
                foreach (DictionaryEntry item in resourceDictionary)
                {
                    this.AddResource(item.Key.ToString()!, item.Value!);
                }
            }
            else 
            {
                foreach (ResourceDictionary dictionary in resourceDictionaryCollection) 
                {
                    this.AddResourceDictionary(dictionary);
                }
            }
        }

        public void AddResource(string resourceKey, object resourceValue) 
        {
            if (regionalResourceDictionary.TryGetValue(resourceKey, out object? temp)) 
            {
                throw new ArgumentException($"Repeated Resource Key: [{resourceKey}]");
            }

            regionalResourceDictionary[resourceKey] = resourceValue;
        }

        public object GetResource(string resourceKey) 
        {
            if (regionalResourceDictionary.TryGetValue(resourceKey, out object? temp))
            {
                return temp;
            }
            else 
            {
                throw new KeyNotFoundException($"The Resource Key [{resourceKey}] dose not exist.");
            }
        }

    }
}

using Newtonsoft.Json;
using NV.CT.ServiceFrame.ApplicationService.Contract.Models;
using NV.CT.ServiceFrame.Extensions;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.Environment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NV.CT.ServiceFrame;

public class DynamicLoader
{
    private static Lazy<DynamicLoader> _instance = new Lazy<DynamicLoader>(() => new DynamicLoader());

    private DynamicLoader()
    {
        Initialize();
    }

    public static DynamicLoader Instance => _instance.Value;

    private List<Item> _appItems;

    private void Initialize()
    {
        var fileName = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Adjustment", "appsetting.json");

        var root = JsonConvert.DeserializeObject<Root>(File.ReadAllText(fileName));

        _appItems = root.Items;
    }

    public Item GetAppItem(string appName)
    {
        return _appItems.FirstOrDefault(a => a.Title == appName);
    }

    public List<IBootLoader?> Load(string appName)
    {
        var appItem = GetAppItem(appName);

        var loaderTypes = new List<Type>();

        if (!string.IsNullOrEmpty(appItem.CommonAssembly))
        {
            var assemblyResolver = new AssemblyResolver(Path.Combine(RuntimeConfig.Console.MCSBin.Path, appItem.CommonAssembly));
            var types = assemblyResolver.Assembly.GetTypes();
            var loaderType = assemblyResolver.Assembly.GetTypes().FirstOrDefault(t => (typeof(IBootLoader).IsAssignableFrom(t)));
            if (loaderType is not null && !loaderTypes.Contains(loaderType))
            {
                loaderTypes.Add(loaderType);
            }
        }

        foreach (var child in appItem.Children)
        {
            var assemblyResolver = new AssemblyResolver(Path.Combine(RuntimeConfig.Console.MCSBin.Path, child.AppAssemblyPath));
            var types = assemblyResolver.Assembly.GetTypes();
            var loaderType = assemblyResolver.Assembly.GetTypes().FirstOrDefault(t => (typeof(IBootLoader).IsAssignableFrom(t)));
            if (loaderType is not null && !loaderTypes.Contains(loaderType))
            {
                loaderTypes.Add(loaderType);
            }
        }

        var loaders = loaderTypes.Select(l => { return Activator.CreateInstance(l) as IBootLoader; }).ToList();

        return loaders;
    }
}

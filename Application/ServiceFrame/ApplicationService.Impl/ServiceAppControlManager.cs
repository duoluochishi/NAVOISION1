using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.AppService.Contract;
using NV.CT.ServiceFrame.ApplicationService.Contract.Interfaces;
using NV.CT.ServiceFrame.ApplicationService.Contract.Models;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.Environment;

namespace NV.CT.ServiceFrame.ApplicationService.Impl;

public class ServiceAppControlManager : IServiceAppControlManager
{
    private List<Item> _items;
    private readonly ILogger<ServiceAppControlManager> _logger;
    IApplicationCommunicationService _applicationCommunicationService;
    public event EventHandler<object> OnChildrenSet;
    public event EventHandler<ApplicationResponse> ApplicationClosing;
    private string _fileName;

    public ServiceAppControlManager(ILogger<ServiceAppControlManager> logger, IApplicationCommunicationService applicationCommunicationService)
    {
        _logger = logger;
        _applicationCommunicationService = applicationCommunicationService;
        _applicationCommunicationService.NotifyApplicationClosing += ApplicationCommunicationService_NotifyApplicationClosing;
        _fileName = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Adjustment/appsetting.json");
        Deserialize();
    }

    private void ApplicationCommunicationService_NotifyApplicationClosing(object? sender, ApplicationResponse e)
    {
        ApplicationClosing?.Invoke(sender, e);
    }

    public List<ChildrenItem> GetAppItems(string modelName)
    {
        try
        {
            var item = LoadServiceAppConfig(modelName);
            return item;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    private void Deserialize()
    {
        var content = File.ReadAllText(_fileName);
        var root = JsonConvert.DeserializeObject<Root>(content);
        if (root != null)
        {
            _items = root.Items;
        }
        else
        {
            _items = new List<Item>();
        }

    }
    private List<ChildrenItem> LoadServiceAppConfig(string modelName)
    {
        var item = _items.FirstOrDefault(s => s.Title == modelName);
        if (item != null)
        {

            foreach (var appItem in item.Children)
            {
                AddServiceAppItem(appItem);
            }
            return item.Children;
        }
        else
        {
            return new List<ChildrenItem>();
        }

    }
    private bool AddServiceAppItem(ChildrenItem item)
    {
        if (item is null)
        {
            return false;
        }
        var assemblyPath = Path.Combine(RuntimeConfig.Console.MCSBin.Path, item.AppAssemblyPath);
        using var dynamicContext = new AssemblyResolver(assemblyPath);

        var type = dynamicContext.Assembly.GetType(item.ControlName, true, false);
        if (null != type)
        {
            var obj = Activator.CreateInstance(type);
            item.AppControl = obj as IServiceControl;
        }
        return true;
    }

    public void SetChildren(object children)
    {
        OnChildrenSet?.Invoke(this, children);
    }
}
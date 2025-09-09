using NV.CT.AppService.Contract;
using NV.CT.ServiceFrame.ApplicationService.Contract.Models;

namespace NV.CT.ServiceFrame.ApplicationService.Contract.Interfaces;

public interface IServiceAppControlManager
{
    List<ChildrenItem> GetAppItems(string modelName);
    void SetChildren(object children);
    event EventHandler<object> OnChildrenSet;
    event EventHandler<ApplicationResponse> ApplicationClosing;
}

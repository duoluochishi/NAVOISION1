using NV.CT.ServiceFramework.Contract;

namespace NV.CT.ServiceFrame.ApplicationService.Contract.Models;

[Serializable]
public class ChildrenItem
{
   public string ControlName { get; set; }      
    public string AppAssemblyPath { get; set; }
    public string Category { get; set; }
    public IServiceControl AppControl { get; set; }
}

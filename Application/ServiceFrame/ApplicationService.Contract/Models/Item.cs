namespace NV.CT.ServiceFrame.ApplicationService.Contract.Models;

[Serializable]
public class Item
{
    public string Title { get; set; }

    public string CommonAssembly { get; set; }

    public List<ChildrenItem> Children { get; set; }
}

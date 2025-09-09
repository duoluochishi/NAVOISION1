namespace NV.CT.AppService.Contract;

public class ApplicationListResponse
{
	public ApplicationListResponse()
	{
		ApplicationList = new();
	}
	public ApplicationListResponse(List<ApplicationInfo> list)
	{
		ApplicationList = list;
	}
	public List<ApplicationInfo> ApplicationList { get; set; }
}
namespace NV.CT.Service.HardwareTest.Share.Models
{
    public record DataResponse<T>(bool status, string message, T data) where T : class;
}

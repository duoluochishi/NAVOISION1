using NV.CT.Service.Enums;
using System;

namespace NV.CT.Service.Models
{
    public record GenericResponse(bool status, string message);
    public record DataResponse<T>(bool status, string message, T? data) where T : class;
    public record LoggerMessage(string message, PrintLevel level);
}

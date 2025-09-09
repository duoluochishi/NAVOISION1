using NV.CT.ErrorCodes;

namespace NV.CT.Service.Common.Extensions
{
    public static class ErrorCodeExtension
    {
        public static string GetErrorCodeDescription(this string? code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return string.Empty;
            }

            var errorCode = ErrorCodeHelper.GetErrorCode(code);
            return errorCode.Description;
            // return errorCode.Level == ErrorLevel.NotDefined ? code : errorCode.Description;
        }
    }
}
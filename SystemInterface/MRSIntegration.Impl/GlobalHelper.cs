namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public static class GlobalHelper
{
    public static string GetErrorMessage(string errorCode)
    {
        if (string.IsNullOrEmpty(errorCode)) return string.Empty;

        var errorInfo = ErrorCodes.ErrorCodeHelper.GetErrorCode(errorCode);

        if (errorInfo == null) return string.Empty;

        return errorInfo.Description;
    }

}

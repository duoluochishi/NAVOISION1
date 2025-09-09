using System;

namespace NV.CT.Service.Upgrade
{
    internal static class Global
    {
        #region Error Code

        public const string ErrorCode_NotConnect = "MCS023000001";
        public const string ErrorCode_MD5ValidateFail = "MCS023000002";

        #endregion

        public const string ServiceAppName = "FirmwareUpgrade";
        public static IServiceProvider ServiceProvider { get; private set; }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
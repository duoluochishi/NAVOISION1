namespace NV.CT.Service.AutoCali.Logic
{
    public class ErrorCodeDefines
    {
        public static readonly string ERROR_CODE_USER_CANCELED = "UserCanceled";

        public static readonly string ERROR_CODE_DEVICE_DISCONNECTED = "MCS01700001";

        public static readonly string ERROR_CODE_MCS_MRS_ACQ_DISCONNECTED = "MCS01700002";

        public static readonly string ERROR_CODE_MCS_OFFLINE_RECON_DISCONNECTED = "MCS01700003";

        /// <summary>
        /// 离线机服务断联
        /// </summary>
        public static readonly string ERROR_CODE_MCS_OFFLINE_MACHINE_DISCONNECTED = "MCS01700004";
    }
}

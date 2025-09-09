namespace NV.CT.Service.AutoCali.Logic
{
    public class Constants
    {
        #region const
        public static readonly string MSG_CONNECTION_CONNECTED = "CONNECTED";
        public static readonly string MSG_CONNECTION_DISCONNECTED = "DISCONNECTED";

        public static readonly string MSG_MODULE_OFFLINERECON = $"OfflineRecon";
        public static readonly string MSG_OFFLINERECON_CONNECTED = $"{MSG_MODULE_OFFLINERECON} {MSG_CONNECTION_CONNECTED}.";
        public static readonly string MSG_OFFLINERECON_DISCONNECTED = $"{MSG_MODULE_OFFLINERECON} {MSG_CONNECTION_DISCONNECTED}.";
        
        public static readonly string MSG_MODULE_OFFLINE_MACHINE = $"OfflineMachine";
        public static readonly string MSG_OFFLINE_MACHINE_CONNECTED = $"{MSG_MODULE_OFFLINE_MACHINE} {MSG_CONNECTION_CONNECTED}.";
        public static readonly string MSG_OFFLINE_MACHINE_DISCONNECTED = $"{MSG_MODULE_OFFLINE_MACHINE} {MSG_CONNECTION_DISCONNECTED}.";

        public static readonly string MSG_MODULE_SCAN_RECON_ENGINE = $"ScanReconEngine";
        public static readonly string MSG_SCAN_RECON_ENGINE_CONNECTED = $"{MSG_MODULE_SCAN_RECON_ENGINE} {MSG_CONNECTION_CONNECTED}.";
        public static readonly string MSG_SCAN_RECON_ENGINE_DISCONNECTED = $"{MSG_MODULE_SCAN_RECON_ENGINE} {MSG_CONNECTION_DISCONNECTED}.";
                
        public static readonly string MSG_MODULE_DEVICE = $"Device";
        public static readonly string MSG_DEVICE_CONNECTED = $"{MSG_MODULE_DEVICE} {MSG_CONNECTION_CONNECTED}.";
        public static readonly string MSG_DEVICE_DISCONNECTED = $"{MSG_MODULE_DEVICE} {MSG_CONNECTION_DISCONNECTED}.";

        public static readonly string MSG_USER_CANCELED = "User cancelled.";

        #endregion const
    }
}

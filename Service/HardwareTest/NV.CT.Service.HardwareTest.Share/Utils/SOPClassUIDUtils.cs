namespace NV.CT.Service.HardwareTest.Share.Utils
{
    public sealed class SOPClassUIDUtils
    {
        public static readonly string SOPClassUIDCTImage = "1.2.840.10008.5.1.4.1.1.2";
        public static readonly string SOPClassUIDEnhancedCTImage = "1.2.840.10008.5.1.4.1.1.2.1";
        public static readonly string SOPClassUIDGeneralECGWaveform = "1.2.840.10008.5.1.4.1.1.9.1.2";
        public static readonly string SOPClassUIDNVInstanceHeader = "1.1.23.4.14356.1.1";
        public static readonly string SOPClassUIDSeriesInstance = "1.2.840.1.59.0.8569.2309111327464000.141";


        public static string Generate18UID()
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyymmddHHmmssffff");
            return timeStamp;
        }

        public static string Generate16UID()
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyymmddHHmmssff");
            //防止连续生成相同的uid
            Thread.Sleep(15);
            return timeStamp;
        }

    }
}

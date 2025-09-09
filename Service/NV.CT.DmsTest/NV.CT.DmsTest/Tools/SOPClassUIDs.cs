using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DmsTest.Tools
{
    public sealed class SOPClassUIDs
    {
        public static readonly string SOPClassUIDCTImage = "1.2.840.10008.5.1.4.1.1.2";
        public static readonly string SOPClassUIDEnhancedCTImage = "1.2.840.10008.5.1.4.1.1.2.1";
        public static readonly string SOPClassUIDGeneralECGWaveform = "1.2.840.10008.5.1.4.1.1.9.1.2";
        public static readonly string SOPClassUIDNVInstanceHeader = "1.1.23.4.14356.1.1";

        public static string Generate18UID()
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyymmddHHmmssffff");
            return timeStamp;
        }

        public static string Generate16UID()
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyymmddHHmmssff");
            //防止连续生成相同的uid
            System.Threading.Thread.Sleep(15);
            return timeStamp;
        }
    }
}

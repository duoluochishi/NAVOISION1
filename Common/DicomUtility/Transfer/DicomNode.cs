//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/13 15:48:44     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.Transfer
{
    public class DicomNode
    {
        public string HostIP { get; }
        public int Port { get; }
        public string CallingAE { get; }
        public string CalledAE { get; }

        public bool UseTlSecurity { get; }

        public DicomNode(string hostIp, int port, string callingAE, string calledAE, bool useTlSecurity = false)
        {
            HostIP = hostIp;
            Port = port;
            CallingAE = callingAE;
            CalledAE = calledAE;
            UseTlSecurity = useTlSecurity;
        }
    }
}

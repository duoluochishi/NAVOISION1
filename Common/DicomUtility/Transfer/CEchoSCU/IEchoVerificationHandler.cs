//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/27 13:45:36      V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.Transfer.CEchoSCU
{
    public interface IEchoVerificationHandler
    {
        /// <summary>
        /// Echo服务
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="callingAE"></param>
        /// <param name="calledAE"></param>
        /// <param name="useTlSecurity"></param>
        /// <returns></returns>
        (bool, string) VerifyEcho(string host, int port, string callingAE, string calledAE, bool useTlSecurity = false);

    }
}

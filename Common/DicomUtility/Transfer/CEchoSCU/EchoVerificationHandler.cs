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

using System.Data.Common;

namespace NV.CT.DicomUtility.Transfer.CEchoSCU
{

    public class EchoVerificationHandler : IEchoVerificationHandler
    {
        public (bool, string) VerifyEcho(string host, int port, string callingAE, string calledAE, bool useTlSecurity = false)
        {
            //使用C-Echo服务验证SCP是否可用
            var echoService = new CEchoScuExecutor();
            var echoResult = echoService.Echo(new DicomNode(host, port, callingAE, calledAE, useTlSecurity));
            //如果验证失败,则修改任务状态，并发出错误信息
            if (echoResult.Item1 is ExecuteStatus.Succeeded)
            {
                return (true, string.Empty);
            }
            else
            {
                return (false, echoResult.Item2);
            }
        }


    }
}

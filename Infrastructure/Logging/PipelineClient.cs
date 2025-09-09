//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/1 17:12:44           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.TP.NamedPipeWrapper;

namespace NV.CT.Logging
{
    public class PipelineClient
    {
        private readonly static Lazy<PipelineClient> _instance = new Lazy<PipelineClient>(() => new PipelineClient());

        private readonly NamedPipeClient<string> _client;

        private PipelineClient()
        {
            _client = new NamedPipeClient<string>("Logging");
            _client.Start();
        }

        public static PipelineClient Instance => _instance.Value;

        public void PushMessage(string message)
        {
            _client.PushMessage(message);
        }
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DicomImageViewer;
using NV.CT.Print.Models;

namespace NV.CT.Print
{
    public class Global
    {
        private static readonly Lazy<Global> _instance = new(() => new Global());

        private static ClientInfo? _clientInfo;
        private static MCSServiceClientProxy? _serviceClientProxy;
        private JobClientProxy? _jobClientProxy;

        public PrintingStudyModel? PrintingStudy { get; set; }

        public PrintImageViewer ImageViewer { get; } = new PrintImageViewer((int)SystemParameters.WorkArea.Width - 656 - 34, (int)SystemParameters.WorkArea.Height - 24);

        public static Global Instance => _instance.Value;

        private Global()
        {
        }

        public void Subscribe()
        {
            _clientInfo = new ClientInfo { Id = $"[Print]-{DateTime.Now:yyyyMMddHHmmss}" };

            _serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
            _serviceClientProxy?.Subscribe(_clientInfo);

            _jobClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<JobClientProxy>();
            _jobClientProxy?.Subscribe(_clientInfo);
        }

        public void Unsubscribe()
        {
            if (_clientInfo is null)
            {
                return;
            }

            _serviceClientProxy?.Unsubscribe(_clientInfo);
            _jobClientProxy?.Unsubscribe(_clientInfo);
        }
    }
}

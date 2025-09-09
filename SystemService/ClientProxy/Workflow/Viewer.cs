//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.Communication;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ClientProxy.Workflow
{
    internal class Viewer : IViewer
    {
        private readonly MCSServiceClientProxy _clientProxy;

        public string StudyId { get; set; } = string.Empty;

#pragma warning disable 67
        public event EventHandler<string>? ViewerChanged;
#pragma warning restore 67

        public Viewer(MCSServiceClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        public void StartViewer(string studyId)
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IViewer).Namespace,
                SourceType = nameof(IViewer),
                ActionName = nameof(IViewer.StartViewer),
                Data = studyId
            });
            //if (commandResponse.Success)
            //{
            //    var res = commandResponse.Data;         
            //}


        }
        public bool CheckExists()
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IViewer).Namespace,
                SourceType = nameof(IViewer),
                ActionName = nameof(IViewer.CheckExists),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = Convert.ToBoolean(commandResponse.Data);
                return res;
            }

            return false;
        }
    }
}

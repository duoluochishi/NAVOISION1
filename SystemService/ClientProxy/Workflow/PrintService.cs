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
using NV.CT.CTS.Enums;
using NV.CT.CTS;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Communication;

namespace NV.CT.ClientProxy.Workflow
{
    public class PrintService : IPrint
    {
        public event EventHandler<string>? StudyChanged;
        public event EventHandler<EventArgs<(string, JobTaskStatus)>>? PrintStatusChanged;
        public event EventHandler<string>? PrintStarted;
        public event EventHandler<string>? PrintClosed;

        private readonly MCSServiceClientProxy _clientProxy;


        public PrintService(MCSServiceClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        public bool ChceckExists()
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IPrint).Namespace,
                SourceType = nameof(IPrint),
                ActionName = nameof(IPrint.ChceckExists),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = Convert.ToBoolean(commandResponse.Data);
                return res;
            }

            return false;
        }

        public void StartPrint(string studyId)
        {
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IPrint).Namespace,
                SourceType = nameof(IPrint),
                ActionName = nameof(IPrint.StartPrint),
                Data = studyId
            });
        }

        public void ClosePrint()
        {
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IPrint).Namespace,
                SourceType = nameof(IPrint),
                ActionName = nameof(IPrint.ClosePrint),
                Data = string.Empty
            });
        }

        public string GetCurrentStudyId()
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IPrint).Namespace,
                SourceType = nameof(IPrint),
                ActionName = nameof(IPrint.GetCurrentStudyId),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = commandResponse.Data;
                return res;
            }
            return string.Empty;
        }

    }


}

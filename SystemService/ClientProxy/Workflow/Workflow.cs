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
using Newtonsoft.Json;

using NV.MPS.Communication;
using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Protocol.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ClientProxy.Workflow
{
    public class Workflow : IWorkflow
    {
        private readonly MCSServiceClientProxy _clientProxy;

        public event EventHandler<string>? StudyChanged;
        public event EventHandler<string>? WorkflowStatusChanged;
        public event EventHandler<string>? LockStatusChanged;

        public Workflow(MCSServiceClientProxy clientProxy)
        {
            _clientProxy = clientProxy;
        }

        public string GetCurrentStudy()
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.GetCurrentStudy),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = commandResponse.Data;
                return res;
            }

            return "";
        }

        public void StartWorkflow(string studyId)
        {
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.StartWorkflow),
                Data = studyId
            });
        }

        public bool CheckExist()
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.CheckExist),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = Convert.ToBoolean(commandResponse.Data);
                return res;
            }

            return false;
        }

        public void CloseWorkflow()
        {
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.CloseWorkflow),
                Data = string.Empty
            });
        }

        public void Locking()
        {
            //TODO: 检查过程中，界面锁定操作
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.Locking),
                Data = string.Empty
            });
        }

        public void Unlocking()
        {
            //TODO: 检查过程中，界面解锁操作
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.Unlocking),
                Data = string.Empty
            });
        }

        //public void CreateReport(string studyId)
        //{
        //    _clientProxy.ExecuteCommand(new CommandRequest()
        //    {
        //        Namespace = typeof(IWorkflow).Namespace,
        //        SourceType = nameof(IWorkflow),
        //        ActionName = nameof(IWorkflow.CreateReport),
        //        Data = studyId
        //    });
        //}

        //public void ReplaceProtocol(string templateId)
        //{
        //    _clientProxy.ExecuteCommand(new CommandRequest()
        //    {
        //        Namespace = typeof(IWorkflow).Namespace,
        //        SourceType = nameof(IWorkflow),
        //        ActionName = nameof(IWorkflow.ReplaceProtocol),
        //        Data = templateId
        //    });
        //}

        //public void ConfirmExam()
        //{
        //    _clientProxy.ExecuteCommand(new CommandRequest()
        //    {
        //        Namespace = typeof(IWorkflow).Namespace,
        //        SourceType = nameof(IWorkflow),
        //        ActionName = nameof(IWorkflow.ConfirmExam),
        //        Data = string.Empty
        //    });
        //}

        public void SelectionScan(RgtScanModel scanModel)
        {
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.SelectionScan),
                Data = scanModel.ToJson()
            });
        }

        public (StudyModel, PatientModel) GetCurrentStudyInfo()
        {
            var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.GetCurrentStudyInfo),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = JsonConvert.DeserializeObject<(StudyModel, PatientModel)>(commandResponse.Data);
                return res;
            }

            return (new StudyModel(), new PatientModel());
        }

  //      public void ResumeReconStates(string studyId)
		//{
		//	_clientProxy.ExecuteCommand(new CommandRequest()
  //          {
  //              Namespace = typeof(IWorkflow).Namespace,
  //              SourceType = nameof(IWorkflow),
  //              ActionName = nameof(IWorkflow.ResumeReconStates),
  //              Data = studyId
		//	});
		//}

        public void LockScreen()
		{
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.LockScreen),
                Data = string.Empty
            });
		}

		public void UnlockScreen(string nextScreen)
		{
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.UnlockScreen),
                Data = nextScreen
            });
		}

		//public event EventHandler? ConfirmExamChanged;
		public event EventHandler<EventArgs<RgtScanModel>>? SelectionScanChanged;
        public event EventHandler<string>? ExamStarted;
        public event EventHandler<string>? ExamClosed;
		public event EventHandler? LockScreenChanged;
		public event EventHandler<string>? UnlockScreenChanged;
		public event EventHandler? EmergencyExamStarted;

		public void EnterEmergencyExam()
        {
			_clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.EnterEmergencyExam),
                Data = string.Empty
            });
		}

        public void LeaveEmergencyExam()
        {
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.LeaveEmergencyExam),
                Data = string.Empty
            });
		}

        public bool IsEmergencyExam()
        {
			var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.IsEmergencyExam),
                Data = string.Empty
            });
            if (commandResponse.Success)
            {
                var res = Convert.ToBoolean(commandResponse.Data);
                return res;
            }

            return false;
		}

		public void RepairAbnormalStudy()
		{
            _clientProxy.ExecuteCommand(new CommandRequest()
            {
                Namespace = typeof(IWorkflow).Namespace,
                SourceType = nameof(IWorkflow),
                ActionName = nameof(IWorkflow.RepairAbnormalStudy),
                Data = string.Empty
            });
		}
	}
}
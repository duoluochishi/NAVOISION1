using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy.Workflow;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.JobViewer.ApplicationService.Contract;
using NV.CT.JobViewer.Models;
using NV.CT.UI.ViewModel;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.JobViewer.ViewModel
{
    public class TaskViewModel : BaseViewModel
    {
        private readonly IOfflineReconTaskService _offlineService;
        private readonly ILogger<TaskViewModel> _logger;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;

        private readonly IStudyService _studyService;
        private readonly IPatientService _patientService;

        public IntPtr MainWindowAuxHwnd { get; set; }

        private OfflineReconModel currentSelectItem;
        public OfflineReconModel CurrentSelectItem
        {
            get => currentSelectItem;
            set
            {
                if (null != value)
                {
                    if (value.Status == OfflineTaskStatus.Created || value.Status == OfflineTaskStatus.Waiting)
                    {
                        if (value.Priority == TaskPriority.High)
                        {
                            CanToppingTask = false;
                            CanUpgradeTask = false;
                            CanDowngradeTask = true;
                            CanMoveUpTask = false;
                            CanMoveDownTask = false;
                        }
                        else if (value.Priority == TaskPriority.Middle || value.Priority == TaskPriority.Low)
                        {
                            if (value.Index == 0)
                            {
                                CanMoveUpTask = false;
                            }
                            else
                            {
                                CanMoveUpTask = true;
                            }
                            CanMoveDownTask = true;
                            if (value.Priority == TaskPriority.Middle)
                            {
                                if (OfflineReconInfoModels.Any(t => (t.Status == OfflineTaskStatus.Created || t.Status == OfflineTaskStatus.Waiting) && t.Priority == TaskPriority.High))
                                {
                                    CanUpgradeTask = false;
                                }
                                else
                                {
                                    CanUpgradeTask = true;
                                    CanToppingTask = true;
                                }
                            }
                            if (value.Priority == TaskPriority.Low)
                            {
                                CanDowngradeTask = false;
                            }
                            else
                            {
                                CanDowngradeTask = true;
                            }
                        }
                    }
                    else
                    {
                        CanToppingTask = false;
                        CanUpgradeTask = false;
                        CanDowngradeTask = false;
                        CanMoveUpTask = false;
                        CanMoveDownTask = false;
                    }
                }
                SetProperty(ref currentSelectItem, value);
            }
        }

        private bool canToppingTask;
        public bool CanToppingTask
        {
            get => canToppingTask;
            set => SetProperty(ref canToppingTask, value);
        }
        private bool canUpgradeTask;
        public bool CanUpgradeTask
        {
            get => canUpgradeTask;
            set => SetProperty(ref canUpgradeTask, value);
        }
        private bool canDowngradeTask;
        public bool CanDowngradeTask
        {
            get => canDowngradeTask;
            set => SetProperty(ref canDowngradeTask, value);
        }
        private bool canMoveUpTask;
        public bool CanMoveUpTask
        {
            get => canMoveUpTask;
            set => SetProperty(ref canMoveUpTask, value);
        }
        private bool canMoveDownTask;
        public bool CanMoveDownTask
        {
            get => canMoveDownTask;
            set => SetProperty(ref canMoveDownTask, value);
        }
        private ObservableCollection<OfflineReconModel> offlineReconInfoModels = new();
        public ObservableCollection<OfflineReconModel> OfflineReconInfoModels
        {
            get => offlineReconInfoModels;
            set => SetProperty(ref offlineReconInfoModels, value);
        }

        public TaskViewModel(ILogger<TaskViewModel> logger, 
                             IOfflineReconTaskService offlineService, 
                             IMapper mapper, 
                             IPatientService patientService, 
                             IStudyService studyService, 
                             IDialogService dialogService)
        {
            _logger = logger;
            _mapper = mapper;
            _offlineService = offlineService;
            _dialogService = dialogService;
            _patientService = patientService;
            _studyService = studyService;

            canToppingTask = true;
            canUpgradeTask = true;
            canDowngradeTask = true;
            canMoveUpTask = true;
            canMoveDownTask = true;

            _offlineService.TaskCreated += OfflineService_ReconCreated;
            _offlineService.TaskWaiting += OfflineService_ReconWaiting;
            _offlineService.TaskStarted += OfflineService_ReconStarted;
            _offlineService.TaskCanceled += OfflineService_ReconCanceled;
            _offlineService.TaskAborted += OfflineService_ReconAborted;
            _offlineService.ImageProgressChanged += OfflineService_ImageProgressChanged;
            _offlineService.TaskFinished += OfflineService_ReconFinished;
            _offlineService.TaskDone += OfflineService_ReconDone;
            _offlineService.TaskRemoved += OfflineService_ReconRemoved;

            Commands.Add("LoadedCommand", new DelegateCommand(LoadedCommand));

            Commands.Add("ReconTaskToppingCommand", new DelegateCommand<object>(ReconTaskToppingCommand));
            //Commands.Add("ReconTaskMoveUpCommand", new DelegateCommand<object>(ReconTaskMoveUpCommand));
            //Commands.Add("ReconTaskMoveDownCommand", new DelegateCommand<object>(ReconTaskMoveDownCommand));
            //Commands.Add("ReconTaskUpgradeCommand", new DelegateCommand<object>(ReconTaskUpgradeCommand));
            //Commands.Add("ReconTaskDowngradeCommand", new DelegateCommand<object>(ReconTaskDowngradeCommand));

            Commands.Add("ReconTaskRefreshCommand", new DelegateCommand(LoadedCommand));

            Commands.Add("ReconTaskCancelCommand", new DelegateCommand<object>(ReconTaskCancelCommand));
            Commands.Add("ReconTaskIndexCommand", new DelegateCommand<object>(ReconTaskIndexCommand));
            Commands.Add("ReconTaskPriorityCommand", new DelegateCommand<object>(ReconTaskPriorityCommand));
            Commands.Add("ReconTaskStartCommand", new DelegateCommand<object>(ReconTaskStartCommand));

        }

        private void OfflineService_ReconWaiting(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == e.Data.ReconId && o.ScanId == e.Data.ScanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                offlineReconModel.Status = OfflineTaskStatus.Waiting;
                offlineReconModel.ShowStatus = offlineReconModel.Progress < 1 ? OfflineReconStatus.Waiting.ToString() : OfflineReconStatus.Finished.ToString();


                offlineReconModel.PatientId = e.Data.PatientId;

                offlineReconModel.StartEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Start);
                offlineReconModel.CancelEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Cancel);
                offlineReconModel.DeleteEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Delete);
                offlineReconModel.UpgradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Upgrade);
                offlineReconModel.DowngradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Downgrade);
            }
        }

        private void OfflineService_ReconStarted(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == e.Data.ReconId && o.ScanId == e.Data.ScanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                offlineReconModel.Status = OfflineTaskStatus.Executing;
                offlineReconModel.PatientId = e.Data.PatientId;
                offlineReconModel.ShowStatus = "Reconing";

                offlineReconModel.StartEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Start);
                offlineReconModel.CancelEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Cancel);
                offlineReconModel.DeleteEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Delete);
                offlineReconModel.UpgradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Upgrade);
                offlineReconModel.DowngradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Downgrade);
            }
        }

        private void OfflineService_ReconCanceled(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == e.Data.ReconId && o.ScanId == e.Data.ScanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                offlineReconModel.Status = OfflineTaskStatus.Cancelled;
                offlineReconModel.Progress = 0;
                offlineReconModel.ShowStatus = OfflineReconStatus.Cancelled.ToString();

                offlineReconModel.StartEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Start);
                offlineReconModel.CancelEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Cancel);
                offlineReconModel.DeleteEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Delete);
                offlineReconModel.UpgradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Upgrade);
                offlineReconModel.DowngradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Downgrade);
            }
        }

        private void OfflineService_ReconAborted(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            RunReconTaskDone(e.Data, true);
        }

        private void OfflineService_ImageProgressChanged(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {

            // no need to show recon progress in task viewer
            //OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == e.Data.ReconId && o.ScanId == e.Data.ScanId);
            //if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            //{
            //    offlineReconModel.Status = OfflineTaskStatus.Executing;
            //    offlineReconModel.Progress = e.Data.Progress;
            //    offlineReconModel.ShowStatus = offlineReconModel.Progress < 1 ? "Reconing" : OfflineTaskStatus.Finished.ToString();
            //    offlineReconModel.PatientId = e.Data.PatientId;

            //    offlineReconModel.StartEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Start);
            //    offlineReconModel.CancelEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Cancel);
            //    offlineReconModel.DeleteEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Delete);
            //    offlineReconModel.UpgradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Upgrade);
            //    offlineReconModel.DowngradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Downgrade);

            //    offlineReconModel.ShowBlue = false;
            //    offlineReconModel.ShowOrange = true;
            //}
        }

        private void LoadedCommand()
        {
            LoadOfflineReconTask();
        }

        private void OfflineService_ReconCreated(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            RunReconTaskCreated(e.Data);
        }

        private void OfflineService_ReconFinished(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            RunReconTaskDone(e.Data);
        }

        private void OfflineService_ReconDone(object? sender, CTS.EventArgs<OfflineTaskInfo> e)
        {
            RunReconTaskDone(e.Data);
        }

        private void OfflineService_ReconRemoved(object? sender, string e)
        {
            RunReconTaskRemoved(e);
        }

        private void RunReconTaskRemoved(string reconId)
        {
            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId);

            if (string.IsNullOrEmpty(reconId) || offlineReconModel is null)
            {
                _logger.LogError($"[JobViewer]Remove Recon Task failed due to Id is not exist reconId:{reconId} or offlineReconMdel is null");
                return;
            }

            UIInvoke(() =>
            {
                if (OfflineReconInfoModels.Contains(offlineReconModel))
                {
                    OfflineReconInfoModels.Remove(offlineReconModel);
                }
            });
        }

        private void RunReconTaskToppingById(string reconId, string scanId)
        {
            OfflineReconInfoModels = OfflineReconInfoModels.OrderByDescending(t => ((int)t.Priority).ToString() + t.Index.ToString().PadLeft(3, '0')).ToList().ToObservableCollection();
        }

        private void ReconTaskToppingCommand(object pOfflineReconModel)
        {
            if (pOfflineReconModel is OfflineReconModel offlineReconModel)
            {
                if (!string.IsNullOrEmpty(offlineReconModel.ReconId)) // Ensure ReconId is not null or empty
                {
                    if (offlineReconModel.Status == OfflineTaskStatus.Created || offlineReconModel.Status == OfflineTaskStatus.Waiting)
                    {
                        offlineReconModel.Priority = TaskPriority.High;
                        offlineReconModel.Index = 0;

                        _offlineService.ToppingTaskPriority(offlineReconModel.ReconId);

                        // 将选中的任务UI置顶
                        OfflineReconInfoModels.Remove(offlineReconModel);
                        OfflineReconInfoModels.Insert(0, offlineReconModel);

                        CurrentSelectItem = offlineReconModel;
                    }
                    else
                    {
                        _logger.LogWarning($"[JobViewer]Topping is ignored due to current recon task status:{offlineReconModel.Status.ToString()}");
                    }
                }
                else
                {
                    _logger.LogError("[JobViewer]ReconId is null or empty, skipping ToppingTaskPriority.");
                }
            }
        }
        private void RunReconTaskByTransfer(OfflineReconModel pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                int i = OfflineReconInfoModels.ToList().FindIndex(r => r.ReconId == pOfflineReconModel.ReconId && r.ScanId == pOfflineReconModel.ScanId);
                OfflineReconInfoModels[i] = pOfflineReconModel;
                List<OfflineReconModel> list = OfflineReconInfoModels.ToList();

                list = list.OrderBy(x => x.Status).ThenByDescending(y => y.Priority).ThenBy(x => x.Index).ThenBy(x => x.ReconTaskDateTime).ToList();

                OfflineReconInfoModels = list.ToObservableCollection();
                CurrentSelectItem = pOfflineReconModel;
                //OfflineReconInfoModels.OrderByDescending(t => ((int)t.Priority).ToString() + t.Index.ToString().PadLeft(3, '0')).ToList<OfflineReconModel>().ToObservableCollection<OfflineReconModel>();
            }
        }
        private void ReconTaskMoveUpCommand(object pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;

                int index = offlineReconModel.Index;
                index = index > 0 ? index - 1 : 0;
                offlineReconModel.Index = index;
                Task.Run(() =>
                {
                    OfflineCommandResult offlineCommandResult = _offlineService.SetReconTaskIndex(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, index);
                });

                //if (offlineCommandResult != null)
                //{
                //    if (offlineCommandResult.Status == CTS.Enums.CommandExecutionStatus.Success)
                //    {
                //
                RunReconTaskByTransfer(offlineReconModel);
                //    }
                //}
            }
        }
        private void RunReconTaskMoveDownById(string reconId, string scanId)
        {
            OfflineReconInfoModels = OfflineReconInfoModels.OrderByDescending(t => ((int)t.Priority).ToString() + t.Index.ToString().PadLeft(3, '0')).ToList().ToObservableCollection();
        }
        private void ReconTaskMoveDownCommand(object pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
                int index = offlineReconModel.Index;
                index += 1;
                offlineReconModel.Index = index;
                Task.Run(() =>
                {
                    OfflineCommandResult offlineCommandResult = _offlineService.SetReconTaskIndex(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, index);
                });

                //if (offlineCommandResult != null)
                //{
                //    if (offlineCommandResult.Status == CTS.Enums.CommandExecutionStatus.Success)
                //    {
                //
                RunReconTaskByTransfer(offlineReconModel);
                //    }
                //}
            }
        }
        private void RunReconTaskUpgradeById(string reconId, string scanId)
        {
            OfflineReconInfoModels = OfflineReconInfoModels.OrderByDescending(t => ((int)t.Priority).ToString() + t.Index.ToString().PadLeft(3, '0')).ToList().ToObservableCollection();
        }
        private void ReconTaskUpgradeCommand(object pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
                TaskPriority taskPriority = offlineReconModel.Priority;
                switch (taskPriority)
                {
                    case TaskPriority.High:
                    case TaskPriority.Middle:
                        if (!OfflineReconInfoModels.Any(t => (t.Status == OfflineTaskStatus.Created || t.Status == OfflineTaskStatus.Waiting) && t.Priority == TaskPriority.High))
                        {
                            taskPriority = TaskPriority.High;
                        }
                        break;
                    case TaskPriority.Low:
                        taskPriority = TaskPriority.Middle; break;
                }
                offlineReconModel.Priority = taskPriority;
                Task.Run(() =>
                {
                    OfflineCommandResult offlineCommandResult = _offlineService.SetReconTaskPriority(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, taskPriority);
                });

                //if (offlineCommandResult != null)
                //{
                //    if (offlineCommandResult.Status == CTS.Enums.CommandExecutionStatus.Success)
                //    {
                //
                RunReconTaskByTransfer(offlineReconModel);
                //RunReconTaskUpgradeById(offlineReconModel.ReconId, offlineReconModel.ScanId);
                //    }
                //}
            }
        }
        private void RunReconTaskDowngradeById(string reconId, string scanId)//<OfflineReconModel>
        {
            OfflineReconInfoModels = OfflineReconInfoModels.OrderByDescending(t => ((int)t.Priority).ToString() + t.Index.ToString().PadLeft(3, '0')).ToList().ToObservableCollection();
        }
        private void ReconTaskDowngradeCommand(object pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
                TaskPriority taskPriority = offlineReconModel.Priority;
                switch (taskPriority)
                {
                    case TaskPriority.Low:
                    case TaskPriority.Middle:
                        taskPriority = TaskPriority.Low; break;
                    case TaskPriority.High:
                        taskPriority = TaskPriority.Middle; break;
                }
                offlineReconModel.Priority = taskPriority;
                Task.Run(() =>
                {
                    OfflineCommandResult offlineCommandResult = _offlineService.SetReconTaskPriority(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, taskPriority);
                });

                //if (offlineCommandResult != null)
                //{
                //    if (offlineCommandResult.Status == CTS.Enums.CommandExecutionStatus.Success)
                //    {
                //
                RunReconTaskByTransfer(offlineReconModel);
                //RunReconTaskDowngradeById(offlineReconModel.ReconId, offlineReconModel.ScanId);
                //    }
                //}
            }
        }
        private void RunReconTaskCreated(OfflineTaskInfo offlineReconInfo)
        {
            using (Mutex mutex = new Mutex(false, "Created"))
            {
                mutex.WaitOne();// && o.ScanId == offlineReconInfo.ScanId
                _logger.LogInformation($"[JobViewer]Current recon task count:{OfflineReconInfoModels.Count.ToString()}, {offlineReconInfo.ReconId}");

                if (string.IsNullOrEmpty(offlineReconInfo.ReconId))
                {

                    _logger.LogError($"[JobViewer]Get Recon Task Id is empty:{offlineReconInfo.ReconId}");
                    mutex.ReleaseMutex();
                    return;
                }
                
                OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == offlineReconInfo.ReconId);

                if (null != offlineReconModel)
                {
                    // if exist, update info
                    SetReconTaskInfo(offlineReconInfo, offlineReconModel);
                   
                } else
                {
                    offlineReconModel = new OfflineReconModel();
                    
                    SetReconTaskInfo(offlineReconInfo, offlineReconModel);
                    //offlineReconModel.StartEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Start);
                    //offlineReconModel.CancelEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Cancel);
                    //offlineReconModel.DeleteEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Delete);
                    //offlineReconModel.UpgradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Upgrade);
                    //offlineReconModel.DowngradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Downgrade);

                    UIInvoke(() =>
                    {
                        OfflineReconInfoModels.Insert(0, offlineReconModel);
                    });
                }
                mutex.ReleaseMutex();
            }
        }

        private void SetReconTaskInfo(OfflineTaskInfo offlineReconInfo, OfflineReconModel offlineReconModel)
        {
            //根据scanId和reconId 从数据库读取
            var pe = _studyService.GetWithUID(offlineReconInfo.StudyUID);
            var patientModel = pe.Patient;

            offlineReconModel.PatientId = patientModel?.PatientId;
            offlineReconModel.PatientName = patientModel?.PatientName;

            offlineReconModel.MachineName = offlineReconInfo.MachineName;
            offlineReconModel.Index = offlineReconInfo.Index;
            offlineReconModel.ScanId = offlineReconInfo.ScanId;
            offlineReconModel.ReconId = offlineReconInfo.ReconId;
            offlineReconModel.IsOver = offlineReconInfo.IsOver;
            offlineReconModel.Status = offlineReconInfo.Status;
            offlineReconModel.Progress = offlineReconInfo.Progress;
            offlineReconModel.ShowStatus = offlineReconInfo.Progress < 1 ? OfflineReconStatus.Created.ToString() : OfflineReconStatus.Finished.ToString();
            offlineReconModel.SeriesDescription = !string.IsNullOrEmpty(offlineReconInfo.SeriesDescription) ? offlineReconInfo.SeriesDescription : "";
            offlineReconModel.SeriesUID = offlineReconInfo.SeriesUID;
            offlineReconModel.StudyUID = offlineReconInfo.StudyUID;
            offlineReconModel.ReconTaskDateTime = offlineReconInfo.ReconTaskDateTime;
        }

        private void RunReconTaskStarted(OfflineTaskInfo offlineReconInfo)
        {
            string reconId = offlineReconInfo.ReconId;
            float progress = offlineReconInfo.Progress;
            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId && o.ScanId == offlineReconInfo.ScanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                offlineReconModel.Status = OfflineTaskStatus.Executing;
                var pe = _studyService.GetWithUID(offlineReconInfo.StudyUID);
                var patientModel = pe.Patient;

                offlineReconModel.PatientId = patientModel?.PatientId;
                offlineReconModel.PatientName = patientModel?.PatientName;
                offlineReconModel.SeriesUID = offlineReconInfo.SeriesUID;
                offlineReconModel.SeriesDescription = offlineReconInfo.SeriesDescription;
                offlineReconModel.ReconTaskDateTime = offlineReconInfo.ReconTaskDateTime.Year > 1 ? offlineReconInfo.ReconTaskDateTime : DateTime.Now;
            }
        }

        private void ReconTaskStartCommand(object pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
                OfflineCommandResult offlineCommandResult = _offlineService.StartReconTask(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId);
                if (null != offlineCommandResult)
                {
                    if (offlineCommandResult.Status != CommandExecutionStatus.Success)
                    {
                        MessageBox.Show("Start Failure!");
                    }
                }
            }
        }

        private void RunReconTaskCancelById(string reconId, string scanId)
        {
            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId && o.ScanId == scanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                offlineReconModel.Status = OfflineTaskStatus.Cancelled;
                offlineReconModel.Progress = 0;
                offlineReconModel.ShowStatus = OfflineTaskStatus.Cancelled.ToString();
                offlineReconModel.StartEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Start);
                offlineReconModel.CancelEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Cancel);
                offlineReconModel.DeleteEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Delete);
                offlineReconModel.UpgradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Upgrade);
                offlineReconModel.DowngradeEnabled = GetButtonShowEnabled(offlineReconModel.Status, OfflineReconFunctionType.Downgrade);
            }
        }
        
        private void ReconTaskCancelCommand(object pOfflineReconModel)
        {
            if (null != pOfflineReconModel)
            {
                OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
                OfflineCommandResult offlineCommandResult = _offlineService.CloseReconTask(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId);
                

                //if (offlineCommandResult != null)
                //{
                //    if (offlineCommandResult.Status == CTS.Enums.CommandExecutionStatus.Success)
                //    {
                RunReconTaskCancelById(offlineReconModel.ReconId, offlineReconModel.ScanId);
                //    }
                //}
            }
        }

        private void RunReconTaskIndex(OfflineTaskInfo offlineReconInfo, int taskIndex)
        {
            //Todo:
            string reconId = offlineReconInfo.ReconId;
            string scanId = offlineReconInfo.ScanId;

            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId && o.ScanId == scanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                _offlineService.SetReconTaskIndex(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, taskIndex);
            }
        }

        private void RunReconTaskIndexById(string scanId, string reconId, int taskIndex)
        {
            //Todo:
            if (!string.IsNullOrEmpty(scanId) && !string.IsNullOrEmpty(reconId))
            {
                OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId && o.ScanId == scanId);
                if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
                {
                    _offlineService.SetReconTaskIndex(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, taskIndex);
                }
            }
        }

        private void ReconTaskIndexCommand(object pOfflineReconModel)
        {
            OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
            _offlineService.SetReconTaskIndex(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, 1);
            //Todo:置顶的排到最上面，列表按照优先级和时间排序

        }

        private void RunReconTaskPriority(OfflineTaskInfo offlineReconInfo)
        {
            //Todo:
            string reconId = offlineReconInfo.ReconId;
            string scanId = offlineReconInfo.ScanId;

            OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId && o.ScanId == scanId);
            if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
            {
                _offlineService.SetReconTaskPriority(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, offlineReconModel.Priority);
            }
        }

        private void RunReconTaskPriorityById(string scanId, string reconId, TaskPriority taskPriority)
        {
            //Todo:
            if (!string.IsNullOrEmpty(scanId) && !string.IsNullOrEmpty(reconId))
            {
                OfflineReconModel offlineReconModel = OfflineReconInfoModels.FirstOrDefault(o => o.ReconId == reconId && o.ScanId == scanId);
                if (null != offlineReconModel && !string.IsNullOrEmpty(offlineReconModel.ReconId))
                {
                    _offlineService.SetReconTaskPriority(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, offlineReconModel.Priority);
                }
            }
        }
        
        private void ReconTaskPriorityCommand(object pOfflineReconModel)
        {
            OfflineReconModel offlineReconModel = pOfflineReconModel as OfflineReconModel;
            _offlineService.SetReconTaskPriority(offlineReconModel.StudyUID, offlineReconModel.ScanId, offlineReconModel.ReconId, offlineReconModel.Priority);
            //Todo:置顶的排到最上面，列表按照优先级和时间排序

        }

        private void RunReconTaskDone(OfflineTaskInfo offlineReconInfo, bool hasError = false)
        {
            var findItem = OfflineReconInfoModels.FirstOrDefault(n => n.ReconId == offlineReconInfo.ReconId && n.ScanId == offlineReconInfo.ScanId);

            if (null != findItem && !string.IsNullOrEmpty(findItem.ReconId))
            {
                findItem.Status = offlineReconInfo.Status;
                findItem.Progress = 1;
                findItem.ShowStatus = hasError ? OfflineTaskStatus.Error.ToString() : OfflineTaskStatus.Finished.ToString();
                //PatientEntity patienModel = _patientService.Get(offlineReconInfo.PatientId);
                var pe = _studyService.GetWithUID(offlineReconInfo.StudyUID);
                var patientModel = pe.Patient;
                findItem.PatientId = patientModel?.PatientId;
                findItem.PatientName = patientModel?.PatientName;

                findItem.SeriesUID = offlineReconInfo.SeriesUID;
                if (!string.IsNullOrEmpty(offlineReconInfo.SeriesDescription))
                {
                    findItem.SeriesDescription = offlineReconInfo.SeriesDescription;
                }

                findItem.ReconTaskDateTime = DateTime.Now;

                findItem.StartEnabled = GetButtonShowEnabled(findItem.Status, OfflineReconFunctionType.Start);
                findItem.CancelEnabled = GetButtonShowEnabled(findItem.Status, OfflineReconFunctionType.Cancel);
                findItem.DeleteEnabled = GetButtonShowEnabled(findItem.Status, OfflineReconFunctionType.Delete);
                findItem.UpgradeEnabled = GetButtonShowEnabled(findItem.Status, OfflineReconFunctionType.Upgrade);
                findItem.DowngradeEnabled = GetButtonShowEnabled(findItem.Status, OfflineReconFunctionType.Downgrade);
                if (findItem.Status == OfflineTaskStatus.Finished)
                {
                    findItem.ShowOrange = false;
                    findItem.ShowBlue = true;
                }
            }
        }

        private bool GetButtonShowEnabled(OfflineTaskStatus status, OfflineReconFunctionType offlineReconFunctionType)
        {
            bool flag = false;
            switch (status)
            {
                case OfflineTaskStatus.Created:
                case OfflineTaskStatus.Waiting:
                    switch (offlineReconFunctionType)
                    {
                        case OfflineReconFunctionType.Cancel:
                            flag = false; break;
                        case OfflineReconFunctionType.Start:
                        case OfflineReconFunctionType.Delete:
                        case OfflineReconFunctionType.Upgrade:
                        case OfflineReconFunctionType.Downgrade:
                            flag = true; break;
                    }
                    break;
                case OfflineTaskStatus.Executing:
                    switch (offlineReconFunctionType)
                    {
                        case OfflineReconFunctionType.Start:
                            flag = false; break;
                        case OfflineReconFunctionType.Cancel:
                            flag = true; break;
                        case OfflineReconFunctionType.Delete:
                            flag = false; break;
                        case OfflineReconFunctionType.Upgrade:
                            flag = false; break;
                        case OfflineReconFunctionType.Downgrade:
                            flag = false; break;
                    }
                    break;
                case OfflineTaskStatus.Cancelled:
                case OfflineTaskStatus.Finished:
                    flag = false;
                    break;
            }
            return flag;
        }

        /// <summary>
        /// 
        /// </summary>
        public void LoadOfflineReconTask()
        {
            List<OfflineTaskInfo> offlineReconInfoList = _offlineService.GetReconAllTasks();
            if (null == offlineReconInfoList) return;
            _logger.LogInformation($"The current number of offline recon tasks:{offlineReconInfoList.Count.ToString()}");
            if (offlineReconInfoList.Count <= 0) return;

            var localOfflineReconInfos = new List<OfflineReconModel>();
            localOfflineReconInfos = offlineReconInfoList.Select(t => new OfflineReconModel
            {
                SeriesUID = t.SeriesUID,
                ReconId = t.ReconId,
                Status = t.Status,
                PatientName = t.PatientName,
                PatientId = t.PatientId,
                ScanId = t.ScanId,
                StudyUID = t.StudyUID,
                Progress = t.Status == OfflineTaskStatus.Finished ? 1 : t.Progress,
                ReconTaskDateTime = t.ReconTaskDateTime,
                SeriesDescription = t.SeriesDescription,
                ShowStatus = GetOfflineReconStatus(t.Status)
            }).ToList();
            //TODO:transform

            OfflineReconInfoModels = new ObservableCollection<OfflineReconModel>(localOfflineReconInfos);
        }
        /// <summary>
        /// 根据OfflineTaskStatus返回对应的离线重建任务状态字符串
        /// </summary>
        /// <param name="status">离线任务状态</param>
        /// <returns>离线任务状态字符串</returns>
        private static string GetOfflineReconStatus(OfflineTaskStatus status)
        {
            if (status == OfflineTaskStatus.Executing)
            {
                return "Reconing";
            } else
            {
                return status.ToString();
            }
        }
    }
}

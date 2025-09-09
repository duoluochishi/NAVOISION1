//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.Language;
using NV.CT.Print.ApplicationService.Contract.Interfaces;
using NV.CT.Print.Events;
using NV.CT.Print.Models;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Generic;

namespace NV.CT.Print.ViewModel
{
    public class MainPrintControlViewModel : BaseViewModel
    {
        private readonly ILogger<MainPrintControlViewModel> _logger;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IApplicationCommunicationService _applicationCommunicationService;
        private readonly IPrint _printService;
        private readonly IPrintApplicationService _printApplicationService;

        private bool _isOverviewMode = false;    
        public bool IsOverviewMode
        {
            get
            { 
                return _isOverviewMode;
            }
            set 
            {
                this.SetProperty(ref _isOverviewMode, value);
            }
        }

        private bool _isBrowserMode = true;
        public bool IsBrowserMode
        {
            get
            {
                return _isBrowserMode;
            }
            set
            {
                this.SetProperty(ref _isBrowserMode, value);
            }
        }

        public MainPrintControlViewModel(ILogger<MainPrintControlViewModel> logger,
                                         IMapper mapper,
                                         IDialogService dialogService, 
                                         IApplicationCommunicationService applicationCommunicationService,
                                         IPrint printService,
                                         IPrintApplicationService printApplicationService)
        {   
            _logger = logger;
            _mapper = mapper;
            _dialogService = dialogService;
            _applicationCommunicationService = applicationCommunicationService;
            _printService = printService;
            _printApplicationService = printApplicationService;

            EventAggregator.Instance.GetEvent<DisplayModeChangedEvent>().Subscribe(OnDisplayModeChanged);
            _applicationCommunicationService.NotifyApplicationClosing += OnNotifyApplicationClosing;
            _printService.StudyChanged += OnStudyChanged;
        }

        private void OnStudyChanged(object? sender, string newStudyId)
        {
            _logger.LogInformation( $"[Print] OnStudyChanged with newStudyId : {newStudyId}");

            if (string.IsNullOrWhiteSpace(newStudyId))
            {
                _logger.LogWarning("[Print] new studyId in OnStudyChanged is empty.");
                return;
            }

            LoadStudyDataById(newStudyId);
        }

        public void LoadStudyDataById(string studyId)
        {
            _logger?.LogDebug($"start LoadStudyDataById for studyId :{studyId}");
            //reload study data
            var result = _printApplicationService.Get(studyId);
            var newPrintingStudyModel = _mapper.Map<PrintingStudyModel>(result.Study);
            newPrintingStudyModel.PrintingPatientModel = _mapper.Map<PrintingPatientModel>(result.Patient);

            var seriesList = _printApplicationService.GetSeriesByStudyId(studyId);
            newPrintingStudyModel.PrintingSeriesModelList = _mapper.Map<List<ApplicationService.Contract.Models.SeriesModel>,
                                                                        List<PrintingSeriesModel>>(seriesList);

            _logger?.LogDebug($"finish LoadStudyDataById for PatientName :{newPrintingStudyModel.PrintingPatientModel.PatientName}");

            Global.Instance.PrintingStudy = newPrintingStudyModel;

            //Notify all components
            EventAggregator.Instance.GetEvent<SelectedStudyChangedEvent>().Publish(Global.Instance.PrintingStudy);
            //EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Publish(Global.Instance.PrintingStudy.PrintingSeriesModelList.FirstOrDefault());

        }

        private void OnNotifyApplicationClosing(object? sender, ApplicationResponse e)
        {
            if (e.ApplicationName != ApplicationParameterNames.APPLICATIONNAME_PRINT) 
            {
                return;
            }
            if (Process.GetCurrentProcess().Id != e.ProcessId)
            {
                return;
            }

            if (e.NeedConfirm)
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    string confirmMessage = string.Format(LanguageResource.Message_Confirm_CloseApplication,
                        e.ApplicationName);
                    _dialogService.ShowDialog(true, MessageLeveles.Info,
                        LanguageResource.Message_Info_CloseConfirmTitle,
                        confirmMessage, arg =>
                        {
                            if (arg.Result == ButtonResult.OK)
                            {
                                _printService.ClosePrint();
                                Process.GetCurrentProcess().Kill();
                            }
                        }, ConsoleSystemHelper.WindowHwnd);
                });
            }
            else
            {
                _printService.ClosePrint();
                Process.GetCurrentProcess().Kill();
			}
        }

        private void OnDisplayModeChanged(PrintDisplayMode printDisplayMode)
        {
            IsOverviewMode = printDisplayMode == PrintDisplayMode.Overview;
            IsBrowserMode = printDisplayMode == PrintDisplayMode.Browser;
        }

    }

}

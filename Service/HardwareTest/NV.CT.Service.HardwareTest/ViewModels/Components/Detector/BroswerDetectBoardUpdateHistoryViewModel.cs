using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.Detector
{
    public partial class BroswerDetectBoardUpdateHistoryViewModel : ObservableObject, IDialogAware
    {
        private readonly ILogService logService;

        public BroswerDetectBoardUpdateHistoryViewModel(ILogService logService)
        {
            //Get from DI
            this.logService = logService;
            //Initialize
            InitializeProperties();       
        }

        #region Initialize

        private void InitializeProperties()
        {
            DetectBoardSourceUpdateHistory = new();
        }

        #endregion

        #region Properties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        private uint detectorModuleID;
        [ObservableProperty]
        public List<DetectBoardDto> detectBoardSourceUpdateHistory = null!;

        public string Title => $"[DetectBoardSlot-{DetectorModuleID.ToString("00")}] Detector Board Update History";

        #endregion

        #region DialogAware

        public void EnterDialog(object[] parameters)
        {
            //解析detectorIndex和boardIndex
            _ = uint.TryParse(parameters[0].ToString(), out uint detectorModuleID);
            //更新
            DetectorModuleID = detectorModuleID;
            //获取历史记录
            var detectBoardSources = (IEnumerable<DetectBoardDto>)parameters[1];
            //获取更新历史
            DetectBoardSourceUpdateHistory = new(detectBoardSources);
        }

        #endregion

    }
}

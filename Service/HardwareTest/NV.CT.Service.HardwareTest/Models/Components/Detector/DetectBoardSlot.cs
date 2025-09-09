using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    /// <summary>
    /// 检出板槽位
    /// </summary>
    public partial class DetectBoardSlot : ObservableObject
    {
        //对应的探测器单元ID(1-64)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Name))]
        private uint detectorModuleID;
        //安装在槽位上的检出板(可为空，即未安装)
        [ObservableProperty]
        private DetectBoardDto? installedDetectBoard;

        public string Name => $"DetectBoardSlot-{DetectorModuleID.ToString("00")}";

        /// <summary>
        /// 替换检出板
        /// </summary>
        public void ExchangeBoard(DetectBoardDto newDetectBoard) 
        {
            if (InstalledDetectBoard is not null)
            {
                InstalledDetectBoard.Retire();
            }

            InstalledDetectBoard = newDetectBoard;
        }

    }
}

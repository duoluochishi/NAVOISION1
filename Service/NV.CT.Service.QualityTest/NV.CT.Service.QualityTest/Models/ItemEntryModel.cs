using System.Text.Json.Serialization;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.Models.ScanReconModels;
using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models.ItemEntryParam;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.QualityTest.Models
{
    public class ItemEntryModel : ViewModelBase
    {
        #region Field

        private ItemEntryParamBaseModel _param = null!;
        private bool _isChecked;
        private StatusType _status;
        private double _progress;
        private bool? _result;

        #endregion

        public int ID { get; set; }
        public string ScanUID { get; set; } = string.Empty;
        public string OfflineReconTaskID { get; set; } = string.Empty;
        public string OfflineReconImageFolder { get; set; } = string.Empty;
        public bool IsOfflineReconSucceed { get; set; }

        [JsonIgnore]
        public ScanReconParamModel ScanReconParamDto { get; set; } = default!;

        [JsonIgnore]
        public ScanReconParamModel OfflineReconParamDto { get; set; } = default!;

        [JsonIgnore]
        public ItemModel Parent { get; set; } = default!;

        public ItemEntryParamBaseModel Param
        {
            get => _param;
            set => SetProperty(ref _param, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public StatusType Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public bool? Result
        {
            get => _result;
            set => SetProperty(ref _result, value);
        }

        public void SetScanAndReconParam(TablePosition tablePosition)
        {
            Clear();
            Param.ScanParam.ReconVolumeStartPosition = ((double)(tablePosition.Horizontal.Millimeter2Micron() - SystemConfig.LaserConfig.Laser.Offset.Value)).Micron2Millimeter();
            Param.ScanParam.TableHeight = tablePosition.Vertical;
            Param.ScanParam.Update();
            ScanUID = Param.ScanParam.ScanUID;
            ScanReconParamDto.Study?.Update();
            ScanReconParamDto.ScanParameter = Param.ScanParam;
            OfflineReconParamDto.ScanParameter = Param.ScanParam;

            if (Param.RealTimeReconParam != null)
            {
                Param.RealTimeReconParam.Update(Param.ScanParam);
                ScanReconParamDto.ReconSeriesParams = [Param.RealTimeReconParam];
            }

            if (Param.OfflineReconParam != null)
            {
                Param.OfflineReconParam.Update(Param.ScanParam);
                OfflineReconParamDto.ReconSeriesParams = [Param.OfflineReconParam];
            }
        }

        public bool Validate()
        {
            Result = Param.Validate();
            return Result.Value;
        }

        public void Clear()
        {
            ScanUID = string.Empty;
            OfflineReconTaskID = string.Empty;
            OfflineReconImageFolder = string.Empty;
            IsOfflineReconSucceed = false;
            Status = StatusType.None;
            Progress = 0;
            Result = null;
            Param.Clear();
        }
    }
}
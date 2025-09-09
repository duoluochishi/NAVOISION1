using NV.CT.Service.Common.Framework;
using NV.CT.Service.Common.Models.ScanReconModels;

namespace NV.CT.Service.QualityTest.Models.ItemEntryParam
{
    public abstract class ItemEntryParamBaseModel : ViewModelBase
    {
        private string _name = string.Empty;
        private ScanParamModel _scanParam = new();
        private ReconSeriesParamModel? _realTimeReconParam;
        private ReconSeriesParamModel? _offlineReconParam;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ScanParamModel ScanParam
        {
            get => _scanParam;
            set => SetProperty(ref _scanParam, value);
        }

        public ReconSeriesParamModel? RealTimeReconParam
        {
            get => _realTimeReconParam;
            set => SetProperty(ref _realTimeReconParam, value);
        }

        public ReconSeriesParamModel? OfflineReconParam
        {
            get => _offlineReconParam;
            set => SetProperty(ref _offlineReconParam, value);
        }

        public abstract bool Validate();
        public abstract void Clear();
    }
}
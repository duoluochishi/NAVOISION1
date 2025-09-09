using System.Collections.ObjectModel;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Framework;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="ScanReconParam"/>
    public class ScanReconParamModel : ViewModelBase
    {
        private PatientModel? _patient;
        private StudyModel? _study;
        private ScanParamModel _scanParameter = new();
        private ObservableCollection<ReconSeriesParamModel> _reconSeriesParams = [];

        /// <inheritdoc cref="ScanReconParam.Patient"/>
        public PatientModel? Patient
        {
            get => _patient;
            set => SetProperty(ref _patient, value);
        }

        /// <inheritdoc cref="ScanReconParam.Study"/>
        public StudyModel? Study
        {
            get => _study;
            set => SetProperty(ref _study, value);
        }

        /// <inheritdoc cref="ScanReconParam.ScanParameter"/>
        public ScanParamModel ScanParameter
        {
            get => _scanParameter;
            set => SetProperty(ref _scanParameter, value);
        }

        /// <inheritdoc cref="ScanReconParam.ReconSeriesParams"/>
        public ObservableCollection<ReconSeriesParamModel> ReconSeriesParams
        {
            get => _reconSeriesParams;
            set => SetProperty(ref _reconSeriesParams, value);
        }

        public ScanReconParam Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<ScanReconParam>(this);
        }
    }
}
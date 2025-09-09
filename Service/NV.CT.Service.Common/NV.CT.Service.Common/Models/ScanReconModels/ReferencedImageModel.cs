using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Framework;
using NV.CT.ServiceFramework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="ReferencedImage"/>
    public class ReferencedImageModel : ViewModelBase
    {
        private string _referencedSOPClassUID = string.Empty;
        private string _referencedSOPInstanceUID = string.Empty;

        /// <inheritdoc cref="ReferencedImage.ReferencedSOPClassUID"/>
        public string ReferencedSOPClassUID
        {
            get => _referencedSOPClassUID;
            set => SetProperty(ref _referencedSOPClassUID, value);
        }

        /// <inheritdoc cref="ReferencedImage.ReferencedSOPInstanceUID"/>
        public string ReferencedSOPInstanceUID
        {
            get => _referencedSOPInstanceUID;
            set => SetProperty(ref _referencedSOPInstanceUID, value);
        }

        public ReferencedImage Converter()
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<ReferencedImage>(this);
        }
    }
}
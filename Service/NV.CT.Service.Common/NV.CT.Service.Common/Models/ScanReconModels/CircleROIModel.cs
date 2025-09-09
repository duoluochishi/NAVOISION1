using NV.CT.FacadeProxy.Common.Models.ScanRecon;
using NV.CT.Service.Common.Framework;

namespace NV.CT.Service.Common.Models.ScanReconModels
{
    /// <inheritdoc cref="CircleROI"/>
    public class CircleROIModel : ViewModelBase
    {
        private int _centerX;
        private int _centerY;
        private int _radius;

        /// <inheritdoc cref="CircleROI.CenterX"/>
        public int CenterX
        {
            get => _centerX;
            set => SetProperty(ref _centerX, value);
        }

        /// <inheritdoc cref="CircleROI.CenterY"/>
        public int CenterY
        {
            get => _centerY;
            set => SetProperty(ref _centerY, value);
        }

        /// <inheritdoc cref="CircleROI.Radius"/>
        public int Radius
        {
            get => _radius;
            set => SetProperty(ref _radius, value);
        }
    }
}
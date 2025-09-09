using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients
{
    public class SourceAndGantryBaseModel : ObservableObject
    {
        private uint _sourceId;
        private double _gantryAngle;
        public bool IsChanged { get; private set; }

        /// <summary>
        /// 球管编号
        /// </summary>
        public uint SourceId
        {
            get => _sourceId;
            set
            {
                if (SetProperty(ref _sourceId, value))
                {
                    IsChanged = true;
                }
            }
        }

        /// <summary>
        /// 机架角度
        /// </summary>
        public double GantryAngle
        {
            get => _gantryAngle;
            set
            {
                if (SetProperty(ref _gantryAngle, value))
                {
                    IsChanged = true;
                }
            }
        }

        public void ResetChanged()
        {
            IsChanged = false;
        }
    }
}
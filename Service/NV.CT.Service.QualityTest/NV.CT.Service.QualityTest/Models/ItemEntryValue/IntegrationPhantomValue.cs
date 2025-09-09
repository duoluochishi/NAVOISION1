using NV.CT.Service.Common.Framework;
using NV.CT.Service.QualityTest.Alg.Models;

namespace NV.CT.Service.QualityTest.Models.ItemEntryValue
{
    public class IntegrationPhantomValue : ViewModelBase
    {
        #region Field

        private string _errorMsg = string.Empty;
        private int _position;

        #endregion

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        public Point3D[] Balls { get; private set; } = new Point3D[4];

        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public string ErrorMsg
        {
            get => _errorMsg;
            set => SetProperty(ref _errorMsg, value);
        }

        public void Clear()
        {
            ImageWidth = default;
            ImageHeight = default;
            Balls = new Point3D[4];
            ErrorMsg = string.Empty;
        }
    }
}
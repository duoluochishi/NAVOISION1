using NV.CT.UI.ViewModel;

namespace NV.CT.ProtocolManagement.ViewModels.Models
{
    public class ReconParameter : BaseViewModel
    {
        public ReconParameter()
        {

        }

        private string _seriesDescription = string.Empty;

        public string SeriesDescription
        {
            get => _seriesDescription;
            set => SetProperty(ref _seriesDescription, value);
        }
        private string _funcMode;//0
        public string FuncMode
        {
            get => _funcMode;
            set => SetProperty(ref _funcMode, value);
        }

        private string _airCorrectionMode;//0
        public string AirCorrectionMode
        {
            get => _airCorrectionMode;
            set => SetProperty(ref _airCorrectionMode, value);
        }

        private string _reconType;//1

        public string ReconType
        {
            get => _reconType;
            set => SetProperty(ref _reconType, value);
        }
        private string _voxelNumX = 0.ToString();//D
        public string VoxelNumX
        {
            get => _voxelNumX;
            set => SetProperty(ref _voxelNumX, value);
        }
        private string _voxelNumY = 0.ToString();//D
        public string VoxelNumY
        {
            get => _voxelNumY;
            set => SetProperty(ref _voxelNumY, value);
        }
        private string _voxelNumZ = 0.ToString();//D
        public string VoxelNumZ
        {
            get => _voxelNumZ;
            set => SetProperty(ref _voxelNumZ, value);
        }
        private string _voxelSizeX = 0.ToString();//D
        public string VoxelSizeX
        {
            get => _voxelSizeX;
            set => SetProperty(ref _voxelSizeX, value);
        }
        private string _voxelSizeY = 0.ToString();//D
        public string VoxelSizeY
        {
            get => _voxelSizeY;
            set => SetProperty(ref _voxelSizeY, value);
        }
        private string _voxelSizeZ = 0.ToString();//D
        public string VoxelSizeZ
        {
            get => _voxelSizeZ;
            set => SetProperty(ref _voxelSizeZ, value);
        }
        private string _centerX = 0.ToString();//2
        public string CenterX
        {
            get => _centerX;
            set => SetProperty(ref _centerX, value);
        }
        private string _centerY = 0.ToString();//3
        public string CenterY
        {
            get => _centerY;
            set => SetProperty(ref _centerY, value);
        }
        private string _filterType;//4
        public string FilterType
        {
            get => _filterType;
            set => SetProperty(ref _filterType, value);
        }
        private string _binningXY;//5
        public string Binning
        {
            get => _binningXY;
            set => SetProperty(ref _binningXY, value);
        }
        private string _postDenoiseCoef = 0.ToString();//6
        public string PostDenoiseCoef
        {
            get => _postDenoiseCoef;
            set => SetProperty(ref _postDenoiseCoef, value);
        }
        private string _boneAritifacEnable;//7
        public string BoneAritifacEnable
        {
            get => _boneAritifacEnable;
            set => SetProperty(ref _boneAritifacEnable, value);
        }
        private string _interpType;//8
        public string InterpType
        {
            get => _interpType;
            set => SetProperty(ref _interpType, value);
        }

        private string _reconFOV = 0.ToString();//9
        public string ReconFOV
        {
            get => _reconFOV;
            set => SetProperty(ref _reconFOV, value);
        }

        private string _metalAritifactEnable;//10
        public string MetalAritifactEnable
        {
            get => _metalAritifactEnable;
            set => SetProperty(ref _metalAritifactEnable, value);
        }

        private string _sliceThickness = 0.ToString();//11
        public string SliceThickness
        {
            get => _sliceThickness;
            set => SetProperty(ref _sliceThickness, value);
        }
        private string _preDenoiseCoef = 0.ToString();//12
        public string PreDenoiseCoef
        {
            get => _preDenoiseCoef;
            set => SetProperty(ref _preDenoiseCoef, value);
        }
        private string _bowtie;//13
        public string Bowtie
        {
            get => _bowtie;
            set => SetProperty(ref _bowtie, value);
        }
        private string _windowType;

        public string WindowType
        {
            get => _windowType;
            set => SetProperty(ref _windowType, value);
        }
        private string _windowCenter = 0.ToString();//14
        public string WindowCenter
        {
            get => _windowCenter;
            set => SetProperty(ref _windowCenter, value);
        }

        private string _windowWidth = 0.ToString();//15
        public string WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        //private string _centerFirstX;
        //private string _centerFirstY;
        //private string _centerFirstZ;
        //private string _centerLastX;
        //private string _centerLastY;
        //private string _centerLastZ;
        //private string _foVDirectionHorX;
        //private string _foVDirectionHorY;
        //private string _foVDirectionHorZ;
        //private string _foVDirectionVertX;
        //private string _foVDirectionVertY;
        //private string _foVDirectionVertZ;
        private string _imageMatrixHor;
        public string ImageMatrixHor
        {
            get => _imageMatrixHor;
            set => SetProperty(ref _imageMatrixHor, value);
        }
        private string _imageMatrixVert;
        public string ImageMatrixVert
        {
            get => _imageMatrixVert;
            set => SetProperty(ref _imageMatrixVert, value);
        }
        private string _imageIncrement = string.Empty;
        public string ImageIncrement
        {
            get => _imageIncrement;
            set => SetProperty(ref _imageIncrement, value);
        }
        private string _imageOrder = string.Empty;

        public string ImageOrder
        {
            get => _imageOrder;
            set => SetProperty(ref _imageOrder, value);
        }
        private string _isRTD = false.ToString();//
        public string IsRTD
        {
            get => _isRTD;
            set => SetProperty(ref _isRTD, value);
        }
        private string _isRingAritifactEnable = false.ToString();//
        public string IsRingAritifactEnable
        {
            get => _isRingAritifactEnable;
            set => SetProperty(ref _isRingAritifactEnable, value);
        }
        private string _isSmoothZEnable = false.ToString();//
        public string IsSmoothZEnable
        {
            get => _isSmoothZEnable;
            set => SetProperty(ref _isSmoothZEnable, value);
        }
        private string _isTwoPassEnable = false.ToString();//
        public string IsTwoPassEnable
        {
            get => _isTwoPassEnable;
            set => SetProperty(ref _isTwoPassEnable, value);
        }
        private string _ivrTVCoef = 0.ToString();//
        public string IVRTVCoef
        {
            get => _ivrTVCoef;
            set => SetProperty(ref _ivrTVCoef, value);
        }
        private string _ringCorrectCoef = 0.ToString();//
        public string RingCorrectCoef
        {
            get => _ringCorrectCoef;
            set => SetProperty(ref _ringCorrectCoef, value);
        }
        private string _preDenoiseType;

        public string PreDenoiseType
        {
            get => _preDenoiseType;
            set => SetProperty(ref _preDenoiseType, value);
        }
        private string _postDenoiseType;
        public string PostDenoiseType
        {
            get => _postDenoiseType;
            set => SetProperty(ref _postDenoiseType, value);
        }
        private string _reconBodyPart;
        public string ReconBodyPart
        {
            get => _reconBodyPart;
            set => SetProperty(ref _reconBodyPart, value);
        }
        private bool _isHDRecon;
        public bool IsHDRecon
        {
            get => _isHDRecon;
            set => SetProperty(ref _isHDRecon, value);
        }
        private bool _isWindMillEnable;
        public bool IsWindMillEanble
        {
            get => _isWindMillEnable;
            set => SetProperty(ref _isWindMillEnable, value);
        }
        private bool _isConeAngleEnable;
        public bool IsConeAngleEanble
        {
            get => _isConeAngleEnable;
            set => SetProperty(ref _isConeAngleEnable, value);
        }
        private bool _isAutoRecon;
        public bool IsAutoRecon
        {
            get => _isAutoRecon;
            set => SetProperty(ref _isAutoRecon, value);
        }
    }
}
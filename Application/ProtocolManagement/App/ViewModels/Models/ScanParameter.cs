using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.ProtocolManagement.ViewModels.Common;
using NV.CT.ProtocolManagement.ViewModels.Enums;
using NV.CT.UI.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EnumConverter = NV.CT.ProtocolManagement.ViewModels.Common.EnumConverter;

namespace NV.CT.ProtocolManagement.ViewModels.Models
{
    public class ScanParameter : BaseViewModel
    {
        private Dictionary<string, EnumViewModel<KilovoltValue>> _kvs = new();
        private Dictionary<string, EnumViewModel<ScanOption>> _scanTaskOption;
        private string _pitch = string.Empty;

        public Dictionary<string, EnumViewModel<KilovoltValue>> KVs
        {
            get => _kvs;
            set => SetProperty(ref _kvs, value);
        }
        public Dictionary<string, EnumViewModel<ScanOption>> ScanTaskOption
        {
            get => _scanTaskOption;
            set => SetProperty(ref _scanTaskOption, value);
        }

        private string _scanName = string.Empty;

        private string _binning = string.Empty;

        private string _voiceId = 0.ToString();

        private string _scanUID = string.Empty;

        private string _autoScan = false.ToString();

        private string _isEnhanced = 0.ToString();

        private string _scanNumber = 0.ToString();

        private string _mA = string.Empty;
        public string MA
        {
            get => _mA;
            set => SetProperty(ref _mA, value);
        }

        private string _kV = string.Empty;

        public string KV
        {
            get => _kV;
            set => SetProperty(ref _kV, value);
        }

        //private string _exposureTime;
        private string _frameTime = 0.ToString();
        private string _scanOption = string.Empty;
        public string ScanOption
        {
            get => _scanOption;
            set => SetProperty(ref _scanOption, value);
        }
        private string _scanMode;
        private string _scanLength = 0.ToString();
        private string _scanPositionEnd = 0.ToString();
        private string _scanPositionStart = 0.ToString();
        private string _tableHeight = 0.ToString();
        private string _bodyPart;
        private string _exposureDelayTime = 0.ToString();
        private string _preVoiceDelayTime = 0.ToString();
        private string _framesPerCycle = 0.ToString();


        public string Pitch
        {
            get => _pitch;
            set => SetProperty(ref _pitch, value);
        }

        public string ScanName
        {
            get => _scanName;
            set => SetProperty(ref _scanName, value);
        }

        public string Binning
        {
            get => _binning;
            set => SetProperty(ref _binning, value);
        }
        public string VoiceId
        {
            get => _voiceId;
            set => SetProperty(ref _voiceId, value);
        }

        private string _postVoiceId = 0.ToString();
        public string PostVoiceId
        {
            get => _postVoiceId;
            set => SetProperty(ref _postVoiceId, value);
        }

        public string ScanUID
        {
            get => _scanUID;
            set => SetProperty(ref _scanUID, value);
        }
        public string AutoScan
        {
            get => _autoScan;
            set => SetProperty(ref _autoScan, value);
        }
        public string IsEnhanced
        {
            get => _isEnhanced;
            set => SetProperty(ref _isEnhanced, value);
        }
        public string ScanNumber
        {
            get => _scanNumber;
            set => SetProperty(ref _scanNumber, value);
        }

        public string FrameTime
        {
            get => _frameTime;
            set => SetProperty(ref _frameTime, value);
        }
        public string ScanMode
        {
            get => _scanMode;
            set => SetProperty(ref _scanMode, value);
        }
        public string ScanLength
        {
            get => _scanLength;
            set => SetProperty(ref _scanLength, value);
        }
        public string ScanPositionEnd
        {
            get => _scanPositionEnd;
            set => SetProperty(ref _scanPositionEnd, value);
        }
        public string ScanPositionStart
        {
            get => _scanPositionStart;
            set => SetProperty(ref _scanPositionStart, value);
        }
        public string TableHeight
        {
            get => _tableHeight;
            set => SetProperty(ref _tableHeight, value);
        }
        public string BodyPart
        {
            get => _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }

        public string ExposureDelayTime
        {
            get => _exposureDelayTime;
            set => SetProperty(ref _exposureDelayTime, value);
        }

        public string PreVoiceDelayTime
        {
            get => _preVoiceDelayTime;
            set => SetProperty(ref _preVoiceDelayTime, value);
        }

        private bool _isvoicesupported = false;

        public bool IsVoiceSupported
        {
            get => _isvoicesupported; 
            set => SetProperty(ref _isvoicesupported, value);
        }


        public string FramesPerCycle
        {
            get => _framesPerCycle;
            set => SetProperty(ref _framesPerCycle, value);
        }

        private string _exposureTime = string.Empty;
        public string ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }

        private string _rawDataType = string.Empty;
        public string RawDataType
        {
            get => _rawDataType;
            set => SetProperty(ref _rawDataType, value);
        }

        private string _exposureMode = string.Empty;
        public string ExposureMode
        {
            get => _exposureMode;
            set => SetProperty(ref _exposureMode, value);
        }

        private string _tableDirection = string.Empty;
        public string TableDirection
        {
            get => _tableDirection;
            set => SetProperty(ref _tableDirection, value);
        }
        private string _collimitorZ = string.Empty;
        public string CollimitorZ
        {
            get => _collimitorZ;
            set => SetProperty(ref _collimitorZ, value);
        }

        private bool _isBowtieEnabled = false;
        public bool IsBowtieEnabled
        {
            get => _isBowtieEnabled;
            set => SetProperty(ref _isBowtieEnabled, value);
        }

        private string _collimitorSliceWidth = string.Empty;
        public string CollimitorSliceWidth
        {
            get => _collimitorSliceWidth;
            set => SetProperty(ref _collimitorSliceWidth, value);
        }

        private string _gain = string.Empty;
        public string Gain
        {
            get => _gain;
            set => SetProperty(ref _gain, value);
        }

        private string _tableFeed = string.Empty;
        public string TableFeed
        {
            get => _tableFeed;
            set => SetProperty(ref _tableFeed, value);
        }

        private string _tableAcceleration = string.Empty;
        public string TableAcceleration
        {
            get => _tableAcceleration;
            set => SetProperty(ref _tableAcceleration, value);
        }

        private string _xRayFocus = string.Empty;
        public string XRayFocus
        {
            get => _xRayFocus;
            set => SetProperty(ref _xRayFocus, value);
        }

        private string _gantryAcceleration = string.Empty;
        public string GantryAcceleration
        {
            get => _gantryAcceleration;
            set => SetProperty(ref _gantryAcceleration, value);
        }

        private string _preOffsetFrames = string.Empty;
        public string PreOffsetFrames
        {
            get => _preOffsetFrames;
            set => SetProperty(ref _preOffsetFrames, value);
        }

        private string _postOffsetFrames = string.Empty;
        public string PostOffsetFrames
        {
            get => _postOffsetFrames;
            set => SetProperty(ref _postOffsetFrames, value);
        }

        private string _autoDeleteNum = string.Empty;
        public string AutoDeleteNum
        {
            get => _autoDeleteNum;
            set => SetProperty(ref _autoDeleteNum, value);
        }

        private string _scanFOV = string.Empty;
        public string ScanFOV
        {
            get => _scanFOV;
            set => SetProperty(ref _scanFOV, value);
        }

        private string _tubePositions0 = string.Empty;
        public string TubePositions0
        {
            get => _tubePositions0;
            set => SetProperty(ref _tubePositions0, value);
        }

        private string _tubePositions1 = string.Empty;
        public string TubePositions1
        {
            get => _tubePositions1;
            set => SetProperty(ref _tubePositions1, value);
        }

        private string _loops = string.Empty;
        public string Loops
        {
            get => _loops;
            set => SetProperty(ref _loops, value);
        }

        private string _loopTime = string.Empty;
        public string LoopTime
        {
            get => _loopTime;
            set => SetProperty(ref _loopTime, value);
        }
        private string _objectFov=string.Empty;

        public string ObjectFov
        {
            get => _objectFov;
            set => SetProperty(ref _objectFov, value); 
        }
        private string _postDeleteLength = string.Empty;

        public string PostDeleteLength
        {
            get => _postDeleteLength;
            set => SetProperty(ref _postDeleteLength, value);
        }
        private string _doseNotification_CTDI;

        public string DoseNotification_CTDI
        {
            get => _doseNotification_CTDI;
            set => SetProperty(ref _doseNotification_CTDI, value);
        }

        private string _doseNotification_DLP;

        public string DoseNotification_DLP
        {
            get => _doseNotification_DLP;
            set => SetProperty(ref _doseNotification_DLP, value);
        }

        private string _allowErrorTubeCount;

        public string AllowErrorTubeCount
        {
            get => _allowErrorTubeCount;
            set => SetProperty(ref _allowErrorTubeCount, value);
        }
        private string _exposureIntervalTime;

        public string ExposureIntervalTime
        {
            get => _exposureIntervalTime;
            set => SetProperty(ref _exposureIntervalTime, value);
        }
        private string _activeExposureSourceCount;

        public string ActiveExposureSourceCount
        {
            get => _activeExposureSourceCount;
            set => SetProperty(ref _activeExposureSourceCount, value);
        }
        private string _collimatorOpenMode;

        public string CollimatorOpenMode
        {
            get => _collimatorOpenMode;
            set => SetProperty(ref _collimatorOpenMode, value);
        }
        public ScanParameter()
        {
            ScanTaskOption = EnumConverter.ToDic<ScanOption>();
        }
    }
}
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Service.TubeHistory.Models
{
    public class TubeDetailModel : ObservableObject
    {
        #region Field

        private DateTime _scanTime;
        private string _scanUID = string.Empty;
        private int _tubeNo;
        private string _tubeSN = string.Empty;
        private string _scanParamStr = string.Empty;
        private uint[] _kv = [];
        private double[] _ma = [];
        private double _exposureTime;
        private double _kvDose;
        private double _maDose;
        private double _exposureTimeDose;
        private double _frameTime;
        private ScanOption _scanOption;
        private ExposureMode _exposureMode;
        private FocalType _focal;
        private uint _totalFrames;
        private int _views;
        private string _microArcingCountStr = string.Empty;
        private string _bigArcingCountStr = string.Empty;
        private double _heatCapBeforeScan;
        private double _oilTempBeforeScan;
        private double _heatCapAfterScan;
        private double _oilTempAfterScan;

        #endregion

        public DateTime ScanTime
        {
            get => _scanTime;
            set => SetProperty(ref _scanTime, value);
        }

        public string ScanUID
        {
            get => _scanUID;
            set => SetProperty(ref _scanUID, value);
        }

        public int TubeNo
        {
            get => _tubeNo;
            set => SetProperty(ref _tubeNo, value);
        }

        public string TubeSN
        {
            get => _tubeSN;
            set => SetProperty(ref _tubeSN, value);
        }

        public string ScanParamStr
        {
            get => _scanParamStr;
            set => SetProperty(ref _scanParamStr, value);
        }

        public uint[] KV
        {
            get => _kv;
            set => SetProperty(ref _kv, value);
        }

        public double[] MA
        {
            get => _ma;
            set => SetProperty(ref _ma, value);
        }

        public double ExposureTime
        {
            get => _exposureTime;
            set => SetProperty(ref _exposureTime, value);
        }

        public double KVDose
        {
            get => _kvDose;
            set => SetProperty(ref _kvDose, value);
        }

        public double MADose
        {
            get => _maDose;
            set => SetProperty(ref _maDose, value);
        }

        public double ExposureTimeDose
        {
            get => _exposureTimeDose;
            set => SetProperty(ref _exposureTimeDose, value);
        }

        public double FrameTime
        {
            get => _frameTime;
            set => SetProperty(ref _frameTime, value);
        }

        public ScanOption ScanOption
        {
            get => _scanOption;
            set => SetProperty(ref _scanOption, value);
        }

        public ExposureMode ExposureMode
        {
            get => _exposureMode;
            set => SetProperty(ref _exposureMode, value);
        }

        public FocalType Focal
        {
            get => _focal;
            set => SetProperty(ref _focal, value);
        }

        public uint TotalFrames
        {
            get => _totalFrames;
            set => SetProperty(ref _totalFrames, value);
        }

        /// <summary>
        /// 本球管，本次扫描的曝光数
        /// </summary>
        public int Views
        {
            get => _views;
            set => SetProperty(ref _views, value);
        }

        public string MicroArcingCountStr
        {
            get => _microArcingCountStr;
            set => SetProperty(ref _microArcingCountStr, value);
        }

        public string BigArcingCountStr
        {
            get => _bigArcingCountStr;
            set => SetProperty(ref _bigArcingCountStr, value);
        }

        public double HeatCapBeforeScan
        {
            get => _heatCapBeforeScan;
            set => SetProperty(ref _heatCapBeforeScan, value);
        }

        public double OilTempBeforeScan
        {
            get => _oilTempBeforeScan;
            set => SetProperty(ref _oilTempBeforeScan, value);
        }

        public double HeatCapAfterScan
        {
            get => _heatCapAfterScan;
            set => SetProperty(ref _heatCapAfterScan, value);
        }

        public double OilTempAfterScan
        {
            get => _oilTempAfterScan;
            set => SetProperty(ref _oilTempAfterScan, value);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Prism.Commands;
using Microsoft.Extensions.Logging;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Contract;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.UI.ViewModel;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using ScanModel = NV.CT.Protocol.Models.ScanModel;
using NV.CT.Language;
using NV.MPS.Environment;
using NV.CT.DatabaseService.Contract;
using NV.MPS.Configuration;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using System.Windows.Controls;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ScanParameterControlViewModel : BaseViewModel
    {
        private const string STRING_ZERO = "0";
        private const string STRING_BOLUS = "Bolus"; 
        private ScanParameter _scanParameter;
        private readonly IProtocolApplicationService _protocolApplicationService;
        private readonly IDialogService _dialogService;
        private readonly IVoiceService _voiceService;
        private readonly ILogger<ScanParameterControlViewModel> _logger;
        private const int HEIGHT_HIDE = 0;
        private const int HEIGHT_SHOW = 60;

        #region Properties

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private int _hightOfAutoScanRow = 0;
        public int HightOfAutoScanRow
        {
            get => _hightOfAutoScanRow;
            set => SetProperty(ref _hightOfAutoScanRow, value);
        }

        private int _hightOfLoopsRow = 0;
        public int HightOfLoopsRow
        {
            get => _hightOfLoopsRow;
            set => SetProperty(ref _hightOfLoopsRow, value);
        }


        public ScanParameter ScanParameter
        {
            get => _scanParameter;
            set => SetProperty(ref _scanParameter, value);
        }

        public ObservableCollection<KeyValuePair<int, string>> _kvValueList;
        public ObservableCollection<KeyValuePair<int, string>> KvValueList
        {
            get => _kvValueList;
            set => SetProperty(ref _kvValueList, value);
        }

        private KeyValuePair<int, string> _selectedKv;
        public KeyValuePair<int, string> SelectedKv
        {
            get => _selectedKv;
            set => SetProperty(ref _selectedKv, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _allScanOptionList;
        public ObservableCollection<KeyValuePair<int, string>> AllScanOptionList
        {
            get => _allScanOptionList;
            set => SetProperty(ref _allScanOptionList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _scanOptionList;
        public ObservableCollection<KeyValuePair<int, string>> ScanOptionList
        {
            get => _scanOptionList;
            set => SetProperty(ref _scanOptionList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _collimatorOpenModeList;
        public ObservableCollection<KeyValuePair<int, string>> CollimatorOpenModeList
        {
            get => _collimatorOpenModeList;
            set => SetProperty(ref _collimatorOpenModeList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _preVoiceist;
        public ObservableCollection<KeyValuePair<int, string>> PreVoiceist
        {
            get => _preVoiceist;
            set => SetProperty(ref _preVoiceist, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _postVoiceist;
        public ObservableCollection<KeyValuePair<int, string>> PostVoiceist
        {
            get => _postVoiceist;
            set => SetProperty(ref _postVoiceist, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _objectFovList;

        public ObservableCollection<KeyValuePair<int, string>> ObjectFovList
        {
            get => _objectFovList;
            set => SetProperty(ref _objectFovList, value);
        }

        private KeyValuePair<int, string> _selectedScanOption;
        public KeyValuePair<int, string> SelectedScanOption
        {
            get => _selectedScanOption;
            set
            {
                SetProperty(ref _selectedScanOption, value);
                this.RefreshVisibility();
            } 
        }

        private KeyValuePair<int, string> _selectedPreVoice;
        public KeyValuePair<int, string> SelectedPreVoice
        {
            get => _selectedPreVoice;
            set
            {
                SetProperty(ref _selectedPreVoice, value);
                this.CheckDelay(value.Key);
            } 
        }

        private KeyValuePair<int, string> _selectedPostVoice;
        public KeyValuePair<int, string> SelectedPostVoice
        {
            get => _selectedPostVoice;
            set => SetProperty(ref _selectedPostVoice, value);
        }

        private KeyValuePair<int, string> _selectedCollimatorOpenMode;
        public KeyValuePair<int, string> SelectedCollimatorOpenMode
        {
            get => _selectedCollimatorOpenMode;
            set => SetProperty(ref _selectedCollimatorOpenMode, value);
        }

        private KeyValuePair<int, string> _selectedObjectFov;
        public KeyValuePair<int, string> SelectedObjectFov
        {
            get => _selectedObjectFov;
            set => SetProperty(ref _selectedObjectFov, value);
        }

        private Visibility _pitchShow = Visibility.Collapsed;
        public Visibility PitchShow
        {
            get => _pitchShow;
            set => SetProperty(ref _pitchShow, value);
        }

        private Visibility _tableFeedShow = Visibility.Visible;
        public Visibility TableFeedShow
        {
            get => _tableFeedShow;
            set => SetProperty(ref _tableFeedShow, value);
        }

        private bool _selectedAutoScan = false;
        public bool SelectedAutoScan
        {
            get => _selectedAutoScan;
            set => SetProperty(ref _selectedAutoScan, value);
        }

        private bool _selectedIsEnhanced = false;
        public bool SelectedIsEnhanced
        {
            get => _selectedIsEnhanced;
            set => SetProperty(ref _selectedIsEnhanced, value);
        }

        private bool _scanIsEnabled = false;

        public bool ScanIsEnabled
        {
            get => _scanIsEnabled;
            set => SetProperty(ref _scanIsEnabled, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _scanBinningList;
        public ObservableCollection<KeyValuePair<int, string>> ScanBinningList
        {
            get => _scanBinningList;
            set => SetProperty(ref _scanBinningList, value);
        }
        private KeyValuePair<int, string> _selectedScanBinning;
        public KeyValuePair<int, string> SelectedScanBinning
        {
            get => _selectedScanBinning;
            set => SetProperty(ref _selectedScanBinning, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _scanModeList;
        public ObservableCollection<KeyValuePair<int, string>> ScanModeList
        {
            get => _scanModeList;
            set => SetProperty(ref _scanModeList, value);
        }
        private KeyValuePair<int, string> _selectedScanMode;
        public KeyValuePair<int, string> SelectedScanMode
        {
            get => _selectedScanMode;
            set => SetProperty(ref _selectedScanMode, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _rawDataTypeList;
        public ObservableCollection<KeyValuePair<int, string>> RawDataTypeList
        {
            get => _rawDataTypeList;
            set => SetProperty(ref _rawDataTypeList, value);
        }
        private KeyValuePair<int, string> _selectedRawDataType;
        public KeyValuePair<int, string> SelectedRawDataType
        {
            get => _selectedRawDataType;
            set => SetProperty(ref _selectedRawDataType, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _exposureModeList;
        public ObservableCollection<KeyValuePair<int, string>> ExposureModeList
        {
            get => _exposureModeList;
            set => SetProperty(ref _exposureModeList, value);
        }
        private KeyValuePair<int, string> _selectedExposureMode;
        public KeyValuePair<int, string> SelectedExposureMode
        {
            get => _selectedExposureMode;
            set => SetProperty(ref _selectedExposureMode, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _tableDirectionList;
        public ObservableCollection<KeyValuePair<int, string>> TableDirectionList
        {
            get => _tableDirectionList;
            set => SetProperty(ref _tableDirectionList, value);
        }
        private KeyValuePair<int, string> _selectedTableDirection;
        public KeyValuePair<int, string> SelectedTableDirection
        {
            get => _selectedTableDirection;
            set => SetProperty(ref _selectedTableDirection, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _gainList;
        public ObservableCollection<KeyValuePair<int, string>> GainList
        {
            get => _gainList;
            set => SetProperty(ref _gainList, value);
        }
        private KeyValuePair<int, string> _selectedGain;
        public KeyValuePair<int, string> SelectedGain
        {
            get => _selectedGain;
            set => SetProperty(ref _selectedGain, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _bodyPartList;
        public ObservableCollection<KeyValuePair<int, string>> BodyPartList
        {
            get => _bodyPartList;
            set => SetProperty(ref _bodyPartList, value);
        }
        private KeyValuePair<int, string> _selectedBodyPart;
        public KeyValuePair<int, string> SelectedBodyPart
        {
            get => _selectedBodyPart;
            set => SetProperty(ref _selectedBodyPart, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _xRayFocusList;
        public ObservableCollection<KeyValuePair<int, string>> XRayFocusList
        {
            get => _xRayFocusList;
            set => SetProperty(ref _xRayFocusList, value);
        }
        private KeyValuePair<int, string> _selectedXRayFocus;
        public KeyValuePair<int, string> SelectedXRayFocus
        {
            get => _selectedXRayFocus;
            set => SetProperty(ref _selectedXRayFocus, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _tubePositionsList;
        public ObservableCollection<KeyValuePair<int, string>> TubePositionsList
        {
            get => _tubePositionsList;
            set => SetProperty(ref _tubePositionsList, value);
        }

        private KeyValuePair<int, string> _selectedTubePositions0;
        public KeyValuePair<int, string> SelectedTubePositions0
        {
            get => _selectedTubePositions0;
            set => SetProperty(ref _selectedTubePositions0, value);
        }

        private KeyValuePair<int, string> _selectedTubePositions1;
        public KeyValuePair<int, string> SelectedTubePositions1
        {
            get => _selectedTubePositions1;
            set => SetProperty(ref _selectedTubePositions1, value);
        }

        #endregion

        #region Constructor
        public ScanParameterControlViewModel(IProtocolApplicationService protocolApplicationService, 
                                             IDialogService dialogService, 
                                             ILogger<ScanParameterControlViewModel> logger,
                                             IVoiceService voiceService)
        {
            ScanParameter = new();
            _protocolApplicationService = protocolApplicationService;
            _dialogService = dialogService;
            _voiceService = voiceService;
            _logger = logger;

            InitComboBoxItem();
            EventResponse();
            CollectCommand();
            InitDynamicPara();

            this.HightOfAutoScanRow = ScanParameter.ScanName == ProtocolParameterNames.SCAN_OPTION_TOPOGRAM ? HEIGHT_HIDE : HEIGHT_SHOW;
        }

        #endregion

        private void InitComboBoxItem()
        {
			int kvMin = SystemConfig.ScanningParamConfig.ScanningParam.AvailableVoltages.Min;
			int kvMax = SystemConfig.ScanningParamConfig.ScanningParam.AvailableVoltages.Max;
			KvValueList = new ObservableCollection<KeyValuePair<int, string>>();
			for (int i = kvMin; i <= kvMax; i = i + 10)
			{
				KvValueList.Add(new KeyValuePair<int, string>(i, i.ToString()));
			}
			AllScanOptionList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(ScanOption));
            ScanOptionList = AllScanOptionList;
            ScanBinningList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(ScanBinning));
            ScanModeList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(ScanMode));
            RawDataTypeList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(RawDataType));
            ExposureModeList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(ExposureMode));
            TableDirectionList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(TableDirection));
            GainList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(Gain));
            BodyPartList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(CTS.Enums.BodyPart));
            XRayFocusList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(FocalType));
            TubePositionsList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(TubePosition));
            CollimatorOpenModeList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(CollimatorOpenMode));

            var allValidVoices = _voiceService.GetValidVoices();
            PreVoiceist = allValidVoices.Where(v => v.IsFront)
                                        .OrderBy(v => v.InternalId)
                                        .Select(v => new KeyValuePair<int, string>(v.InternalId, $"{v.InternalId} {v.Description} {v.VoiceLength}"))
                                        .ToList().ToObservableCollection();

            PreVoiceist.Insert(0, new KeyValuePair<int, string>(0, "0 Not selected yet"));

            PostVoiceist = allValidVoices.Where(v => !v.IsFront)
                            .OrderBy(v => v.InternalId)
                            .Select(v => new KeyValuePair<int, string>(v.InternalId, $"{v.InternalId} {v.Description} {v.VoiceLength}"))
                            .ToList().ToObservableCollection();
            PostVoiceist.Insert(0, new KeyValuePair<int, string>(0, "0 Not selected yet"));

            SelectedKv = KvValueList[0];
            SelectedScanOption = ScanOptionList[0];
            SelectedScanBinning = ScanBinningList[0];
            SelectedCollimatorOpenMode = CollimatorOpenModeList[0];

            SelectedPreVoice = PreVoiceist.FirstOrDefault();
            SelectedPostVoice = PostVoiceist.FirstOrDefault();
        }

        private void InitDynamicPara()
        {
            ICollection<string> list = _protocolApplicationService.GetValuesNameToType(SystemSettingKeys.Kv);
            if (null != list && list.Count > 0)
            {
                list.ForEach(str => ScanParameter.KVs.Add(str, new()));
            }
            //_protocolManagementApplicationService.GetValuesNameToType(SystemSettingKeys.Pitch).ForEach(str => ScanParameter.Pitch.Add(str, new()));
        }

        private void CollectCommand()
        {
            Commands.Add(Constants.COMMAND_SAVE, new DelegateCommand(Save));
            Commands.Add(Constants.COMMAND_CHECKED, new DelegateCommand(UpdateVoiceId));
        }
        [UIRoute]
        private void UpdateVoiceId()
        {
            SelectedPreVoice = PreVoiceist.FirstOrDefault(r=>r.Key==4);
            SelectedPostVoice = PostVoiceist.FirstOrDefault(r=>r.Key==3);
        }

        [UIRoute]
        private void Save()
        {
            var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
            var scanModel = template.Protocol.Children.SelectMany(f => f.Children).SelectMany(m => m.Children).FirstOrDefault(s => s.Descriptor.Id == Global.Instance.SelectNodeID);
            if (scanModel is not null)
            {
                scanModel.Descriptor.Name = ScanParameter.ScanName;
                scanModel=UpdateScanParameters(scanModel);
            }else
            {
                return;
            }
            string message = string.Empty;
            try
            {
                _protocolApplicationService.Save(template);
                message = LanguageResource.Message_Saved_Successfully;
            }
            catch (Exception e)
            {
                this._logger.LogWarning($"Failed to save scaning for {template.FileName}, the exception is:{e.Message}");
                message = LanguageResource.Message_FailedToSave;
            }
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseInformationTitle, message, a =>
            {
                if (a.Result == ButtonResult.OK)
                {
                    //TODO
                }
            }, ConsoleSystemHelper.WindowHwnd);
        }
        private ScanModel UpdateScanParameters(ScanModel scanModel)
        {
      
            var scanLength = string.IsNullOrEmpty(ScanParameter.ScanLength) ? 0 : float.Parse(ScanParameter.ScanLength) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, scanLength.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_AUTO_SCAN, SelectedAutoScan.ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.IS_ENHANCED, (SelectedIsEnhanced ? 1 : 0).ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_OPTION, SelectedScanOption.Value, true);
            if (SelectedScanOption.Value== "DualScout")
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_MILLIAMPERE, $"[{ScanParameter.MA},{ScanParameter.MA},0,0,0,0,0,0]", true);
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_KILOVOLT, $"[{SelectedKv.Value},{SelectedKv.Value},0,0,0,0,0,0]", true);
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE, "DualScout", true);
            }
            else
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_MILLIAMPERE, $"[{ScanParameter.MA},0,0,0,0,0,0,0]", true);
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_KILOVOLT, $"[{SelectedKv.Value},0,0,0,0,0,0,0]", true);
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE, "Normal", true);
            }

            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_PRE_VOICE_ID, SelectedPreVoice.Key.ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_POST_VOICE_ID, SelectedPostVoice.Key.ToString(), true);
            var exposureDelayTime = !string.IsNullOrEmpty(ScanParameter.ExposureDelayTime) ? (int.Parse(ScanParameter.ExposureDelayTime) * Constants.MICROSECOND_UNIT).ToString() : STRING_ZERO;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME, exposureDelayTime, true);
            var preVoiceDelayTime = !string.IsNullOrEmpty(ScanParameter.PreVoiceDelayTime) ? (int.Parse(ScanParameter.PreVoiceDelayTime) * Constants.MICROSECOND_UNIT).ToString() : STRING_ZERO;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME, preVoiceDelayTime, true);
            var pitch = (float.TryParse(ScanParameter.Pitch, out float outPitchValue) ? outPitchValue : 0) * Constants.HUNDRED_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_PITCH, pitch.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_BINNING, SelectedScanBinning.Value, true);
            var exposureTime = !string.IsNullOrEmpty(ScanParameter.ExposureTime) ? (float.Parse(ScanParameter.ExposureTime) * Constants.MILLIMETER_UNIT).ToString() : STRING_ZERO;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_EXPOSURE_TIME, exposureTime, true);
            var frameTime = !string.IsNullOrEmpty(ScanParameter.FrameTime) ? (float.Parse(ScanParameter.FrameTime) * Constants.MILLIMETER_UNIT).ToString() : STRING_ZERO;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_FRAME_TIME, frameTime, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_FRAMES_PER_CYCLE, ScanParameter.FramesPerCycle, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_MODE, SelectedScanMode.Value, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_RAW_DATA_TYPE, SelectedRawDataType.Value, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_EXPOSURE_MODE, SelectedExposureMode.Value, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_TABLE_DIRECTION, SelectedTableDirection.Value, true);
            var collimatorz = int.TryParse(ScanParameter.CollimitorZ, out int outCollimatorz) ? outCollimatorz : 242;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_COLLIMATOR_Z, collimatorz.ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_BOWTIE_ENABLE, ScanParameter.IsBowtieEnabled.ToString(), true);
            var collimitorSliceWidth = string.IsNullOrEmpty(ScanParameter.CollimitorSliceWidth) ? 0 : float.Parse(ScanParameter.CollimitorSliceWidth) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_COLLIMATOR_SLICE_WIDTH, collimitorSliceWidth.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_GAIN, SelectedGain.Value, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.BODY_PART, SelectedBodyPart.Value, true);
            var tableFeed = string.IsNullOrEmpty(ScanParameter.TableFeed) ? 0 : float.Parse(ScanParameter.TableFeed) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_TABLE_FEED, tableFeed.ToString(STRING_ZERO), true);
            var tableAcceleration = string.IsNullOrEmpty(ScanParameter.TableAcceleration) ? 0 : float.Parse(ScanParameter.TableAcceleration) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_TABLE_ACCELERATION, tableAcceleration.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_FOCAL_TYPE, SelectedXRayFocus.Value, true);
            var gantryAcceleration = string.IsNullOrEmpty(ScanParameter.GantryAcceleration) ? 0 : float.Parse(ScanParameter.GantryAcceleration) * Constants.HUNDRED_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_GANTRY_ACCELERATION, gantryAcceleration.ToString(STRING_ZERO), true);
            var preOffsetFrames = string.IsNullOrEmpty(ScanParameter.PreOffsetFrames) ? 0 : float.Parse(ScanParameter.PreOffsetFrames) ;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_PRE_OFFSET_FRAMES, preOffsetFrames.ToString(), true);
            var postOffsetFrames = string.IsNullOrEmpty(ScanParameter.PostOffsetFrames) ? 0 : float.Parse(ScanParameter.PostOffsetFrames) ;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_PRE_OFFSET_FRAMES, postOffsetFrames.ToString(), true);
            var autoDeleteNum = int.TryParse(ScanParameter.AutoDeleteNum, out int outAutoDeleteNumberValue) ? outAutoDeleteNumberValue : 0;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_AUTO_DELETE_NUMBER, autoDeleteNum.ToString(), true);
            var fov = (float.TryParse(ScanParameter.ScanFOV, out float outFovValue) ? outFovValue : 0) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_FOV, fov.ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_TUBE_POSITIONS, $"[{SelectedTubePositions0.Key},{SelectedTubePositions1.Key}]", true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_LOOPS, ScanParameter.Loops, true);
            var loopTime = !string.IsNullOrEmpty(ScanParameter.LoopTime) ? (float.Parse(ScanParameter.LoopTime) * Constants.MILLIMETER_UNIT).ToString() : STRING_ZERO;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_LOOP_TIME, loopTime, true);
            var objectFov = string.IsNullOrEmpty(ScanParameter.ObjectFov) ? 0 : float.Parse(ScanParameter.ObjectFov) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_OBJECT_FOV, objectFov.ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_IS_VOICE_SUPPORTED, ScanParameter.IsVoiceSupported.ToString(), true);
            var postDeleteLength = string.IsNullOrEmpty(ScanParameter.PostDeleteLength) ? 0 : float.Parse(ScanParameter.PostDeleteLength);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_POST_DELETE_LENGTH, postDeleteLength.ToString(), true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_CTDI, ScanParameter.DoseNotification_CTDI, true);
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_DLP, ScanParameter.DoseNotification_DLP, true);
            var tubeCount = (int.TryParse(ScanParameter.AllowErrorTubeCount, out int outTubeCount) ? outTubeCount : 1);
            ScanParameter.AllowErrorTubeCount = tubeCount.ToString();
            if (ScanParameter.ScanOption=="Surview"|| ScanParameter.ScanOption == "DualScout"|| ScanParameter.ScanOption == "NVTestBolus" || ScanParameter.ScanOption == "NVTestBolusBase" || ScanParameter.ScanOption == "TestBolus"
                ||ScanParameter.ScanMode== "SixteenSourceJumpExposure"||ScanParameter.ScanMode == "EightSourceJumpExposure" || ScanParameter.ScanMode == "TwelveSourceJumpExposure")
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_ALLOW_ERROR_TUBE_COUNT, "0", true);
            }
            else
            {
                var allowErrorTubeCount = (int.TryParse(ScanParameter.AllowErrorTubeCount, out int outAllowErrorTubeCount) ? outAllowErrorTubeCount : 1);
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_ALLOW_ERROR_TUBE_COUNT, allowErrorTubeCount.ToString(), true);
            }
            var exposuerIntervalTime = (int.TryParse(ScanParameter.ExposureIntervalTime, out int outExposuerIntervalTime) ? outExposuerIntervalTime : 10)* Constants.MICROSECOND_UNIT;
            ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_EXPOSURE_INTERVAL_TIME, exposuerIntervalTime.ToString(), true);
            if (ScanParameter.ScanOption == "Surview")
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_ACTIVE_EXPOSURE_SOURCE_COUNT, "1", true);
            } else if(ScanParameter.ScanOption == "DualScout")
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_ACTIVE_EXPOSURE_SOURCE_COUNT, "2", true);
            }
            else
            {
                var exposureSourceCount = (int.TryParse(ScanParameter.ActiveExposureSourceCount, out int outExposureSourceCount) ? outExposureSourceCount : 24) ;
                exposureSourceCount= exposureSourceCount==0 ? 24 : exposureSourceCount;
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_ACTIVE_EXPOSURE_SOURCE_COUNT, exposureSourceCount.ToString(), true);
            }
            if (collimatorz==288)
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_COLLIMATOR_OPEN_MODE, CollimatorOpenModeList[1].Value, true);
            }
            else
            {
                ProtocolHelper.SetParameter(scanModel, ProtocolParameterNames.SCAN_COLLIMATOR_OPEN_MODE, CollimatorOpenModeList[0].Value, true);
            }
            return scanModel;
        }
        private void EventResponse()
        {
            _protocolApplicationService.ProtocolTreeSelectNodeChanged += TreeSelectChanged;
        }

        private void TreeSelectChanged(object? sender, CTS.EventArgs<(string NodeType, string NodeId, string TemplateId)> nodeTypeNodeIdAndTemplateID)
        {
            if (nodeTypeNodeIdAndTemplateID.Data.NodeType == ProtocolLayeredName.SCAN_NODE)
            {
                InitScanParameter(nodeTypeNodeIdAndTemplateID.Data.NodeId.ToString(), nodeTypeNodeIdAndTemplateID.Data.TemplateId.ToString());
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void InitScanParameter(string ScanID, string TemplateID)
        {

            Global.Instance.SelectTemplateID = TemplateID;
            Global.Instance.SelectNodeID = ScanID;
            var protocolTemplate = _protocolApplicationService.GetProtocolTemplate(TemplateID);
            ProtocolHelper.ResetParent(protocolTemplate.Protocol);

            var scan = (from f in protocolTemplate.Protocol.Children
                        from m in f.Children
                        from s in m.Children
                        where s.Descriptor.Id == ScanID
                        select s).FirstOrDefault();
            if (scan is not null)
            {
                ProtocolDTOToModelForView(scan);
            }
            else
            {
                //TODO:
            }
        }

        /// <summary>
        /// 数据模型转为UI模型
        /// </summary>
        /// <param name="protocolTemplate"></param>
        private void ProtocolDTOToModelForView(ScanModel scanModel)
        {
            //如果开发环境开关打开，则始终允许修改；否则检查是否是出厂协议  
            ScanIsEnabled = RuntimeConfig.IsDevelopment ? true : !scanModel.Parent.Parent.Parent.IsFactory; 
            ScanParameter.ScanName = scanModel.Descriptor.Name;
            ScanParameter.MA = scanModel.Milliampere[0].ToString();
            ScanParameter.KV = scanModel.Kilovolt[0].ToString();
            SelectedKv = KvValueList.FirstOrDefault(n => n.Value == ScanParameter.KV);
            ScanParameter.ScanLength = ((float)scanModel.ScanLength / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.AutoScan = scanModel.AutoScan.ToString();
            SelectedAutoScan = scanModel.AutoScan;
            ScanParameter.IsEnhanced = scanModel.IsEnhanced.ToString();
            SelectedIsEnhanced = scanModel.IsEnhanced;

            //if (ScanParameter.ScanName != ProtocolParameterNames.SCAN_OPTION_TOPOGRAM)
            //{
            //    ScanOptionList = new ObservableCollection<KeyValuePair<int, string>>(AllScanOptionList.Where(t => t.Value != ScanOption.Surview.ToString() && t.Value != ScanOption.DualScout.ToString()));
            //}

            ScanParameter.ScanOption = scanModel.ScanOption.ToString();
            SelectedScanOption = ScanOptionList.FirstOrDefault(n => n.Value == ScanParameter.ScanOption);

            var isFoundPreVoice = PreVoiceist.Any(n => n.Key == scanModel.PreVoiceId);
            if (isFoundPreVoice)
            {
                SelectedPreVoice = PreVoiceist.FirstOrDefault(n => n.Key == scanModel.PreVoiceId);
                ScanParameter.VoiceId = scanModel.PreVoiceId.ToString();               
            }
            else
            {
                SelectedPreVoice = PreVoiceist.FirstOrDefault();
                ScanParameter.VoiceId = SelectedPreVoice.Key.ToString();
            }

            var isFoundPostVoice = PostVoiceist.Any(n => n.Key == scanModel.PostVoiceId);
            if (isFoundPostVoice)
            {
                SelectedPostVoice = PostVoiceist.FirstOrDefault(n => n.Key == scanModel.PostVoiceId);
                ScanParameter.PostVoiceId = scanModel.PostVoiceId.ToString();      
            }
            else
            {
                SelectedPostVoice = PostVoiceist.FirstOrDefault();
                ScanParameter.PostVoiceId = SelectedPostVoice.Key.ToString();
            }
            ScanParameter.PreVoiceDelayTime = (float.Parse(scanModel.Parameters.First(arg => arg.Name == ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME).Value) / Constants.MICROSECOND_UNIT).ToString(Constants.PRECISION_FORMAT_6);
            ScanParameter.ExposureDelayTime = (float.Parse(scanModel.Parameters.First(arg => arg.Name == ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME).Value) / Constants.MICROSECOND_UNIT).ToString(Constants.PRECISION_FORMAT_6);
            ScanParameter.Pitch = ((float)scanModel.Pitch / Constants.HUNDRED_UNIT).ToString(Constants.PRECISION_FORMAT_2);
            ScanParameter.Binning = scanModel.Binning.ToString();
            SelectedScanBinning = ScanBinningList.FirstOrDefault(n => n.Value == ScanParameter.Binning);
            ScanParameter.ExposureTime = (float.Parse(scanModel.Parameters.First(arg => arg.Name == ProtocolParameterNames.SCAN_EXPOSURE_TIME).Value) / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.FrameTime = (float.Parse(scanModel.Parameters.First(arg => arg.Name == ProtocolParameterNames.SCAN_FRAME_TIME).Value) / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.FramesPerCycle = scanModel.FramesPerCycle.ToString();
            ScanParameter.ScanMode = scanModel.ScanMode.ToString();
            SelectedScanMode = ScanModeList.FirstOrDefault(n => n.Value == ScanParameter.ScanMode);
            ScanParameter.RawDataType = scanModel.RawDataType.ToString();
            SelectedRawDataType = RawDataTypeList.FirstOrDefault(n => n.Value == ScanParameter.RawDataType);
            ScanParameter.ExposureMode = scanModel.ExposureMode.ToString();
            SelectedExposureMode = ExposureModeList.FirstOrDefault(n => n.Value == ScanParameter.ExposureMode);
            ScanParameter.CollimitorZ = scanModel.CollimatorZ.ToString();
            ScanParameter.CollimatorOpenMode = scanModel.CollimatorOpenMode.ToString();
            if (scanModel.CollimatorZ==288)
            {
                SelectedCollimatorOpenMode = CollimatorOpenModeList[1];
            }else
            {
                SelectedCollimatorOpenMode = CollimatorOpenModeList[0];
            }    
            ScanParameter.TableDirection = scanModel.TableDirection.ToString();
            SelectedTableDirection = TableDirectionList.FirstOrDefault(n => n.Value == ScanParameter.TableDirection);
            ScanParameter.IsBowtieEnabled = scanModel.BowtieEnable;
            ScanParameter.CollimitorSliceWidth = ((float)scanModel.CollimatorSliceWidth / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.Gain = scanModel.Gain.ToString();
            SelectedGain = GainList.FirstOrDefault(n => n.Value == ScanParameter.Gain);
            ScanParameter.BodyPart = scanModel.BodyPart.ToString();
            SelectedBodyPart = BodyPartList.FirstOrDefault(n => n.Value == ScanParameter.BodyPart);
            ScanParameter.TableFeed = ((float)scanModel.TableFeed / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.TableAcceleration = (scanModel.TableAcceleration / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.XRayFocus = scanModel.FocalType.ToString();
            SelectedXRayFocus = XRayFocusList.FirstOrDefault(n => n.Value == ScanParameter.XRayFocus);           
            ScanParameter.GantryAcceleration = ((float)scanModel.GantryAcceleration / Constants.HUNDRED_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.PreOffsetFrames = scanModel.PreOffsetFrames.ToString();
            ScanParameter.PostOffsetFrames = scanModel.PostOffsetFrames.ToString();
            ScanParameter.AutoDeleteNum = scanModel.AutoDeleteNum.ToString();
            ScanParameter.ScanFOV = ((float)scanModel.ScanFOV / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.Loops = scanModel.Loops.ToString();
            ScanParameter.LoopTime = (float.Parse(scanModel.Parameters.First(arg => arg.Name == ProtocolParameterNames.SCAN_LOOP_TIME).Value) / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            if (scanModel.TubePositions.Length >= 2)
            {
                ScanParameter.TubePositions0 = scanModel.TubePositions[0].ToString();
                ScanParameter.TubePositions1 = scanModel.TubePositions[1].ToString();
                SelectedTubePositions0 = TubePositionsList.FirstOrDefault(n => n.Value == ScanParameter.TubePositions0);
                SelectedTubePositions1 = TubePositionsList.FirstOrDefault(n => n.Value == ScanParameter.TubePositions1);
            }
            ScanParameter.ObjectFov= ((float)scanModel.ObjectFOV / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ScanParameter.PostDeleteLength = scanModel.PostDeleteLength.ToString(); 
            ScanParameter.IsVoiceSupported = scanModel.IsVoiceSupported;
            ScanParameter.DoseNotification_CTDI= scanModel.DoseNotificationCTDI.ToString();
            ScanParameter.DoseNotification_DLP = scanModel.DoseNotificationDLP.ToString();
            ScanParameter.AllowErrorTubeCount = scanModel.AllowErrorTubeCount.ToString();
            ScanParameter.ExposureIntervalTime = (scanModel.ExposureIntervalTime/ Constants.MICROSECOND_UNIT).ToString();
            ScanParameter.ActiveExposureSourceCount = scanModel.ActiveExposureSourceCount.ToString();
            this.RefreshVisibility();
        }

        private void CheckDelay(int preVoiceId)
        {
            if (preVoiceId == 0)
            {
                return;
            }

            var selectedVoiceValue = PreVoiceist.First(v => v.Key == preVoiceId).Value;
            int indexOfLastSpace = selectedVoiceValue.LastIndexOf(' ');
            int voiceLength = int.Parse(selectedVoiceValue.Substring(indexOfLastSpace));

            int.TryParse(ScanParameter.ExposureDelayTime, out int expouseDelayTime);
            if (expouseDelayTime < voiceLength)
            {
                ScanParameter.ExposureDelayTime = voiceLength.ToString();
            }
        }

        private void RefreshVisibility()
        {
            this.HightOfAutoScanRow = SelectedScanOption.Value.IndexOf( ProtocolParameterNames.SCAN_OPTION_TOPOGRAM, 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ? HEIGHT_HIDE : HEIGHT_SHOW;
            this.HightOfLoopsRow = (SelectedScanOption.Value.IndexOf(STRING_BOLUS, 0, StringComparison.InvariantCultureIgnoreCase) >= 0) ? HEIGHT_SHOW : HEIGHT_HIDE;

            PitchShow = (SelectedScanOption.Value.IndexOf(ProtocolParameterNames.SCAN_OPTION_TOPOGRAM, 0, StringComparison.InvariantCultureIgnoreCase) >= 0
                          || SelectedScanOption.Value.IndexOf(ScanOption.Axial.ToString(), 0, StringComparison.InvariantCultureIgnoreCase) >= 0) ? Visibility.Collapsed : Visibility.Visible;

            TableFeedShow = SelectedScanOption.Value.IndexOf(ScanOption.Helical.ToString(), 0, StringComparison.InvariantCultureIgnoreCase) >= 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

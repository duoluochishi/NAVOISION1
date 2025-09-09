using Microsoft.Extensions.Logging;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Helpers;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.ReconEnums;
using NV.CT.Language;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Models;
using NV.CT.UI.ViewModel;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Org.BouncyCastle.Pqc.Crypto;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EnumExtension = NV.CT.UI.Controls.Extensions.EnumExtension;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ReconParameterControlViewModel : BaseViewModel
    {
        private const string STRING_ZERO = "0";
        private const string STRING_ONE = "1";
        private const string STRING_CUSTOM = "Custom";
        private readonly IProtocolApplicationService _protocolApplicationService;
        private readonly IDialogService _dialogService;
        private readonly List<WindowingInfo> _windowWidthLevelModels = new();      
        private readonly ILogger<ReconParameterControlViewModel> _logger;
        private string _customWW = "0";
        private string _customWL = "0";


        #region Properties

        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }

        private bool _isBowtie;
        public bool IsBowtie
        {
            get { return _isBowtie; }
            set { SetProperty(ref _isBowtie, value); }
        }

        private ReconParameter _reconParameter;
        public ReconParameter ReconParameter
        {
            get => _reconParameter;
            set => SetProperty(ref _reconParameter, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _imageOrderList;
        public ObservableCollection<KeyValuePair<int, string>> ImageOrderList
        {
            get => _imageOrderList;
            set => SetProperty(ref _imageOrderList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _kernelList;
        public ObservableCollection<KeyValuePair<int, string>> KernelList
        {
            get => _kernelList;
            set => SetProperty(ref _kernelList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _reconMethodList;
        public ObservableCollection<KeyValuePair<int, string>> ReconMethodList
        {
            get => _reconMethodList;
            set => SetProperty(ref _reconMethodList, value);
        }
        private List<KeyValuePair<int, string>> _reconMethods = new();
        public List<KeyValuePair<int, string>> ReconMethods
        {
            get => _reconMethods;
            set => SetProperty(ref _reconMethods, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _removeArtifactList;
        public ObservableCollection<KeyValuePair<int, string>> RemoveArtifactList
        {
            get => _removeArtifactList;
            set => SetProperty(ref _removeArtifactList, value); 
        }

        private ObservableCollection<KeyValuePair<string, string>> _windowList;
        public ObservableCollection<KeyValuePair<string, string>> WindowList
        {
            get => _windowList;
            set => SetProperty(ref _windowList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _sliceThicknessList;
        public ObservableCollection<KeyValuePair<int, string>> SliceThicknessList
        {
            get => _sliceThicknessList;
            set => SetProperty(ref _sliceThicknessList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _reconIncrementList;
        public ObservableCollection<KeyValuePair<int, string>> ReconIncrementList
        {
            get => _reconIncrementList;
            set => SetProperty(ref _reconIncrementList, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _reconMatrixList;
        public ObservableCollection<KeyValuePair<int, string>> ReconMatrixList
        {
            get => _reconMatrixList;
            set => SetProperty(ref _reconMatrixList, value);
        }

        private KeyValuePair<int, string> _selectReconIncrement;
        public KeyValuePair<int, string> SelectReconIncrement
        {
            get => _selectReconIncrement;
            set => SetProperty(ref _selectReconIncrement, value); 
        }

        private KeyValuePair<int, string> _selectSliceThickness;
        public KeyValuePair<int, string> SelectSliceThickness
        {
            get => _selectSliceThickness;
            set => SetProperty(ref _selectSliceThickness, value);
        }


        private KeyValuePair<int, string> _selectReconMatrix;
        public KeyValuePair<int, string> SelectReconMatrix
        {
            get => _selectReconMatrix;
            set => SetProperty(ref _selectReconMatrix, value); 
        }

        private KeyValuePair<int, string> _selectImageOrder;
        public KeyValuePair<int, string> SelectImageOrder
        {
            get => _selectImageOrder;
            set => SetProperty(ref _selectImageOrder, value); 
        }

        private KeyValuePair<int, string> _selectRemoveArtifact;
        public KeyValuePair<int, string> SelectRemoveArtifact
        {
            get => _selectRemoveArtifact;
            set => SetProperty(ref _selectRemoveArtifact, value); 
        }

        private KeyValuePair<int, string> _selectReconMethod;
        public KeyValuePair<int, string> SelectReconMethod
        {
            get => _selectReconMethod;
            set => SetProperty(ref _selectReconMethod, value); 
        }

        private KeyValuePair<int, string> _selectKernel;
        public KeyValuePair<int, string> SelectKernel
        {
            get => _selectKernel;
            set => SetProperty(ref _selectKernel, value);
        }

        private KeyValuePair<string, string> _selectWindow;
        public KeyValuePair<string, string> SelectWindow
        {
            get => _selectWindow;
            set
            {
                SetProperty(ref _selectWindow, value);
                this.SetWindowWidthCenter(value.Key);              
            } 
        }

        private bool _isBoneAritifacEnable = false;
        public bool IsBoneAritifacEnable
        {
            get => _isBoneAritifacEnable;
            set => SetProperty(ref _isBoneAritifacEnable, value);
        }

        private bool _isMetalAritifacEnable = false;
        public bool IsMetalAritifacEnable
        {
            get => _isMetalAritifacEnable;
            set => SetProperty(ref _isMetalAritifacEnable, value);
        }
        private bool _isRingAritifactEnable = false;
        public bool IsRingAritifactEnable
        {
            get => _isRingAritifactEnable;
            set => SetProperty(ref _isRingAritifactEnable, value);
        }
        private bool _isSmoothZEnable = false;
        public bool IsSmoothZEnable
        {
            get => _isSmoothZEnable;
            set => SetProperty(ref _isSmoothZEnable, value);
        }
        private bool _isTwoPassEnable = false;
        public bool IsTwoPassEnable
        {
            get => _isTwoPassEnable;
            set => SetProperty(ref _isTwoPassEnable, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _binningList;
        public ObservableCollection<KeyValuePair<int, string>> BinningList
        {
            get => _binningList;
            set => SetProperty(ref _binningList, value);
        }
        private KeyValuePair<int, string> _selectedBinning;
        public KeyValuePair<int, string> SelectedBinning
        {
            get => _selectedBinning;
            set => SetProperty(ref _selectedBinning, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _interpTypeList;
        public ObservableCollection<KeyValuePair<int, string>> InterpTypeList
        {
            get => _interpTypeList;
            set => SetProperty(ref _interpTypeList, value);
        }
        private KeyValuePair<int, string> _selectedInterpType;
        public KeyValuePair<int, string> SelectedInterpType
        {
            get => _selectedInterpType;
            set => SetProperty(ref _selectedInterpType, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _airCorrectionModeList;
        public ObservableCollection<KeyValuePair<int, string>> AirCorrectionModeList
        {
            get => _airCorrectionModeList;
            set => SetProperty(ref _airCorrectionModeList, value);
        }
        private KeyValuePair<int, string> _selectedAirCorrectionMode;
        public KeyValuePair<int, string> SelectedAirCorrectionMode
        {
            get => _selectedAirCorrectionMode;
            set => SetProperty(ref _selectedAirCorrectionMode, value);
        }

        private bool _reconIsEnabled = false;
        public bool ReconIsEnabled
        {
            get => _reconIsEnabled;
            set => SetProperty(ref _reconIsEnabled, value); 
        }
        private ObservableCollection<KeyValuePair<int, string>> _preDenoiseTypeList;
        public ObservableCollection<KeyValuePair<int, string>> PreDenoiseTypeList
        {
            get => _preDenoiseTypeList;
            set => SetProperty(ref _preDenoiseTypeList, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _postDenoiseTypeList;
        public ObservableCollection<KeyValuePair<int, string>> PostDenoiseTypeList
        {
            get => _postDenoiseTypeList;
            set => SetProperty(ref _postDenoiseTypeList, value);
        }
        private KeyValuePair<int, string> _selectedPreDenoise;
        public KeyValuePair<int, string> SelectedPreDenoise
        {
            get => _selectedPreDenoise;
            set => SetProperty(ref _selectedPreDenoise, value);
        }
        private KeyValuePair<int, string> _selectedPostDenoise;
        public KeyValuePair<int, string> SelectedPostDenoise
        {
            get => _selectedPostDenoise;
            set => SetProperty(ref _selectedPostDenoise, value);
        }
        private ObservableCollection<KeyValuePair<int, string>> _reconBodyPartList;
        public ObservableCollection<KeyValuePair<int, string>> ReconBodyPartList
        {
            get => _reconBodyPartList;
            set => SetProperty(ref _reconBodyPartList, value);
        }
        private KeyValuePair<int, string> _selectedReconBodyPart;
        public KeyValuePair<int, string> SelectedReconBodyPart
        {
            get => _selectedReconBodyPart;
            set => SetProperty(ref _selectedReconBodyPart, value);
        }
        #endregion

        #region Constructor

        public ReconParameterControlViewModel(IProtocolApplicationService protocolApplicationService, 
                                              IDialogService dialogService,
                                              ILogger<ReconParameterControlViewModel> logger)
        {
            ReconParameter = new();
            _protocolApplicationService = protocolApplicationService;
            _dialogService = dialogService;            
            _logger = logger;

            _windowWidthLevelModels = UserConfig.WindowingConfig.Windowings;

            InitComboBoxItem();
            EventResponse();
            CollectCommand();
        }

        #endregion
        private void InitReconMethodList(ScanModel scanModel, ReconModel reconModel)
        {
            SelectReconMethod = new KeyValuePair<int, string>();
            ReconMethodList = new ObservableCollection<KeyValuePair<int, string>>();
            if (reconModel.IsRTD)
            {
                var model = ReconMethods.FirstOrDefault(t => t.Value.Equals(ReconType.HCT.ToString()));
                ReconMethodList.Add(model);
            }
            else
            {
                switch (scanModel.ScanOption)
                {
                    case ScanOption.Surview:
                    case ScanOption.DualScout:
                        var model = ReconMethods.FirstOrDefault(t => t.Value.Equals(ReconType.HCT.ToString()));
                        ReconMethodList.Add(model);
                        break;
                    case ScanOption.Helical:
                        foreach (var item in ReconMethods.FindAll(t => t.Value.Equals(ReconType.FDK.ToString()) || t.Value.Equals(ReconType.IVR_TV.ToString()) || t.Value.Equals(ReconType.IVR_TV_OLD.ToString())).ToList())
                        {
                            ReconMethodList.Add(item);
                        }
                        break;
                    case ScanOption.Axial:
                    case ScanOption.BolusTracking:
                    case ScanOption.TestBolus:
                    case ScanOption.NVTestBolus:
                    case ScanOption.NVTestBolusBase:
                        foreach (var item in ReconMethods.FindAll(t => t.Value.Equals(ReconType.XFDK.ToString()) || t.Value.Equals(ReconType.IVR_TV.ToString()) || t.Value.Equals(ReconType.IVR_TV_OLD.ToString())).ToList())
                        {
                            ReconMethodList.Add(item);
                        }
                        break;
                    default:
                        foreach (var item in ReconMethods)
                        {
                            ReconMethodList.Add(item);
                        }
                        break;
                }
            }
        }
        private void InitComboBoxItem()
        {
            ImageOrderList = EnumExtension.EnumToList(typeof(ImageOrders));
            KernelList = EnumExtension.EnumToList(typeof(FilterType));

            ReconMethodList = EnumExtension.EnumToList(typeof(ReconType));
            ReconMethods = EnumExtension.EnumToList(typeof(ReconType)).ToList();
            RemoveArtifactList = EnumExtension.EnumToList(typeof(RemoveArtifacts));
            WindowList = GetWindowList();

            BinningList = EnumExtension.EnumToList(typeof(PreBinning));
            InterpTypeList = EnumExtension.EnumToList(typeof(InterpType));
            AirCorrectionModeList = EnumExtension.EnumToList(typeof(AirCorrectionMode));

            PreDenoiseTypeList = EnumExtension.EnumToList(typeof(PreDenoiseType));
            PostDenoiseTypeList = EnumExtension.EnumToList(typeof(PostDenoiseType));

            ReconBodyPartList = EnumExtension.EnumToList(typeof(CTS.Enums.BodyPart));

            InitReconMatrix();
			InitSliceThickness();
			InitReconIncrement();

            SelectSliceThickness = SliceThicknessList[0];
            SelectReconIncrement = ReconIncrementList[0];

            if (ReconMatrixList.Count > 0)
                SelectReconMatrix = ReconMatrixList[0];
            SelectImageOrder = ImageOrderList[0];
            SelectRemoveArtifact = RemoveArtifactList[0];
            SelectReconMethod = ReconMethodList[0];
            SelectKernel = KernelList[0];
            SelectedReconBodyPart = ReconBodyPartList[0];

            if (WindowList.Count > 0)
            {
                SelectWindow = WindowList[0];
            }          
        }


		private void InitReconMatrix()
		{
			SelectReconMatrix = new KeyValuePair<int, string>();
			ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
			foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges)
			{
				list.Add(new KeyValuePair<int, string>(item, item.ToString()));
			}
			ReconMatrixList = list;
		}

		private void InitSliceThickness()
		{
			SelectReconMatrix = new KeyValuePair<int, string>();
			ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
			foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.SliceThickness.Ranges)
			{
				list.Add(new KeyValuePair<int, string>(item, UnitConvert.ReduceThousand((float)item).ToString()));
			}
			SliceThicknessList = list;
		}

		private void InitReconIncrement()
		{
			SelectReconMatrix = new KeyValuePair<int, string>();
			ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
			foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.ImageIncrement.Ranges)
			{
				list.Add(new KeyValuePair<int, string>(item, UnitConvert.ReduceThousand((float)item).ToString()));
			}
			ReconIncrementList = list;
		}

		private ObservableCollection<KeyValuePair<string, string>> GetWindowList()
        {
            ObservableCollection<KeyValuePair<string, string>> keyValuePairs = new ObservableCollection<KeyValuePair<string, string>>();
            foreach (var item in _windowWidthLevelModels)
            {
                keyValuePairs.Add(new KeyValuePair<string, string>(item.BodyPart, item.BodyPart));
            }
            keyValuePairs.Add(new KeyValuePair<string, string>(ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE, ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE));
            return keyValuePairs;
        }

        private void CollectCommand()
        {
            Commands.Add(Constants.COMMAND_SAVE, new DelegateCommand(Save));
        }

        private void SetWindowWidthCenter(string windowKey)
        {
            var foundWindow = _windowWidthLevelModels.FirstOrDefault(w => w.BodyPart.IndexOf(windowKey, StringComparison.InvariantCultureIgnoreCase) >= 0);
            if (foundWindow is not null)
            {
                ReconParameter.WindowWidth = foundWindow.Width.Value.ToString();
                ReconParameter.WindowCenter = foundWindow.Level.Value.ToString();
            }
            else if (windowKey == STRING_CUSTOM)
            {
                ReconParameter.WindowWidth = this._customWW;
                ReconParameter.WindowCenter = this._customWL;
            }
            else
            {
                ReconParameter.WindowWidth = "0";
                ReconParameter.WindowCenter = "0";
            }
        }

        [UIRoute]
        private void Save()
        {
            var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
            ReconModel? reconMode =  template.Protocol.Children.SelectMany(f => f.Children)
                                     .SelectMany(m => m.Children)
                                     .SelectMany(s => s.Children)
                                     .FirstOrDefault(r => r.Descriptor.Id == Global.Instance.SelectNodeID);

            if (reconMode == null)
            {
                return;
            }
            else 
            {
                reconMode = UpdateReconParameters(reconMode);
            }

            string message = string.Empty;
            try
            {
                _protocolApplicationService.Save(template);
                message = LanguageResource.Message_Saved_Successfully;
            }
            catch (Exception e)
            {
                this._logger.LogWarning($"Failed to save Recon for {template.FileName}, the exception is:{e.Message}");
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

        private ReconModel UpdateReconParameters(ReconModel reconModel)
        {
            var convertedValue = float.Parse(SelectSliceThickness.Value) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_SLICE_THICKNESS, convertedValue.ToString(STRING_ZERO),true);
            var increment = float.Parse(SelectReconIncrement.Value) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IMAGE_INCREMENT, increment.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_BONE_ARITIFACT_ENABALE, this.IsBoneAritifacEnable.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_METAL_ARITIFACT_ENABLE, this.IsMetalAritifacEnable.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_RING_ARITIFACT_ENABLE, this.IsRingAritifactEnable.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_RECON_TYPE, SelectReconMethod.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_FILTER_TYPE, ((FilterType[])Enum.GetValues(typeof(FilterType)))[SelectKernel.Key].ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_INTERP_TYPE, SelectedInterpType.Key.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_AIR_CORRECTION_MODE, SelectedAirCorrectionMode.Value.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_WINDOW_TYPE, SelectWindow.Value.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_WINDOW_WIDTH, $"[{_reconParameter.WindowWidth}]", true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_WINDOW_CENTER, $"[{_reconParameter.WindowCenter}]", true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_PRE_BINNING, SelectedBinning.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL, SelectedBinning.Value, true);
            var reconFOVValue = float.Parse(_reconParameter.ReconFOV) * Constants.MILLIMETER_UNIT;
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL, reconFOVValue.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL, reconFOVValue.ToString(STRING_ZERO), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL, SelectReconMatrix.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL, SelectReconMatrix.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IMAGE_ORDER, SelectImageOrder.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_CENTER_FIRST_X, _reconParameter.CenterX, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_CENTER_FIRST_Y, _reconParameter.CenterY, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_PRE_DENOISE_TYPE, SelectedPreDenoise.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_POST_DENOISE_TYPE, SelectedPostDenoise.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_PRE_DENOISE_COEF, _reconParameter.PreDenoiseCoef, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_POST_DENOISE_COEF, _reconParameter.PostDenoiseCoef, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IVR_TV_COEF, _reconParameter.IVRTVCoef, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_RING_CORRECTION_COEF, _reconParameter.RingCorrectCoef, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IS_RTD, bool.Parse(_reconParameter.IsRTD) ? true.ToString() : false.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_SMOOTH_Z_ENABLE, this.IsSmoothZEnable.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_TWO_PASS_ENABLE, this.IsTwoPassEnable.ToString(), true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_BODY_PART, SelectedReconBodyPart.Value, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IS_HD_RECON, _reconParameter.IsHDRecon, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_WINDMILL_ARTIFACT_ENABLE, _reconParameter.IsWindMillEanble, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_CONE_ANGLE_ARTIFACT_ENABLE, _reconParameter.IsWindMillEanble, true);
            ProtocolHelper.SetParameter(reconModel, ProtocolParameterNames.RECON_IS_AUTO_RECON, _reconParameter.IsAutoRecon, true);
            return reconModel;
        }

        /// <summary>
        /// 事件响应
        /// </summary>
        private void EventResponse()
        {
            _protocolApplicationService.ProtocolTreeSelectNodeChanged += TreeSelectChanged;
        }

        private void TreeSelectChanged(object? sender, EventArgs<(string NodeType, string NodeId, string TemplateId)> nodeTypeNodeIdAndTemplateID)
        {
            if (nodeTypeNodeIdAndTemplateID.Data.NodeType == ProtocolLayeredName.RECON_NODE)
            {
                InitGetReconParameter(nodeTypeNodeIdAndTemplateID.Data.NodeId.ToString(), nodeTypeNodeIdAndTemplateID.Data.TemplateId.ToString());
                IsEnabled = true;
            }
            else
            {
                IsEnabled = false;
            }
        }

        private void InitGetReconParameter(string reconID, string templateID)
        {
            Global.Instance.SelectNodeID = reconID;
            Global.Instance.SelectTemplateID = templateID;
            ReconModel recon = GetReconModel(reconID, templateID);  
            if (recon is not null)
            {
                ProtocolDTOToModelForView(recon);
            }
        }

        private ReconModel GetReconModel(string reconID, string templateID)
        {
            var protocolTemplate = _protocolApplicationService.GetProtocolTemplate(templateID);
            ProtocolHelper.ResetParent(protocolTemplate.Protocol);
            ReconModel recon = null;
            protocolTemplate.Protocol.Children.ForEach(f =>
            {
                f.Children.ForEach(m =>
                {
                    m.Children.ForEach(s =>
                    {
                        s.Children.ForEach(r =>
                        {
                            if (r.Descriptor.Id == reconID)
                            {
                                recon = r;
                            }
                        });
                    });
                });
            });
            
            return recon;
        }

        private void ProtocolDTOToModelForView(ReconModel reconModel)
        {
            InitReconMethodList(reconModel.Parent,reconModel);
            //如果开发环境开关打开，则始终允许修改；否则检查是否是出厂协议
            ReconIsEnabled = RuntimeConfig.IsDevelopment ? true : !(reconModel.Parent.Parent.Parent.Parent.IsFactory || reconModel.IsRTD);
            ReconParameter.SeriesDescription = reconModel.DefaultSeriesDescription;  
            
            ReconParameter.SliceThickness = ((float)reconModel.SliceThickness / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            SelectSliceThickness = SliceThicknessList.Any(t => t.Value == ReconParameter.SliceThickness.Trim()) ? SliceThicknessList.FirstOrDefault(t => t.Value == ReconParameter.SliceThickness.Trim()) : SliceThicknessList[0];
            ReconParameter.ImageIncrement = ((float)reconModel.ImageIncrement / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            SelectReconIncrement = ReconIncrementList.Any(t => t.Value == ReconParameter.ImageIncrement.Trim()) ? ReconIncrementList.FirstOrDefault(t => t.Value == ReconParameter.ImageIncrement.Trim()) : ReconIncrementList[0];
            ReconParameter.PreDenoiseType=reconModel.PreDenoiseType.ToString();
            ReconParameter.PostDenoiseType = reconModel.PostDenoiseType.ToString();

            if (ReconParameter.PreDenoiseType.IsNullOrEmpty())
            {
                SelectedPreDenoise= PreDenoiseTypeList[0]; 
            }
            else
            {
                SelectedPreDenoise = PreDenoiseTypeList.Any(t => t.Value == ReconParameter.PreDenoiseType.Trim()) ? PreDenoiseTypeList.FirstOrDefault(t => t.Value == ReconParameter.PreDenoiseType.Trim()) : PreDenoiseTypeList[0];
            }
            if (ReconParameter.PostDenoiseType.IsNullOrEmpty())
            {
                SelectedPostDenoise= PostDenoiseTypeList[0];
            }
            else
            {
                SelectedPostDenoise = PostDenoiseTypeList.Any(t => t.Value == ReconParameter.PostDenoiseType.Trim()) ? PostDenoiseTypeList.FirstOrDefault(t => t.Value == ReconParameter.PostDenoiseType.Trim()) : PostDenoiseTypeList[0];
            }
            

            IsBoneAritifacEnable = reconModel.BoneAritifactEnable;
            IsMetalAritifacEnable = reconModel.MetalAritifactEnable;
            IsRingAritifactEnable = reconModel.RingAritifactEnable;
            IsSmoothZEnable = reconModel.SmoothZEnable;
            IsTwoPassEnable = reconModel.TwoPassEnable;

            ReconParameter.IVRTVCoef = reconModel.IVRTVCoef.ToString();
            ReconParameter.RingCorrectCoef = reconModel.RingCorrectionCoef.ToString();
            ReconParameter.PreDenoiseCoef = reconModel.PreDenoiseCoef.ToString();
            ReconParameter.PostDenoiseCoef = reconModel.PostDenoiseCoef.ToString();
            ReconParameter.ReconType = reconModel.ReconType.ToString();
            SelectReconMethod = ReconMethodList.Any(t => t.Value == ReconParameter.ReconType.Trim())?ReconMethodList.FirstOrDefault(t => t.Value == ReconParameter.ReconType.Trim()): ReconMethodList[0];
            ReconParameter.FilterType = reconModel.Parameters.FirstOrDefault(r => r.Name == ProtocolParameterNames.RECON_FILTER_TYPE).Value;
            //SelectKernel = KernelList.Any(t=>t.Value== reconModel.FilterTypeDisplay.Trim())? KernelList.FirstOrDefault(t => t.Value == reconModel.FilterTypeDisplay.Trim()): KernelList[0];
            SelectKernel = KernelList.FirstOrDefault(t => t.Value == ReconParameter.FilterType);
            ReconParameter.WindowType = reconModel.WindowType.ToString();
            var lowserWindowType = ReconParameter.WindowType.ToLower();
            SelectWindow = WindowList.Any(t => t.Value == ReconParameter.WindowType.Trim()) ? WindowList.FirstOrDefault(t => t.Value.ToLower() == lowserWindowType) : WindowList[0];

            if (ReconParameter.WindowType == STRING_CUSTOM)
            {
                this._customWW = reconModel.WindowWidth[0].ToString();
                this._customWL = reconModel.WindowCenter[0].ToString();
            }

            ReconParameter.WindowWidth = reconModel.WindowWidth[0].ToString();
            ReconParameter.WindowCenter = reconModel.WindowCenter[0].ToString();

            //ReconParameter.TvDenoiseCoef = reconModel.TvDenoiseCoef.ToString();
            //ReconParameter.Bm3dDenoiseCoef = reconModel.BM3DDenoiseCoef.ToString();
            ReconParameter.ReconFOV = ((float)reconModel.FOVLengthHorizontal / Constants.MILLIMETER_UNIT).ToString(Constants.PRECISION_FORMAT_3);
            ReconParameter.ImageMatrixHor = reconModel.ImageMatrixHorizontal.ToString();
            ReconParameter.ImageMatrixVert = reconModel.ImageMatrixVertical.ToString();
            SelectReconMatrix = ReconMatrixList.FirstOrDefault(t => t.Value == ReconParameter.ImageMatrixHor);
            ReconParameter.ImageOrder = reconModel.ImageOrder.ToString();
            SelectImageOrder = ImageOrderList.Any(t => t.Value == ReconParameter.ImageOrder.Trim()) ? ImageOrderList.FirstOrDefault(t => t.Value == ReconParameter.ImageOrder.Trim()) : ImageOrderList[0];
            ReconParameter.CenterX = reconModel.CenterFirstX.ToString();
            ReconParameter.CenterY = reconModel.CenterFirstY.ToString();
            ReconParameter.Binning = reconModel.PreBinning.ToString();
            if (ReconParameter.Binning is not null)
            {
                SelectedBinning = BinningList.Any(t => t.Value == ReconParameter.Binning.Trim()) ? BinningList.FirstOrDefault(t => t.Value == ReconParameter.Binning.Trim()) : BinningList[0];
            }
            ReconParameter.InterpType = ((int)reconModel.InterpType).ToString();
            SelectedInterpType = InterpTypeList.Any(t => t.Value == ReconParameter.InterpType.Trim()) ? InterpTypeList.FirstOrDefault(t => t.Key == int.Parse(ReconParameter.InterpType)) : InterpTypeList[0];
            ReconParameter.AirCorrectionMode = reconModel.AirCorrectionMode.ToString();
            SelectedAirCorrectionMode = AirCorrectionModeList.Any(t => t.Value == ReconParameter.AirCorrectionMode.Trim()) ? AirCorrectionModeList.FirstOrDefault(t => t.Value == ReconParameter.AirCorrectionMode.Trim()) : AirCorrectionModeList[0];           
            ReconParameter.ReconBodyPart = (reconModel.ReconBodyPart is null)? ReconBodyPartList[0].Value.ToString(): reconModel.ReconBodyPart;
            SelectedReconBodyPart = ReconBodyPartList.Any(t => t.Value == ReconParameter.ReconBodyPart.Trim()) ? ReconBodyPartList.FirstOrDefault(t => t.Value == ReconParameter.ReconBodyPart.Trim()) : ReconBodyPartList[0];
            //ReconParameter.Bowtie = reconModel.Bowtie.ToString();
            IsBowtie = ReconParameter.Bowtie == STRING_ONE;
            ReconParameter.IsRTD = reconModel.IsRTD.ToString();
            ReconParameter.IsHDRecon = reconModel.IsHDRecon;
            ReconParameter.IsWindMillEanble = reconModel.WindmillArtifactEnable;
            ReconParameter.IsConeAngleEanble = reconModel.ConeAngleArtifactEnable;
            ReconParameter.IsAutoRecon = reconModel.IsAutoRecon;


        }
    }
}

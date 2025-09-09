//using FellowOakDicom.Log;
using Microsoft.Extensions.Logging;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Language;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.ProtocolManagement.ViewModels.Common.Const;
using NV.CT.ProtocolManagement.ViewModels.Models;
using NV.CT.UI.ViewModel;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ProtocolParameterControlViewModel : BaseViewModel
    {
        #region private members
        private readonly IProtocolApplicationService _protocolApplicationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ProtocolParameterControlViewModel> _logger;
        private bool _currentProtocolIsEmergency = false;
        #endregion

        #region Properties
        private bool _isEnable;
        public bool IsEnable
        {
            get => _isEnable;
            set => SetProperty(ref _isEnable, value);
        }

        private ProtocolParameter _protocolParameter = new ProtocolParameter();
        public ProtocolParameter ProtocolParameter
        {
            get => _protocolParameter;
            set => SetProperty(ref _protocolParameter, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _patientPositionList;
        public ObservableCollection<KeyValuePair<int, string>> PatientPositionList
        {
            get => _patientPositionList;
            set => SetProperty(ref _patientPositionList, value);
        }

        private KeyValuePair<int, string> _selectedPatientPosition;
        public KeyValuePair<int, string> SelectedPatientPosition
        {
            get => _selectedPatientPosition;
            set => SetProperty(ref _selectedPatientPosition, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _bodySizeList;
        public ObservableCollection<KeyValuePair<int, string>> BodySizeList
        {
            get => _bodySizeList;
            set => SetProperty(ref _bodySizeList, value);
        }

        private KeyValuePair<int, string> _selectedBodySize;
        public KeyValuePair<int, string> SelectedBodySize
        {
            get => _selectedBodySize;
            set => SetProperty(ref _selectedBodySize, value);
        }

        private ObservableCollection<KeyValuePair<int, string>> _scanOptionList;
        public ObservableCollection<KeyValuePair<int, string>> ScanOptionList
        {
            get => _scanOptionList;
            set => SetProperty(ref _scanOptionList, value);
        }

        private KeyValuePair<int, string> _selectedScanOption;
        public KeyValuePair<int, string> SelectedScanOption
        {
            get => _selectedScanOption;
            set => SetProperty(ref _selectedScanOption, value);
        }

        private bool _protocolIsEnabled = false;
        public bool ProtocolIsEnabled
        {
            get => _protocolIsEnabled;
            set => SetProperty(ref _protocolIsEnabled, value);
        }

        #endregion

        #region Constructor
        public ProtocolParameterControlViewModel(IProtocolApplicationService protocolApplicationService, 
                                                 IDialogService dialogService,
                                                 ILogger<ProtocolParameterControlViewModel> logger)
        {
            _protocolApplicationService = protocolApplicationService;
            _dialogService = dialogService;
            _logger = logger;
            InitComboBoxItem();
            _protocolApplicationService.SelectBodyPartForProtocolChanged += SelectBodyPartForProtocolChange;
            _protocolApplicationService.ProtocolTreeSelectNodeChanged += TreeSelectChange;

            Commands.Add(Constants.COMMAND_SAVE, new DelegateCommand(OnSave));
        }

        #endregion

        private void InitComboBoxItem()
        {
            PatientPositionList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(PatientPosition));
            BodySizeList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(BodySize));
            ScanOptionList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(ScanOption));

            SelectedPatientPosition = PatientPositionList[0];
            SelectedBodySize = BodySizeList[0];
            SelectedScanOption = ScanOptionList[0];
        }

        [UIRoute]
        /// <summary>
        /// When the selection change event of the protocol tree occurs, initialize the data of the protocol parameters.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="nodeTypeNodeIdAndTemplateID"></param>
        private void TreeSelectChange(object? sender, EventArgs<(string NodeType, string NodeId, string TemplateId)> nodeTypeNodeIdAndTemplateID)
        {
            if (nodeTypeNodeIdAndTemplateID.Data.NodeType == ProtocolLayeredName.PROTOCOL_NODE)
            {
                InitGetProtocolParameter(nodeTypeNodeIdAndTemplateID.Data.TemplateId);
                IsEnable = true;
            }
            else
            {
                IsEnable = false;
            }
        }

        private void InitGetProtocolParameter(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                this._logger.LogWarning($"Invalid id of InitGetProtocolParameter.");
                return;
            }

            var protocol = _protocolApplicationService.GetProtocolTemplate(id);
            if (protocol is not null)
            {
                ProtocolModelToViewModel(protocol);
            }            
        }

        /// <summary>
        /// Listen to the Body Part selection change event to perform data refresh operation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="bodyPartName"></param>
        private void SelectBodyPartForProtocolChange(object? sender, EventArgs<string> bodyPartName)
        {
            if (bodyPartName.Data is null)
            {
                //this._logger.logDebug("bodyPartName.Data is null in SelectBodyPartForProtocolChange.");
                return;            
            }

            var fileModels = SelectNowBodyPartProtocol(bodyPartName.Data);
            if (fileModels.Count > 0 && fileModels[0] is not null)
            {
                Global.Instance.SelectTemplateID = fileModels[0].Descriptor.Id;
                IsEnable = true;
                ProtocolModelToViewModel(fileModels[0]);
            }
            else
            {
                IsEnable = false;
            }

        }

        [UIRoute]
        /// <summary>
        /// Save the results of this data modification operation for the protocol to an XML file.
        /// </summary>
        private void OnSave()
        {
            var template = _protocolApplicationService.GetProtocolTemplate(Global.Instance.SelectTemplateID);
            template.Protocol.Descriptor.Name = ProtocolParameter.ProtocolName;
            foreach (var protocolItemParameter in template.Protocol.Parameters)
            {
                switch (protocolItemParameter.Name)
                {
                    case nameof(ProtocolParameter.ProtocolName):
                        protocolItemParameter.Value = ProtocolParameter.ProtocolName;
                        break;
                    case ProtocolParameterNames.PROTOCOL_IS_EMERGENCY:
                        protocolItemParameter.Value = (bool.Parse(ProtocolParameter.IsEmergency) ? 1 : 0).ToString();
                        break;
                    case ProtocolParameterNames.PROTOCOL_IS_VALID:
                        protocolItemParameter.Value = (bool.Parse(ProtocolParameter.IsValid) ? 1 : 0).ToString();
                        break;
                    case ProtocolParameterNames.PROTOCOL_IS_FACTORY:
                        protocolItemParameter.Value = (bool.Parse(ProtocolParameter.IsFactory) ? 1 : 0).ToString();
                        break;
                    case ProtocolParameterNames.SCAN_OPTION:
                        protocolItemParameter.Value = SelectedScanOption.Value;
                        break;
                    case ProtocolParameterNames.BODY_SIZE:
                        protocolItemParameter.Value = SelectedBodySize.Value;
                        break;
                    case ProtocolParameterNames.PROTOCOL_FAMILY:
                        protocolItemParameter.Value = ProtocolParameter.ProtocolFamily;
                        break;
                    case ProtocolParameterNames.DESCRIPITON:
                        protocolItemParameter.Value = ProtocolParameter.Description;
                        break;
                    default:
                        break;
                }
            }

            if (template.Protocol.Children is not null && template.Protocol.Children.Count > 0)
            { 
                foreach(var child in template.Protocol.Children)
                {
                    var patientPosition = child.Parameters.FirstOrDefault(p => p.Name == ProtocolParameterNames.PATIENT_POSITION);
                    if (patientPosition is null) { continue; };

                    patientPosition.Value = SelectedPatientPosition.Value;
                } 
            }

            //添加急诊，更新急诊
            bool nowIsEmergency = bool.Parse(ProtocolParameter.IsEmergency);
            if (_currentProtocolIsEmergency != nowIsEmergency)
            {
                var allProtocol = _protocolApplicationService.GetAllProtocolTemplates();
                if (!nowIsEmergency)
                {
                    //取消所有急诊
                    var allEmergencys = allProtocol.Where(protocolTemplate => protocolTemplate.IsEmergency);
                    if (allEmergencys.Count() > 0)
                    {
                        foreach (var protocolTemplateModel in allEmergencys)
                        {
                            //protocolTemplateModel.Protocol.Parameters.FirstOrDefault(t => t.Name == ProtocolParameterNames.PROTOCOL_IS_EMERGENCY).Value = "0";
                            protocolTemplateModel.Protocol.IsEmergency = false;
                            _protocolApplicationService.Save(protocolTemplateModel);
                        }
                    }
                }
                else
                {
                    var protocolTemplateModel = allProtocol.FirstOrDefault(protocolTemplate => protocolTemplate.IsEmergency);
                    if (protocolTemplateModel is not null)
                    {
                        //var emergency = protocolTemplateModel.Protocol.Parameters.FirstOrDefault(t => t.Name == ProtocolParameterNames.PROTOCOL_IS_EMERGENCY);
                        //if (emergency is not null)
                        //{
                        //    emergency.Value = "1";
                        //    _protocolApplicationService.Save(protocolTemplateModel);
                        //}
                        protocolTemplateModel.Protocol.IsEmergency = false;
                        _protocolApplicationService.Save(protocolTemplateModel);
                    }
                }
            }
            _currentProtocolIsEmergency = nowIsEmergency;
            var message = string.Empty;
            try
            {
                template.Protocol.IsEmergency = _currentProtocolIsEmergency;
                _protocolApplicationService.Save(template);
                message = LanguageResource.Message_Saved_Successfully;
            }
            catch (Exception e)
            {
                this._logger.LogWarning($"Failed to save protocol for {template.FileName}, the exception is:{e.Message}");
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

        /// <summary>
        /// Filter protocols based on selected body parts to retrieve all protocols contained within the selected body part.
        /// </summary>
        /// <param name="bodyPartName">body part name</param>
        /// <returns></returns>
        private List<ProtocolTemplateModel> SelectNowBodyPartProtocol(string bodyPartName)
        {
            return _protocolApplicationService
            .GetAllProtocolTemplates()
            .FindAll(protocolTemplate => protocolTemplate.Protocol.Children.Count != 0 &&
                protocolTemplate.Protocol.Parameters
                .FirstOrDefault(arg => arg.Name == ProtocolParameterNames.BODY_PART && arg.Value == bodyPartName) is not null); ;
        }

        private void ProtocolModelToViewModel(ProtocolTemplateModel protocolTemplate)
        {
            //如果开发环境开关打开，则始终允许修改; 否则检查是否是出厂协议   
            ProtocolIsEnabled = RuntimeConfig.IsDevelopment ? true : !protocolTemplate.Protocol.IsFactory;

            ProtocolParameter.ProtocolName = protocolTemplate.Protocol.Descriptor.Name.ToString();
            var selectedPatientPosition = protocolTemplate.Protocol.Children[0].PatientPosition.ToString();
            SelectedPatientPosition = PatientPositionList.FirstOrDefault(n => n.Value == selectedPatientPosition);

            SelectedScanOption = ScanOptionList.FirstOrDefault(n => n.Value == ProtocolParameter.ScanOption);

            ProtocolParameter.IsEmergency = protocolTemplate.Protocol.IsEmergency.ToString();
            _currentProtocolIsEmergency = protocolTemplate.Protocol.IsEmergency;

            ProtocolParameter.IsValid = protocolTemplate.Protocol.IsValid.ToString();
            //遍历protocolTemplate.Protocol下的所有扫描参数，如果有增强，将协议里的增强设置为真，如没有，这里增强参数为假
            bool isEnhanced = protocolTemplate.Protocol.Children.SelectMany(f => f.Children).SelectMany(m => m.Children).Any(t => t.IsEnhanced);
            ProtocolParameter.IsEnhanced = isEnhanced.ToString();
            ProtocolParameter.BodySize = protocolTemplate.Protocol.BodySize.ToString();
            SelectedBodySize = BodySizeList.FirstOrDefault(n => n.Value == ProtocolParameter.BodySize);
            ProtocolParameter.ProtocolFamily = string.IsNullOrEmpty(protocolTemplate.Protocol.ProtocolFamily)? string.Empty: protocolTemplate.Protocol.ProtocolFamily;
            ProtocolParameter.IsFactory = protocolTemplate.Protocol.IsFactory.ToString();
            ProtocolParameter.Description = protocolTemplate.Protocol.Description;

        }
    }
}

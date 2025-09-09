//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using PatientPosition = NV.CT.FacadeProxy.Common.Enums.PatientPosition;
using BodyPart = NV.CT.CTS.Enums.BodyPart;
using System.Collections.Generic;
using NV.CT.Protocol.Models;
using Google.Protobuf.WellKnownTypes;
using System.Xml.Linq;

namespace NV.CT.UI.Exam.ViewModel;

public class ProtocolSaveAsViewModel : BaseViewModel
{
    private readonly IProtocolHostService _protocolHostService;
    private readonly IProtocolModificationService _protocolModificationService;
    private readonly IProtocolOperation _protocolOperation;
    private readonly IDialogService _dialogService;
    ProtocolTemplateModel? protocolTemplateModel;
    private bool IsUIChange = true;

    public ProtocolSaveAsViewModel(IProtocolHostService protocolHostService,
        IProtocolModificationService protocolModificationService,
        IProtocolOperation protocolOperation,
        IDialogService dialogService)
    {
        _dialogService = dialogService;
        _protocolHostService = protocolHostService;
        InitComboBoxItem();
        _protocolModificationService = protocolModificationService;
        _protocolOperation = protocolOperation;

        Commands.Add(CommandParameters.COMMAND_SAVE, new DelegateCommand<object>(Saved, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
    }

    public void InitProtocol()
    {
        IsUIChange = false;
        IsTop = false;
        protocolTemplateModel = new ProtocolTemplateModel { Protocol = _protocolHostService.Instance.Clone() };
        InitProtocolModel();
        SetParameter(protocolTemplateModel.Protocol);
        IsUIChange = true;
    }

    private void InitProtocolModel()
    {
        if (protocolTemplateModel is null)
        {
            return;
        }
        protocolTemplateModel.Protocol.Descriptor.Id = Guid.NewGuid().ToString();
        protocolTemplateModel.Protocol.Descriptor.Name = $"{protocolTemplateModel.Protocol.Descriptor.Name} Copy";
        protocolTemplateModel.Protocol.Status = PerformStatus.Unperform;
        protocolTemplateModel.Protocol.FailureReason = FailureReasonType.None;
        protocolTemplateModel.Descriptor = new DescriptorModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = protocolTemplateModel.Protocol.Descriptor.Name,
            Type = nameof(ProtocolTemplateModel)
        };
        _protocolModificationService.SetParameters(protocolTemplateModel.Protocol, new List<ParameterModel> {
            new ParameterModel
            {
                Name = ProtocolParameterNames.PROTOCOL_IS_FACTORY,
                Value = "0"
            }
        });
        protocolTemplateModel.Protocol.Children.ForEach(frame =>
        {
            frame.Descriptor.Id = Guid.NewGuid().ToString();
            frame.Status = PerformStatus.Unperform;
            frame.FailureReason = FailureReasonType.None;
            frame.Children.ForEach(measurement =>
            {
                measurement.Descriptor.Id = Guid.NewGuid().ToString();
                measurement.Status = PerformStatus.Unperform;
                measurement.FailureReason = FailureReasonType.None;
                measurement.Children.ForEach(scan =>
                {
                    scan.Descriptor.Id = Guid.NewGuid().ToString();
                    scan.Status = PerformStatus.Unperform;
                    scan.FailureReason = FailureReasonType.None;
                    scan.Children.ForEach(recon =>
                    {
                        recon.Descriptor.Id = Guid.NewGuid().ToString();
                        recon.Status = PerformStatus.Unperform;
                        recon.FailureReason = FailureReasonType.None;
                        var reconImageParameter = recon.Parameters.FirstOrDefault(p => p.Name == ProtocolParameterNames.RECON_IMAGE_PATH);
                        if (reconImageParameter is not null)
                        {
                            reconImageParameter.Value = string.Empty;
                        }
                    });
                });
            });
        });
    }

    private void SetParameter(ProtocolModel protocolModel)
    {
        ProtocolName = protocolModel.Descriptor.Name;
        SelectBodyPart = BodyPartList.FirstOrDefault(t => t.Key == (int)protocolModel.BodyPart);
        SelectPatientPosition = PatientPositionList.FirstOrDefault(t => t.Key == (int)protocolModel.Children[0].PatientPosition);
        if (protocolModel.IsAdult)
        {
            SelectBodySize = BodySizeList.FirstOrDefault(t => t.Key == (int)BodySize.Adult);
        }
        else
        {
            SelectBodySize = BodySizeList.FirstOrDefault(t => t.Key == (int)BodySize.Child);
        }
        IsEnhanced = protocolModel.IsEnhanced;
        IsEmergency = false;
        IsTabletSuitable = protocolModel.IsTabletSuitable;
        ProtocolFamily = protocolModel.ProtocolFamily;
        Description = protocolModel.Description;
        if (protocolModel.IsEmergency)
        {
            _protocolModificationService.SetParameters(protocolModel, new List<ParameterModel> {
                new ParameterModel
                {
                    Name = ProtocolParameterNames.PROTOCOL_IS_EMERGENCY,
                    Value = false.ToString(),
                }
            });
        }
    }

    private void InitComboBoxItem()
    {
        BodyPartList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(BodyPart));
        PatientPositionList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(PatientPosition));
        BodySizeList = NV.CT.UI.Controls.Extensions.EnumExtension.EnumToList(typeof(BodySize));
    }

    private string _protocolName = string.Empty;
    public string ProtocolName
    {
        get => _protocolName;
        set
        {
            if (SetProperty(ref _protocolName, value) && IsUIChange && protocolTemplateModel is not null)
            {
                protocolTemplateModel.Protocol.Descriptor.Name = value;
            }
        }
    }

    private ObservableCollection<KeyValuePair<int, string>> _bodyPartList = new();
    public ObservableCollection<KeyValuePair<int, string>> BodyPartList
    {
        get => _bodyPartList;
        set => SetProperty(ref _bodyPartList, value);
    }

    private KeyValuePair<int, string> _selectBodyPart;
    public KeyValuePair<int, string> SelectBodyPart
    {
        get => _selectBodyPart;
        set
        {
            if (SetProperty(ref _selectBodyPart, value) && IsUIChange)
            {
                SaveParameterModel(ProtocolParameterNames.BODY_PART, value.Value);
            }
        }
    }

    private ObservableCollection<KeyValuePair<int, string>> _patientPositionList = new();
    public ObservableCollection<KeyValuePair<int, string>> PatientPositionList
    {
        get => _patientPositionList;
        set => SetProperty(ref _patientPositionList, value);
    }

    private KeyValuePair<int, string> _selectPatientPosition;
    public KeyValuePair<int, string> SelectPatientPosition
    {
        get => _selectPatientPosition;
        set
        {
            if (SetProperty(ref _selectPatientPosition, value) && IsUIChange)
            {
                SaveParameterModel(ProtocolParameterNames.PATIENT_POSITION, value.Value);
            }
        }
    }

    private ObservableCollection<KeyValuePair<int, string>> _bodySizeList = new();
    public ObservableCollection<KeyValuePair<int, string>> BodySizeList
    {
        get => _bodySizeList;
        set => SetProperty(ref _bodySizeList, value);
    }

    private KeyValuePair<int, string> _selectBodySize;
    public KeyValuePair<int, string> SelectBodySize
    {
        get => _selectBodySize;
        set
        {
            if (SetProperty(ref _selectBodySize, value) && IsUIChange)
            {
                SaveParameterModel(ProtocolParameterNames.BODY_SIZE, value.Value);
            }
        }
    }

    private string _protocolFamily = string.Empty;
    public string ProtocolFamily
    {
        get => _protocolFamily;
        set
        {
            if (SetProperty(ref _protocolFamily, value) && IsUIChange)
            {
                SaveParameterModel(ProtocolParameterNames.PROTOCOL_FAMILY, value);
            }
        }
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value) && IsUIChange)
            {
                SaveParameterModel(ProtocolParameterNames.DESCRIPITON, value);
            }
        }
    }

    private bool _isEnhanced = true;
    public bool IsEnhanced
    {
        get => _isEnhanced;
        set
        {
            SetProperty(ref _isEnhanced, value);
        }
    }

    private bool _isTabletSuitable = true;
    public bool IsTabletSuitable
    {
        get => _isTabletSuitable;
        set
        {
            if (SetProperty(ref _isTabletSuitable, value) && IsUIChange)
            {
                SaveParameterModel(ProtocolParameterNames.PROTOCOL_TABLET_SUITABLE, value.ToString());
            }
        }
    }

    private bool _isEmergency = true;
    public bool IsEmergency
    {
        get => _isEmergency;
        set
        {
            SetProperty(ref _isEmergency, value);
		}
    }

    private void SaveParameterModel(string name, string value)
    {
        if (!string.IsNullOrEmpty(name) && protocolTemplateModel is not null)
        {
            _protocolModificationService.SetParameters(protocolTemplateModel.Protocol, new List<ParameterModel> {
                new ParameterModel
                {
                    Name = name,
                    Value =value
                }
            });
        }
    }

    private bool _isTop = false;
    public bool IsTop
    {
        get => _isTop;
        set
        {
            SetProperty(ref _isTop, value);
        }
    }

    public void Saved(object parameter)
    {
        if (parameter is Window window)
        {
            if (string.IsNullOrEmpty(ProtocolName))
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_ProtocolNameNotEmpty, arg => { }, new WindowInteropHelper(window).Handle);
                return;
            }
            List<ProtocolTemplateModel> list = _protocolOperation.GetAllProtocolTemplates();
            bool exist = list.Any(t => t.Descriptor.Name.Equals(ProtocolName) && (int)t.BodyPart == SelectBodyPart.Key);
            if (exist)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_ProtocolNameAlreadyExists, arg => { }, new WindowInteropHelper(window).Handle);
                return;
            }
            if (protocolTemplateModel is not null)
            {
                var models = new List<ParameterModel> {        //设置为非出厂协议
                    new ParameterModel
                    {
                        Name = ProtocolParameterNames.PROTOCOL_IS_FACTORY,
                        Value = false.ToString(),
                    }
                };
                if (IsEmergency)    //判断是否急诊协议
                {
                    _dialogService.ShowDialog(true, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_ProtocolSaveAsEmergency, arg =>
                    {
                        if (arg.Result == ButtonResult.OK)
                        {
                            var model = _protocolOperation.GetAllProtocolTemplates().FirstOrDefault(t => t.IsEmergency);
                            if (model is not null)
                            {
                                model.Protocol.IsEmergency = false;
                                _protocolOperation.Save(model, IsTop);
                            }
                        }
                    }, new WindowInteropHelper(window).Handle);
                }
                protocolTemplateModel.Protocol.IsEmergency = IsEmergency;
                _protocolModificationService.SetParameters(protocolTemplateModel.Protocol, models);
                _protocolOperation.Save(protocolTemplateModel, IsTop);
            }
            window.Hide();
        }
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.Language;

namespace NV.CT.ConfigManagement.ViewModel;

public class EquipmentViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<EquipmentViewModel> _logger;

    private string _equipmentModelNo = string.Empty;
    public string EquipmentModelNo
    {
        get => _equipmentModelNo;
        set => SetProperty(ref _equipmentModelNo, value);
    }

    private string _equipmentSN = string.Empty;
    public string EquipmentSN
    {
        get => _equipmentSN;
        set => SetProperty(ref _equipmentSN, value);
    }

    private string _moduleSN = string.Empty;
    public string ModuleSN
    {
        get => _moduleSN;
        set => SetProperty(ref _moduleSN, value);
    }

    private string _softwareName = string.Empty;
    public string SoftwareName
    {
        get => _softwareName;
        set => SetProperty(ref _softwareName, value);
    }

    private string _softwareVersionNo = string.Empty;
    public string SoftwareVersionNo
    {
        get => _softwareVersionNo;
        set => SetProperty(ref _softwareVersionNo, value);
    }

    private string _softwareDetailVersionNo = string.Empty;
    public string SoftwareDetailVersionNo
    {
        get => _softwareDetailVersionNo;
        set => SetProperty(ref _softwareDetailVersionNo, value);
    }

    private string _deviceSystemVersion = string.Empty;
    public string DeviceSystemVersion
    {
        get => _deviceSystemVersion;
        set => SetProperty(ref _deviceSystemVersion, value);
    }

    private string _computerSystemVersion = string.Empty;
    public string ComputerSystemVersion
    {
        get => _computerSystemVersion;
        set => SetProperty(ref _computerSystemVersion, value);
    }

    public EquipmentViewModel(
        IDialogService dialogService,
        ILogger<EquipmentViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        EquipmentSettingInfo node = UserConfig.EquipmentSettingConfig.EquipmentSetting;
        EquipmentModelNo = node.ManufacturerModelName.Value;
        EquipmentSN = node.DeviceSerialNumber.Value;
        ModuleSN = node.PhantomSeriealNumber.Value;
        SoftwareName = node.SoftwareName.Value;
        SoftwareVersionNo = node.SoftwareVersion.Value;
        SoftwareDetailVersionNo = node.SoftwareVersionDetail.Value;
        DeviceSystemVersion = node.SystemVersion;
        ComputerSystemVersion = node.OperatingSystemVersion.Value;
    }

    public void Saved()
    {
        EquipmentSettingInfo node = UserConfig.EquipmentSettingConfig.EquipmentSetting;
        node.ManufacturerModelName.Value = EquipmentModelNo;
        node.DeviceSerialNumber.Value = EquipmentSN;
        node.PhantomSeriealNumber.Value = ModuleSN;
        node.SoftwareName.Value = SoftwareName;
        node.SoftwareVersion.Value = SoftwareVersionNo;
        node.SoftwareVersionDetail.Value = SoftwareDetailVersionNo;
        node.SystemVersion = DeviceSystemVersion;
        node.OperatingSystemVersion.Value = ComputerSystemVersion;

        UserConfig.EquipmentSettingConfig.EquipmentSetting = node;
        bool saveFlag = UserConfig.SaveEquipmentSetting();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
           arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
}
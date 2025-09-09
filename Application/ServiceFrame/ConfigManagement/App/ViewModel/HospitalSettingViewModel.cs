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

public class HospitalSettingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<HospitalSettingViewModel> _logger;

    private string _manufacturer = string.Empty;
    public string Manufacturer
    {
        get => _manufacturer;
        set => SetProperty(ref _manufacturer, value);
    }

    private string _institutionName = string.Empty;
    public string InstitutionName
    {
        get => _institutionName;
        set => SetProperty(ref _institutionName, value);
    }

    private string _institutionAddress = string.Empty;
    public string InstitutionAddress
    {
        get => _institutionAddress;
        set => SetProperty(ref _institutionAddress, value);
    }

    private string _departmentName = string.Empty;
    public string DepartmentName
    {
        get => _departmentName;
        set => SetProperty(ref _departmentName, value);
    }

    private string _phoneNum = string.Empty;
    public string PhoneNum
    {
        get => _phoneNum;
        set => SetProperty(ref _phoneNum, value);
    }

    private string _institutionZip = string.Empty;
    public string InstitutionZip
    {
        get => _institutionZip;
        set => SetProperty(ref _institutionZip, value);
    }

    private string _city = string.Empty;
    public string City
    {
        get => _city;
        set => SetProperty(ref _city, value);
    }

    private string _country = string.Empty;
    public string Country
    {
        get => _country;
        set => SetProperty(ref _country, value);
    }

    public HospitalSettingViewModel(
        IDialogService dialogService,
        ILogger<HospitalSettingViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        HospitalSettingInfo node = UserConfig.HospitalSettingConfig.HospitalSetting;
        Manufacturer = node.Manufacturer.Value;
        InstitutionName = node.InstitutionName.Value;
        InstitutionAddress = node.InstitutionAddress.Value;
        DepartmentName = node.InstitutionDepartmentName.Value;
        PhoneNum = node.InstitutionPhoneNum.Value;
        InstitutionZip = node.InstitutionZip.Value;
        City = node.City.Value;
        Country = node.Country.Value;
    }

    public void Saved()
    {
        HospitalSettingInfo node = UserConfig.HospitalSettingConfig.HospitalSetting;
        node.Manufacturer.Value = Manufacturer;
        node.InstitutionName.Value = InstitutionName;
        node.InstitutionAddress.Value = InstitutionAddress;
        node.InstitutionDepartmentName.Value = DepartmentName;
        node.InstitutionPhoneNum.Value = PhoneNum;
        node.InstitutionZip.Value = InstitutionZip;
        node.City.Value = City;
        node.Country.Value = Country;

        UserConfig.HospitalSettingConfig.HospitalSetting = node;
        bool saveFlag = UserConfig.SaveHospitalSetting();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
         arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
}
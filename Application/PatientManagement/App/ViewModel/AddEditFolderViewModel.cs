//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="AddEditFolderViewModel.cs" company="纳米维景">
// 版权所有 (C)2023,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

using NV.CT.Language;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NV.CT.PatientManagement.ViewModel;

public class AddEditFolderViewModel : BaseViewModel, INotifyDataErrorInfo
{
    private readonly Func<string> _funcValidateFolderName;
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private readonly IDialogService _dialogService;

    private bool _isAddMode = true;
    public bool IsAddMode
    {
        get { return this._isAddMode; }
        set
        {
            if (value != _isAddMode)
            {
                _isAddMode = value;                
            }

            if (_isAddMode)
            {
                WindowTitle = LanguageResource.Content_AddSubfolder;
            }
            else
            {
                WindowTitle = LanguageResource.Content_RenameFolder;
            }
        }
    }

    private string _windowTitle = string.Empty;
    public string WindowTitle
    {
        get { return this._windowTitle; }
        private set
        {
            SetProperty(ref this._windowTitle, value);
        }
    }

    private string _folderName = string.Empty;
    public string FolderName
    {
        get { return this._folderName; }
        set
        {
            SetProperty(ref this._folderName, value);
            Validate(_funcValidateFolderName, nameof(FolderName));
        }
    }

    public bool IsResultOK { get; private set; }

    public bool IsFolderNameChanged { get; private set; }

    private bool _isSaveEnabled = false;
    public bool IsSaveEnabled
    {
        get { return this._isSaveEnabled; }
        set
        {
            this.SetProperty(ref this._isSaveEnabled, value);
        }
    }

    private readonly IDictionary<string, IList<string>> _errors = new Dictionary<string, IList<string>>();
    public bool HasErrors
    {
        get => this._errors.Count != 0;
    }

    public AddEditFolderViewModel(IDialogService dialogService)
    {
        IsResultOK = false;
        this._dialogService = dialogService;
        this._funcValidateFolderName = () =>
        {
            if (string.IsNullOrEmpty(FolderName))
            {
                return LanguageResource.Messge_Error_RequireFolderName;
            }

            return string.Empty;
        };


        Commands.Add("OKCommand", new DelegateCommand<object>(OnOKClicked));
        Commands.Add("CloseCommand", new DelegateCommand<object>(OnCloseed, _ => true));        
    }

    public void OnOKClicked(object parameter)
    {
        if (string.IsNullOrEmpty(FolderName.Trim()))
        {
            _dialogService.Show(false, MessageLeveles.Error, 
                                LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Content_NotAllowedEmptyString, null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var legalCharacter = "^[.0-9a-zA-Z\u4E00-\u9FA5_-]+$";
        var reg = new Regex(legalCharacter);
        if (!reg.IsMatch(FolderName))
        {
            _dialogService.Show(false, MessageLeveles.Error, 
                LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Content_NotAllowedIllegalCharacters, null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        IsResultOK = true;
        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    public void OnCloseed(object parameter)
    {
        IsResultOK = false;
        this.ClearMessage();

        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    private void ClearMessage()
    {
        this.FolderName = string.Empty;
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return "";
        }

        if (this._errors.ContainsKey(propertyName))
        {
            return this._errors[propertyName];
        }
        else
        {
            return "";
        }

    }

    public void Validate(Func<string> valid, [CallerMemberName] string prop = "")
    {
        string error = valid();
        this._errors.Remove(prop);

        if (!string.IsNullOrEmpty(error))
        {
            this._errors.Add(prop, new[] {error});
        }

        this.ResetEnableStatus();
    }

    private void ResetEnableStatus()
    {
        IsSaveEnabled = !HasErrors;
    }
}
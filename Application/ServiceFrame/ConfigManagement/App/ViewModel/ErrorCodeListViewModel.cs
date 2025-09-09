//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:59    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.SystemConfig;

namespace NV.CT.ConfigManagement.ViewModel;

public class ErrorCodeListViewModel : BaseViewModel
{
    private readonly IErrorCodeService _errorCodeService;
    private ObservableCollection<ErrorInfo> _errorCodeList = new ObservableCollection<ErrorInfo>();
    public ObservableCollection<ErrorInfo> ErrorCodeList
    {
        get => _errorCodeList;
        set => SetProperty(ref _errorCodeList, value);
    }

    private string lastSearchText = string.Empty;
    public string LastSearchText
    {
        get => lastSearchText;
        set => SetProperty(ref lastSearchText, value);
    }

    public ErrorCodeListViewModel(IErrorCodeService errorCodeService)
    {
        _errorCodeService = errorCodeService;

        Commands.Add("SearchCommand", new DelegateCommand<string>(SearchUserList));
        SearchUserList(string.Empty);
    }

    public void SearchUserList(string searchText)
    {
        LastSearchText = searchText;
        ErrorCodeList.Clear();
        ErrorConfig errorConfig = _errorCodeService.GetConfigs();
        if (errorConfig is null)
        {
            return;
        }
        List<ErrorInfo> errorInfoModels = errorConfig.Errors;
        var list = errorInfoModels.ToObservableCollection();
        if (!string.IsNullOrEmpty(searchText))
        {
            list = errorInfoModels.FindAll(t => t.Code.ToLower().Contains(searchText.ToLower()) || (t.Module).ToLower().Contains(searchText.ToLower()) || (t.Level).ToLower().Contains(searchText.ToLower())).ToObservableCollection();
        }
        foreach (var item in list)
        {
            if (!string.IsNullOrEmpty(item.Module))
            {
                item.Module = item.Module.Replace("\n", " ");
            }
            if (!string.IsNullOrEmpty(item.Solution))
            {
                item.Solution = item.Solution.Replace("\n", " ");
            }
            if (!string.IsNullOrEmpty(item.Description))
            {
                item.Description = item.Description.Replace("\n", " ");
            }
            ErrorCodeList.Add(item);
        }
    }
}
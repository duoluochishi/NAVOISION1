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
using System.Collections.Generic;

namespace NV.CT.ConfigManagement.ViewModel;

public class BaseTabletViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _ip = string.Empty;
    public string IP
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }

    private string _sn = string.Empty;
    public string SN
    {
        get => _sn;
        set => SetProperty(ref _sn, value);
    }
}
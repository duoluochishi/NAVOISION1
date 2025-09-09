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

namespace NV.CT.ConfigManagement.ViewModel;

public class BaseArchiveNodeViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }   

    private int _port = 0;
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    private string _ip = string.Empty;
    public string IP
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }

    private string _serverAE = string.Empty;
    public string ServerAE
    {
        get => _serverAE;
        set => SetProperty(ref _serverAE, value);
    }

    private string _clientAE = string.Empty;
    public string ClientAE
    {
        get => _clientAE;
        set => SetProperty(ref _clientAE, value);
    }

    private string _remark = string.Empty;
    public string Remark
    {
        get => _remark;
        set => SetProperty(ref _remark, value);
    }

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }
}
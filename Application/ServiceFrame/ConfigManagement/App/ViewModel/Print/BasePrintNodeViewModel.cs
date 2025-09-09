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
namespace NV.CT.ConfigManagement.ViewModel;

public class BasePrintNodeViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _serverName = string.Empty;
    public string ServerName
    {
        get => _serverName;
        set => SetProperty(ref _serverName, value);
    }

    private string _ip = string.Empty;
    public string IP
    {
        get => _ip;
        set => SetProperty(ref _ip, value);
    }

    private int _port = 0;
    public int Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    private string _serverAE = string.Empty;
    public string ServerAE
    {
        get => _serverAE;
        set => SetProperty(ref _serverAE, value);
    }

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    private string _layout = string.Empty;
    public string Layout
    {
        get => _layout;
        set => SetProperty(ref _layout, value);
    }

    private string _aECaller = string.Empty;
    public string AECaller
    {
        get => _aECaller;
        set => SetProperty(ref _aECaller, value);
    }

    private int _resolution = 0;
    public int Resolution
    {
        get => _resolution;
        set => SetProperty(ref _resolution, value);
    }

    private DateTime _createTime = DateTime.Now;
    public DateTime CreateTime
    {
        get => _createTime;
        set => SetProperty(ref _createTime, value);
    }

    private string _creator = string.Empty;
    public string Creator
    {
        get => _creator;
        set => SetProperty(ref _creator, value);
    }

    private bool _isUsing = true;
    public bool IsUsing
    {
        get => _isUsing;
        set => SetProperty(ref _isUsing, value);
    }

    private string _paperSize = string.Empty;
    public string PaperSize
    {
        get => _paperSize;
        set => SetProperty(ref _paperSize, value);
    }

    private string _remark = string.Empty;
    public string Remark
    {
        get => _remark;
        set => SetProperty(ref _remark, value);
    }
}
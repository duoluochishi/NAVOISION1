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
using NV.CT.ConfigManagement.ApplicationService.Contract;

namespace NV.CT.ConfigManagement.ViewModel;

public class BasePrintProtocolViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private BodyPart _bodyPart = BodyPart.Abdomen;
    public BodyPart BodyPart
    {
        get => _bodyPart;
        set => SetProperty(ref _bodyPart, value);
    }

    public string BodyPartDisplay
    {
        get => BodyPart.ToString();
    }

    private int _row = 5;
    public int Row
    {
        get => _row;
        set
        {
            SetProperty(ref _row, value);
            _printProtocolApplicationService.RowClomunchange();
        }
    }

    private int _column = 5;
    public int Column
    {
        get => _column;
        set
        {
            SetProperty(ref _column, value);
            _printProtocolApplicationService.RowClomunchange();
        }
    }

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }

    private bool _isSystem = false;
    public bool IsSystem
    {
        get => _isSystem;
        set => SetProperty(ref _isSystem, value);
    }

    private readonly IPrintProtocolApplicationService _printProtocolApplicationService;
    public BasePrintProtocolViewModel()
    {
        _printProtocolApplicationService = CTS.Global.ServiceProvider?.GetRequiredService<IPrintProtocolApplicationService>(); ;
    }
}
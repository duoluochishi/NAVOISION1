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
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Models.UserConfig;
using System.Collections.Generic;
using NV.CT.Language;

namespace NV.CT.ConfigManagement.ViewModel;

public class PrintProtocolViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IPrintProtocolApplicationService _printProtocolApplicationService;
    private readonly ILogger<PrintProtocolViewModel> _logger;

    private BasePrintProtocolViewModel _currentNode = new BasePrintProtocolViewModel();
    public BasePrintProtocolViewModel CurrentNode
    {
        get => _currentNode;
        set
        {
            if (SetProperty(ref _currentNode, value))
            {
                SetColorOfCells();
            }
        }
    }

    private ObservableCollection<KeyValuePair<string, string>> _bodyPartlist = new ObservableCollection<KeyValuePair<string, string>>();
    public ObservableCollection<KeyValuePair<string, string>> BodyPartList
    {
        get => _bodyPartlist;
        set => SetProperty(ref _bodyPartlist, value);
    }

    private ObservableCollection<CellViewModel> _cellList = new ObservableCollection<CellViewModel>();
    public ObservableCollection<CellViewModel> CellList
    {
        get
        {
            return this._cellList;
        }
        set
        {
            SetProperty(ref _cellList, value);
        }
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public PrintProtocolViewModel(IPrintProtocolApplicationService printProtocolApplicationService,
        IDialogService dialogService,
        ILogger<PrintProtocolViewModel> logger)
    {
        _dialogService = dialogService;
        _printProtocolApplicationService = printProtocolApplicationService;
        _logger = logger;
        InitBodyPartList();
        InitCellList();
        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
        _printProtocolApplicationService.Changed += PrintProtocolApplicationService_Changed;

        _printProtocolApplicationService.RowColmunChanged += PrintProtocolApplicationService_RowColmunChanged;
    }

    private void PrintProtocolApplicationService_RowColmunChanged(object? sender, EventArgs e)
    {
        SetColorOfCells();
    }

    [UIRoute]
    private void PrintProtocolApplicationService_Changed(object? sender, EventArgs<(OperationType operation, PrintProtocol printProtocol)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetNodeInfo(e.Data.printProtocol);
    }

    private void SetNodeInfo(PrintProtocol printProtocol)
    {
        CurrentNode = new BasePrintProtocolViewModel();
        CurrentNode.ID = printProtocol.Id;
        CurrentNode.Name = printProtocol.Name;
        CurrentNode.IsSystem = printProtocol.IsSystem;
        CurrentNode.BodyPart = Enum.Parse<BodyPart>(printProtocol.BodyPart);
        CurrentNode.Row = printProtocol.Row;
        CurrentNode.Column = printProtocol.Column;
        CurrentNode.IsDefault = printProtocol.IsDefault;

        SetColorOfCells();
    }

    private void InitBodyPartList()
    {
        foreach (var enumItem in Enum.GetValues(typeof(BodyPart)))
        {
            if (enumItem is not null)
            {
                BodyPartList.Add(new KeyValuePair<string, string>(enumItem.ToString(), enumItem.ToString()));
            }
        }
    }

    private void InitCellList()
    {
        CellList.Clear();
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                CellList.Add(new CellViewModel() { ID = (i * 10 + j).ToString(), IsEnable = false.ToString(), RowNumber = i, ColumnNumber = j });
            }
        }
    }

    private void SetColorOfCells()
    {
        var coveredCells = CellList.Where(c => c.RowNumber < CurrentNode.Row && c.ColumnNumber < CurrentNode.Column).ToArray();
        var uncoveredCells = CellList.Where(c => c.RowNumber >= CurrentNode.Row || c.ColumnNumber >= CurrentNode.Column).ToArray();
        foreach (CellViewModel coveredCell in coveredCells)
        {
            coveredCell.IsEnable = true.ToString();
        }

        foreach (CellViewModel uncoveredCell in uncoveredCells)
        {
            uncoveredCell.IsEnable = false.ToString();
        }
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty())
        {
            return;
        }
        PrintProtocol info = new PrintProtocol()
        {
            Id = CurrentNode.ID,
            Name = CurrentNode.Name,
            IsSystem = CurrentNode.IsSystem,
            BodyPart = CurrentNode.BodyPart.ToString(),
            Row = CurrentNode.Row,
            Column = CurrentNode.Column,
            IsDefault = CurrentNode.IsDefault
        };

        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _printProtocolApplicationService.Add(info);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _printProtocolApplicationService.Update(info);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _printProtocolApplicationService.Reload();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentNode.Name))
        {
            sb.Append(string.Format(message, "Name"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _printProtocolApplicationService.Reload();
            window.Hide();
        }
    }
}
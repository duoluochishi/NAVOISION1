//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/30 11:01:27     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.PatientManagement.Models;

namespace NV.CT.PatientManagement.ViewModel;

public class StudyListColumnsConfigViewModel : BaseViewModel
{
    private readonly IMapper _mapper;
    private readonly ILogger<StudyListColumnsConfigViewModel> _logger;
    private readonly IStudyListColumnsConfigService _columnsConfigService;
    private StudyListColumnsConfig _columnsConfig;
    private StudyListColumn _previousSelectedColumn = StudyListColumn.PatientName;//记住上次选择的项目

    private bool _isUpEnabled = false;
    public bool IsUpEnabled
    {
        get
        {
            return _isUpEnabled;
        }
        set
        {
            this.SetProperty(ref _isUpEnabled, value);
        }
    }

    private bool _isDownEnabled = true;
    public bool IsDownEnabled
    {
        get
        {
            return _isDownEnabled;
        }
        set
        {
            this.SetProperty(ref _isDownEnabled, value);
        }
    }

    private ObservableCollection<ColumnItemModel> _columnItemList;
    public ObservableCollection<ColumnItemModel> ColumnItemList
    {
        get
        {
            return _columnItemList;
        }
        set
        {
            this.SetProperty(ref _columnItemList, value);
        }
    }

    private ColumnItemModel? _selectedColumnItem;
    public ColumnItemModel? SelectedColumnItem
    {
        get
        {
            return _selectedColumnItem;
        }
        set
        {
            this.SetProperty(ref _selectedColumnItem, value);
            this.RefreshButtonsState();
        }
    }

    public StudyListColumnsConfigViewModel(IMapper mapper,
                                           ILogger<StudyListColumnsConfigViewModel> logger,
                                           IStudyListColumnsConfigService columnsConfigService)
    {
        _mapper = mapper;
        _logger = logger;
        _columnsConfigService = columnsConfigService;

        Commands.Add(PatientManagementConstants.COMMAND_MOVE_UP, new DelegateCommand(OnMoveUp));
        Commands.Add(PatientManagementConstants.COMMAND_MOVE_DOWN, new DelegateCommand(OnMoveDown));
        Commands.Add(PatientManagementConstants.COMMAND_SAVE, new DelegateCommand(OnSave));
        Commands.Add(PatientManagementConstants.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        Commands.Add(PatientManagementConstants.COMMAND_CLICK_COLUMN_CONFIG, new DelegateCommand<object>(OnClickColumnConfig));

        this.Initialize();
    }

    public void Initialize()
    {
        _columnsConfig = _columnsConfigService.GetConfigs();
        ColumnItemList = _mapper.Map<List<ColumnItemModel>>(_columnsConfig.ColumnItems.Items.OrderBy(c => c.SortNumber).ToList()).ToObservableCollection();
        SelectedColumnItem = ColumnItemList.FirstOrDefault();
        _previousSelectedColumn = SelectedColumnItem.ItemName;
    }

    private void OnClickColumnConfig(object columnItemName)
    {
        var columnItem = Enum.Parse<StudyListColumn>(columnItemName.ToString(), true); 
        var foundItem = ColumnItemList.FirstOrDefault(c => c.ItemName == columnItem);
        if (foundItem is not null)
        {
            //当首次选择该项时，保持原选择状态
            if (_previousSelectedColumn != foundItem.ItemName)
            {
                _previousSelectedColumn = foundItem.ItemName;
                foundItem.IsChecked = (!foundItem.IsChecked);                
            }
            SelectedColumnItem = foundItem;
        }
    }

    private void OnMoveUp()
    {
        if (SelectedColumnItem is null)
        {
            return;
        }

        int index = ColumnItemList.IndexOf(SelectedColumnItem);
        if (index > 0)
        {
            int previousItemIndex = index - 1;
            var targetItem = ColumnItemList[previousItemIndex];

            //swap index
            int sortNumber = targetItem.SortNumber;
            targetItem.SortNumber = SelectedColumnItem.SortNumber;
            SelectedColumnItem.SortNumber = sortNumber;

            //swap item
            ColumnItemList.RemoveAt(previousItemIndex);
            ColumnItemList.Insert(index, targetItem);

            this.RefreshButtonsState();
        }

    }
    private void OnMoveDown()
    {
        if (SelectedColumnItem is null)
        {
            return;
        }

        int index = ColumnItemList.IndexOf(SelectedColumnItem);
        if (index < (ColumnItemList.Count - 1))
        { 
            int nextItemIndex = index + 1;
            var targetItem = ColumnItemList[nextItemIndex];

            //swap index
            int sortNumber = targetItem.SortNumber;
            targetItem.SortNumber = SelectedColumnItem.SortNumber;
            SelectedColumnItem.SortNumber = sortNumber;

            //swap item
            ColumnItemList.RemoveAt(nextItemIndex);
            ColumnItemList.Insert(index, targetItem);

            this.RefreshButtonsState();
        }

    }

    private void OnSave()
    {
        _columnsConfig.ColumnItems.Items.Clear();
        _columnsConfig.ColumnItems.Items.AddRange(_mapper.Map<List<ColumnItem>>(ColumnItemList));
        _columnsConfigService.Save(_columnsConfig);
    }

    private void RefreshButtonsState()
    {
        if (SelectedColumnItem is null)
        {
            IsUpEnabled = IsDownEnabled = false;
        }
        else
        {
            int index = ColumnItemList.IndexOf(SelectedColumnItem);

            IsUpEnabled = index > 0;
            IsDownEnabled = index < (ColumnItemList.Count - 1);
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
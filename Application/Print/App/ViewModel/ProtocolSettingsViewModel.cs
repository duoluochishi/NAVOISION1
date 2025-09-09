//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using MaterialDesignThemes.Wpf.Controls.MarkableTextBox;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.Print.Events;
using System.Collections.Generic;

namespace NV.CT.Print.ViewModel
{
    public class ProtocolSettingsViewModel : BaseViewModel
    {
        private readonly ILogger<ProtocolSettingsViewModel> _logger;
        private readonly IPrintProtocolConfigService _printProtocolConfigService;
        private bool _isAddMode = false;
        private bool _isInCellSelectionMode = false; //标记是否可以鼠标框选单元格来选择行列，仅在首先单击了左上角(0,0)的单元格，会启用该标记。
        private readonly int _defaultCellRow = 1;
        private readonly int _defaultCellColumn = 1;

        private Dictionary<BodyPart, string> _bodyParts = new Dictionary<BodyPart, string>();
        public Dictionary<BodyPart, string> BodyParts
        {
            get => _bodyParts;
            set => SetProperty(ref _bodyParts, value);
        }

        private BodyPart _selectedBodyPart = BodyPart.Abdomen;
        public BodyPart SelectedBodyPart
        {
            get => _selectedBodyPart;
            set
            {
                SetProperty(ref _selectedBodyPart, value);
            } 
        }

        private PrintProtocolConfig? _printProtocolConfig;
        public PrintProtocolConfig? PrintProtocolConfig
        {
            get
            {
                return _printProtocolConfig;
            }
            set
            {
                SetProperty(ref _printProtocolConfig, value);
            }
        }

        private ObservableCollection<PrintProtocol>? _printProtocolSource;
        public ObservableCollection<PrintProtocol>? PrintProtocolSource
        {
            get
            {
                return _printProtocolSource;
            }
            set
            {
                SetProperty(ref _printProtocolSource, value);
            }
        }

        private PrintProtocol? _currentPrintProtocolModel;
        public PrintProtocol? CurrentPrintProtocolModel
        {
            get
            {
                return _currentPrintProtocolModel;
            }
            set
            {
                SetProperty(ref _currentPrintProtocolModel, value);

                if (value is not null && !value.IsSystem)
                {
                    IsEnabled = true;
                }
                else
                {
                    IsEnabled = false;
                }

                if (value is not null)
                {
                    CurrentRowNumber = value.Row;
                    CurrentColumnNumber = value.Column;
                }
                else
                {
                    CurrentRowNumber = _defaultCellRow;
                    CurrentColumnNumber = _defaultCellColumn;
                }

            }

        }

        private int _currentRowNumber = 1;
        public int CurrentRowNumber
        {
            get
            {
                if (CurrentPrintProtocolModel is not null)
                {
                    _currentRowNumber = CurrentPrintProtocolModel.Row;
                }
                return _currentRowNumber;
            }
            set
            {
                SetProperty(ref _currentRowNumber, value);
                if (CurrentPrintProtocolModel is not null)
                {
                    CurrentPrintProtocolModel.Row = value;
                }
                EventAggregator.Instance.GetEvent<CellValueChangedEvent>().Publish();
            }        
        }

        private int _currentColumnNumber = 1;
        public int CurrentColumnNumber
        {
            get
            {
                if (CurrentPrintProtocolModel is not null)
                {
                    _currentColumnNumber = CurrentPrintProtocolModel.Column;
                }
                return _currentColumnNumber;
            }
            set
            {
                SetProperty(ref _currentColumnNumber, value);
                if (CurrentPrintProtocolModel is not null)
                {
                    CurrentPrintProtocolModel.Column = value;
                }
                EventAggregator.Instance.GetEvent<CellValueChangedEvent>().Publish();
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get 
            {
                return _isEnabled;
            }
            set 
            {
                SetProperty(ref _isEnabled, value);
            }
        }

        private MarkControlStatus _nameMarkStatus;
        public MarkControlStatus NameMarkStatus
        {
            get
            {             
                return _nameMarkStatus;
            }
            set
            { 
                SetProperty(ref _nameMarkStatus, value);
            }
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

        public ProtocolSettingsViewModel(ILogger<ProtocolSettingsViewModel> logger, 
                                         IPrintProtocolConfigService printProtocolConfigService)
        {
            _logger = logger;
            _printProtocolConfigService = printProtocolConfigService;

            Commands.Add(PrintConstants.COMMAND_SAVE, new DelegateCommand(OnSave, CanExecuteSave));
            Commands.Add(PrintConstants.COMMAND_CLOSE, new DelegateCommand<object>(OnClosed, _ => true));
            Commands.Add(PrintConstants.COMMAND_ADD_NEW, new DelegateCommand(OnAdd));
            Commands.Add(PrintConstants.COMMAND_DELETE, new DelegateCommand(OnDelete));            
            Commands.Add(PrintConstants.COMMAND_PROTOCOL_SELECTION_CHANGED, new DelegateCommand(OnProtocolSelectionChanged));
            Commands.Add(PrintConstants.COMMAND_BODYPART_SELECTION_CHANGED, new DelegateCommand(OnBodyPartSelectionChanged));
            Commands.Add(PrintConstants.COMMAND_NAME_CHANGED, new DelegateCommand(OnNameChanged));

            EventAggregator.Instance.GetEvent<CellMouseMovedEvent>().Subscribe(OnCellMouseMoved);
            EventAggregator.Instance.GetEvent<CellLeftMouseDownEvent>().Subscribe(OnCellLeftMouseDown);
            EventAggregator.Instance.GetEvent<CellLeftMouseUpEvent>().Subscribe(OnCellLeftMouseUp);
            EventAggregator.Instance.GetEvent<CellValueChangedEvent>().Subscribe(OnCellValueChanged);
        }

        public void Initialize()
        {
            this.LoadCellList();

            BodyParts = typeof(BodyPart).ToDictionary<BodyPart>();                        
            var currentStudy = NV.CT.Print.Global.Instance.PrintingStudy;
            if (System.Enum.TryParse<BodyPart>(currentStudy?.BodyPart, true, out BodyPart defaultBodyPart))
            {
                _logger.LogInformation($"{currentStudy?.BodyPart} found in BodyParts");
                SelectedBodyPart = BodyParts.First(v => v.Key == defaultBodyPart).Key;
            }
            else
            {
                _logger.LogInformation($"{currentStudy?.BodyPart} does not exist in BodyParts");
                SelectedBodyPart = BodyParts.First().Key;
            }

            PrintProtocolConfig = _printProtocolConfigService.GetConfigs();
            RefreshProtocolSource();
        }

        private void LoadCellList()
        {
            _cellList.Clear();
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    _cellList.Add(new CellViewModel() { Id = (i * 10 + j), RowNumber = i, ColumnNumber = j });
                }
            }
        }

        private bool CanExecuteSave()
        {
            if (CurrentPrintProtocolModel is not null && !string.IsNullOrEmpty(CurrentPrintProtocolModel.Name))
            {
                return true;
            }
            return false;
        }

        private void OnClosed(object parameter)
        {
            if (parameter is Window window)
            {
                window.Hide();
            }
        }

        private void OnAdd()
        {
            _isAddMode = true;
            var newPrintProtocol = new PrintProtocol();
            newPrintProtocol.Id = Guid.NewGuid().ToString();
            newPrintProtocol.Name = string.Empty;
            newPrintProtocol.BodyPart = SelectedBodyPart.ToString();
            newPrintProtocol.IsSystem = false;
            newPrintProtocol.IsDefault = false;
            newPrintProtocol.Row = 2;
            newPrintProtocol.Column = 3;

            CurrentPrintProtocolModel = newPrintProtocol;
        }

        private void OnDelete()
        {
            _isAddMode = false;
            if(CurrentPrintProtocolModel == null)
            {
                return;
            }

            var deleteItem = PrintProtocolConfig?.PrintProtocols.FirstOrDefault(p => p.Id == CurrentPrintProtocolModel.Id);
            if (deleteItem == null)
            {
                return;
            }

            //保存到配置文件
            PrintProtocolConfig?.PrintProtocols.Remove(deleteItem);
            _printProtocolConfigService.Save(PrintProtocolConfig);

            //刷新内存数据源
            PrintProtocolSource?.Remove(deleteItem);

            var count = PrintProtocolSource?.Count;
            if (count > 0)
            {
                CurrentPrintProtocolModel = PrintProtocolSource?[count.Value - 1];
            }
            else
            {
                CurrentPrintProtocolModel = null;
            }

            EventAggregator.Instance.GetEvent<ProtocolsChangedEvent>().Publish(SelectedBodyPart);
        }

        private void OnSave()
        {
            NameMarkStatus = MarkControlStatus.Default;

            if (CurrentPrintProtocolModel is null)
            {
                return;
            }

            if (_isAddMode == true)
            {
                if ((PrintProtocolConfig?.PrintProtocols.Any(p => p.Id == CurrentPrintProtocolModel?.Id)).Value == true)
                {
                    _logger.LogError($"Failed to add duplicate Id:{CurrentPrintProtocolModel.Id}, Name is {CurrentPrintProtocolModel.Name}");
                    return;
                }

                if (CurrentPrintProtocolModel.IsDefault)
                {
                    SetIsDefaultForBodyPart();
                }

                PrintProtocolConfig?.PrintProtocols.Add(CurrentPrintProtocolModel);
                PrintProtocolSource?.Add(CurrentPrintProtocolModel);
            }
            else
            {
                var foundProtocol = PrintProtocolConfig?.PrintProtocols.SingleOrDefault(p => p.Id == CurrentPrintProtocolModel?.Id);
                if (foundProtocol is not null)
                {
                    foundProtocol.Name = CurrentPrintProtocolModel.Name;
                    foundProtocol.BodyPart = CurrentPrintProtocolModel.BodyPart;
                    foundProtocol.IsDefault = CurrentPrintProtocolModel.IsDefault;
                    foundProtocol.IsSystem = CurrentPrintProtocolModel.IsSystem;
                    foundProtocol.Row = CurrentPrintProtocolModel.Row;
                    foundProtocol.Column = CurrentPrintProtocolModel.Column;

                    if (CurrentPrintProtocolModel.IsDefault)
                    {
                        SetIsDefaultForBodyPart();
                    }
                }
            }
            _printProtocolConfigService.Save(PrintProtocolConfig);
            _isAddMode = false;

            EventAggregator.Instance.GetEvent<ProtocolsChangedEvent>().Publish(SelectedBodyPart);
        }

        private void OnProtocolSelectionChanged()
        {
            this._isAddMode = false;
            this.NameMarkStatus = MarkControlStatus.Default;
        }

        private void OnBodyPartSelectionChanged()
        {
            RefreshProtocolSource();
        }

        private void OnNameChanged()
        {
            if (string.IsNullOrEmpty(CurrentPrintProtocolModel?.Name))
            {
                this.NameMarkStatus = MarkControlStatus.Error;
            }
            else
            {
                this.NameMarkStatus = MarkControlStatus.Default;
            }

            ((DelegateCommand)Commands[PrintConstants.COMMAND_SAVE]).RaiseCanExecuteChanged(); 

        }

        private void RefreshProtocolSource()
        {
            PrintProtocolSource = PrintProtocolConfig?.PrintProtocols.Where(p => p.BodyPart == SelectedBodyPart.ToString()).ToList().ToObservableCollection();
            CurrentPrintProtocolModel = PrintProtocolSource?.Count > 0 ? PrintProtocolSource[0] : null;
        }

        private void SetIsDefaultForBodyPart()
        {
            //set IsDefault for config
            var protocols = PrintProtocolConfig?.PrintProtocols.Where(p => p.BodyPart == SelectedBodyPart.ToString());
            foreach (var protocol in protocols)
            {
                if (protocol.Id == CurrentPrintProtocolModel?.Id)
                {
                    continue;
                }
                protocol.IsDefault = false;
            }

            //set IsDefault for objects in memory
            foreach (var protocol in PrintProtocolSource)
            {
                if (protocol.Id == CurrentPrintProtocolModel?.Id)
                {
                    continue;
                }
                protocol.IsDefault = false;
            }


        }

        private void OnCellMouseMoved(CellViewModel cellViewModel)
        {
            //如果是非鼠标框选模式，则不予处理
            if (!_isInCellSelectionMode)
            {
                return;
            }

            CurrentRowNumber = cellViewModel.RowNumber + 1;
            CurrentColumnNumber = cellViewModel.ColumnNumber + 1;
        }

        private void OnCellLeftMouseDown(CellViewModel cellViewModel)
        {
            //仅在左键单击最左上角(0,0)的单元格时，才开始进入鼠标框选模式
            if (cellViewModel.RowNumber == 0 && cellViewModel.ColumnNumber == 0)
            {
                _isInCellSelectionMode = true;
                cellViewModel.CellColor = CellViewModel.COVERED_COLOR;
            }
            else
            {
                _isInCellSelectionMode = false;
            }            
        }

        private void OnCellLeftMouseUp(CellViewModel cellViewModel)
        {
            if (_isInCellSelectionMode)
            {
                CurrentRowNumber = cellViewModel.RowNumber + 1;
                CurrentColumnNumber = cellViewModel.ColumnNumber + 1;
                EventAggregator.Instance.GetEvent<CellMouseMovedEvent>().Publish(cellViewModel);
                _isInCellSelectionMode = false;
            }
            
        }

        private void OnCellValueChanged()
        {
            if (CurrentPrintProtocolModel is not null)
            {
                SetColorOfCells(CurrentPrintProtocolModel.Row - 1, CurrentPrintProtocolModel.Column - 1);
            }
            else
            {
                SetColorOfCells(_defaultCellRow-1, _defaultCellRow-1);
            }
        }

        private void SetColorOfCells(int maxRow,  int maxColumn)
        {
            var coveredCells = CellList.Where(c=>c.RowNumber <= maxRow && c.ColumnNumber <= maxColumn).ToArray();
            var uncoveredCells = CellList.Where(c => c.RowNumber > maxRow || c.ColumnNumber > maxColumn).ToArray();
            foreach(var coveredCell in coveredCells)
            {
                coveredCell.CellColor = CellViewModel.COVERED_COLOR;
            }

            foreach (var uncoveredCell in uncoveredCells)
            {
                uncoveredCell.CellColor = CellViewModel.UNCOVERED_COLOR;
            }

        }

    }

}

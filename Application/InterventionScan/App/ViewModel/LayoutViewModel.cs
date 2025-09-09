//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/19 10:32:28           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.InterventionalScan.Models;
using NV.CT.InterventionScan.ApplicationService.Contract;
using NV.CT.InterventionScan.Models;
using NV.CT.Language;
using NV.CT.SyncService.Contract;
using NV.MPS.Configuration;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.InterventionScan.ViewModel;

public class LayoutViewModel : BaseViewModel
{
    private readonly IDataSync _dataSync;
    private readonly IInterventionService _interventionService;
    private readonly IDialogService _dialogService;
   // private readonly IWindowTypeService _windowTypeService;
    private ObservableCollection<SeriesModel> _seriesList = new();
    public ObservableCollection<SeriesModel> SeriesList
    {
        get => _seriesList;
        set => SetProperty(ref _seriesList, value);
    }

    private SeriesModel _selectedSeries = new SeriesModel();
    public SeriesModel SelectedSeries
    {
        get => _selectedSeries;
        set
        {
            if (SetProperty(ref _selectedSeries, value)
                && value is not null
                && !string.IsNullOrEmpty(value.StoragePath)
                && Directory.Exists(value.StoragePath))
            {
                int fileCount = Directory.GetFiles(value.StoragePath).Length;
                if (fileCount <= 2)
                {
                    _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_LAYOUT, CommandParameters.IMAGE_LAYOUT1_1);
                }
                else
                {
                    if (_interventionService.ImageLayoutType.Equals(CommandParameters.IMAGE_LAYOUT1_1))
                    {
                        _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_LAYOUT, CommandParameters.IMAGE_LAYOUT1_2);
                    }
                }
                _interventionService.SetImagePath(value.StoragePath, value.IsIntervention);
            }
        }
    }

    /// <summary>
    /// 系统里定义的针对象列表
    /// </summary>
    private ObservableCollection<NeedleModel> _needleModelSource = new();
    public ObservableCollection<NeedleModel> NeedleModelSource
    {
        get => _needleModelSource;
        set => SetProperty(ref _needleModelSource, value);
    }

    /// <summary>
    /// 增加出来的注入针列表
    /// </summary>
    private ObservableCollection<NeedleModel> _needleModelList = new();
    public ObservableCollection<NeedleModel> NeedleModelList
    {
        get => _needleModelList;
        set => SetProperty(ref _needleModelList, value);
    }

    private NeedleModel _selectedNeedleModel = new NeedleModel { NeedleColor = "#000000" };
    public NeedleModel SelectedNeedleModel
    {
        get => _selectedNeedleModel;
        set
        {
            if (SetProperty(ref _selectedNeedleModel, value) && IsUIChange && value is not null)
            {
                _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_SELECTNEEDLE, value.Name);
            }
        }
    }

    private bool IsUIChange = true;

    private ObservableCollection<WindowingInfo>? _wwwlItems = new();

    public ObservableCollection<WindowingInfo>? WWWLItems
    {
        get => _wwwlItems;
        set => SetProperty(ref _wwwlItems, value);
    }

    public LayoutViewModel(IDataSync dataSync,
        IInterventionService interventionService,
        IDialogService dialogService)
    {
        _dataSync = dataSync;
        _interventionService = interventionService;
        _dialogService = dialogService;     
        Commands.Add(CommandParameters.COMMAND_SETLAYOUT, new DelegateCommand<string>(SetLayout, _ => true));
        Commands.Add(CommandParameters.COMMAND_MOVE, new DelegateCommand(Move, () => true));
        Commands.Add(CommandParameters.COMMAND_ZOOM, new DelegateCommand(Zoom, () => true));
        Commands.Add(CommandParameters.COMMAND_RESET, new DelegateCommand(Reset, () => true));
        Commands.Add(CommandParameters.COMMAND_SETWWWL, new DelegateCommand<string>(SetWwWlCommand, _ => true));
        Commands.Add(CommandParameters.COMMAND_SETROI, new DelegateCommand<string>(SetROICommand, _ => true));
        Commands.Add(CommandParameters.COMMAND_SERIESMOVE, new DelegateCommand<string>(SeriesMove, _ => true));
        Commands.Add(CommandParameters.COMMAND_NEEDLEOPT, new DelegateCommand<string>(NeedleOpt, _ => true));

        _dataSync.SeriesDataChanged -= DataSync_SyncSeriesDataChanged;
        _dataSync.SeriesDataChanged += DataSync_SyncSeriesDataChanged;
        _interventionService.ImageSelectNeedleNameChanged -= InterventionService_ImageSelectNeedleNameChanged;
        _interventionService.ImageSelectNeedleNameChanged += InterventionService_ImageSelectNeedleNameChanged;
        _interventionService.SelectNeedleChanged += InterventionService_SelectNeedleChanged;
        _interventionService.DelectNeedleEvent += InterventionService_DelectNeedleEvent;
        _interventionService.AddNeedleEvent += InterventionService_AddNeedleEvent;

        InitCombox();
    }

    [UIRoute]
    private void InterventionService_AddNeedleEvent(object? sender, EventArgs<string> e)
    {
        if (!(e is not null && !string.IsNullOrEmpty(e.Data) && NeedleModelSource.Count > 0))
        {
            return;
        }
        var needle = NeedleModelSource.FirstOrDefault(t => t.Name == e.Data);
        if (needle is NeedleModel model)
        {
            NeedleModelList.Add(model);
            SelectedNeedleModel = model;
        }
    }

    [UIRoute]
    private void InterventionService_DelectNeedleEvent(object? sender, EventArgs<string> needleName)
    {
        if (!(needleName is not null && !string.IsNullOrEmpty(needleName.Data) && NeedleModelList.Count > 0))
        {
            return;
        }
        var needle = NeedleModelList.FirstOrDefault(t => t.Name == needleName.Data);
        if (needle is null)
        {
            return;
        }
        if (SelectedNeedleModel is not null && SelectedNeedleModel.Name.Equals(needle.Name))
        {
            SelectedNeedleModel = new NeedleModel { NeedleColor = "#000000" };
        }
        NeedleModelList.Remove(needle);
    }

    [UIRoute]
    private void InterventionService_SelectNeedleChanged(object? sender, EventArgs<string> needleName)
    {
        if (!(needleName is not null && !string.IsNullOrEmpty(needleName.Data) && NeedleModelList.Count > 0))
        {
            return;
        }
        var needle = NeedleModelList.FirstOrDefault(t => t.Name == needleName.Data);
        if (needle is NeedleModel model)
        {
            SelectedNeedleModel = needle;
        }
    }

    [UIRoute]
    private void InterventionService_ImageSelectNeedleNameChanged(object? sender, EventArgs<string> e)
    {
        if (e is null || string.IsNullOrEmpty(e.Data))
        {
            return;
        }
        IsUIChange = false;
        var model = NeedleModelSource.FirstOrDefault(t => t.Name.Equals(e.Data));
        if (model is not null)
        {
            SelectedNeedleModel = model;
        }
        IsUIChange = true;
    }

    [UIRoute]
    private void DataSync_SyncSeriesDataChanged(object? sender, EventArgs<(ProtocolModel protocolModel, string currentReconID)> e)
    {
        if (e.Data.protocolModel is null)
        {
            return;
        }
        ProtocolHelper.ResetParent(e.Data.protocolModel);
        foreach (var form in e.Data.protocolModel.Children)
        {
            foreach (var measurementModel in form.Children)
            {
                foreach (var scanModel in measurementModel.Children)
                {
                    foreach (var reconModel in scanModel.Children)
                    {
                        if (scanModel.IsIntervention)  //仅仅增加介入扫描的图像序列
                        {
                            int count = SeriesList.Count(t => t.IsIntervention);
                            InitSeries(reconModel, (count + 1).ToString());
                        }
                    }
                }
            }
        }
        var selectModel = SeriesList.FirstOrDefault(t => t.ReconID.Equals(e.Data.currentReconID));
        if (selectModel is not null)
        {
            SelectedSeries = selectModel;
        }
    }

    private void InitSeries(ReconModel reconModel, string interventionIndex = "")
    {
        if (reconModel != null
            && !string.IsNullOrEmpty(reconModel.Descriptor.Id)
            && reconModel.IsRTD
            && reconModel.Status == PerformStatus.Performed
            && !string.IsNullOrEmpty(reconModel.ImagePath)
            && Directory.Exists(reconModel.ImagePath))
        {
            var model = SeriesList.FirstOrDefault(t => t.ReconID.Equals(reconModel.Descriptor.Id));
            if (model is not null)
            {
                model.StoragePath = reconModel.ImagePath;
            }
            else
            {
                SeriesModel seriesModel = new SeriesModel();
                seriesModel.ID = reconModel.Descriptor.Id;
                seriesModel.Name = $"{reconModel.DefaultSeriesDescription} {interventionIndex}";
                seriesModel.StoragePath = reconModel.ImagePath;
                seriesModel.ReconID = reconModel.Descriptor.Id;
                seriesModel.SortNO = SeriesList.Count + 1;
                seriesModel.IsIntervention = reconModel.Parent.IsIntervention;

                SeriesList.Add(seriesModel);
            }
        }
    }

    private void InitCombox()
    {
        NeedleModel needleModel = new NeedleModel();
        needleModel.ID = "1";
        needleModel.Name = "N1";
        needleModel.NeedleColor = "#00FFFF";
        NeedleModelSource.Add(needleModel);

        needleModel = new NeedleModel();
        needleModel.ID = "2";
        needleModel.Name = "N2";
        needleModel.NeedleColor = "#7B68EE";
        NeedleModelSource.Add(needleModel);

        needleModel = new NeedleModel();
        needleModel.ID = "3";
        needleModel.Name = "N3";
        needleModel.NeedleColor = "#1E90FF";
        NeedleModelSource.Add(needleModel);

        needleModel = new NeedleModel();
        needleModel.ID = "4";
        needleModel.Name = "N4";
        needleModel.NeedleColor = "#FFC0CB";
        NeedleModelSource.Add(needleModel);

        needleModel = new NeedleModel();
        needleModel.ID = "5";
        needleModel.Name = "N5";
        needleModel.NeedleColor = "#FFEBCD";

        NeedleModelSource.Add(needleModel);

        var windowTypes = UserConfig.WindowingConfig.Windowings;
        if (windowTypes is null)
        {
            return;
        }
        //添加自定义ww/wl
        windowTypes.Add(new WindowingInfo()
        {
            Width = new ItemField<int>() { Value = 0 },
            Level = new ItemField<int>() { Value = 0 },
            BodyPart = "Custom",
            Shortcut = "F12",
            Description = "Custom",
        });
        WWWLItems = windowTypes.ToObservableCollection();
    }

    private void SetLayout(string param)
    {
        _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_LAYOUT, param);
    }

    private void Move()
    {
        _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_MOVE, string.Empty);
    }

    private void Zoom()
    {
        _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_ZOOM, string.Empty);
    }

    private void Reset()
    {
        _dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Warning_Title, LanguageResource.Message_Warning_RevertInitialState, arg =>
        {
            if (arg.Result == ButtonResult.OK)
            {
                _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_RESET, string.Empty);
                NeedleModelList.Clear();
            }
        }, ConsoleSystemHelper.WindowHwnd);
    }

    private void SetWwWlCommand(string param)
    {
        _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_WL, param);
    }

    private void SetROICommand(string param)
    {
        _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_ROI, param);
    }

    private void NeedleOpt(string param)
    {
        switch (param)
        {
            case CommandParameters.IMAGE_OPERATE_ADD:
                AddNeedle();
                break;
            case CommandParameters.IMAGE_OPERATE_DELETE:
            default:
                DeleteNeedle();
                break;
        }
    }

    private void AddNeedle()
    {
        if (SelectedSeries is null || !SelectedSeries.IsIntervention)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, LanguageResource.Message_Info_AddScanningNeedleInfo, arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        NeedleModel model = GetInitNeedle();
        if (model is not null && !string.IsNullOrEmpty(model.Name))
        {
            _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_AddNEEDLE, model.Name + "," + model.NeedleColor);
        }
    }

    private NeedleModel GetInitNeedle()
    {
        NeedleModel needle = new NeedleModel();
        foreach (var nNeedle in NeedleModelSource)
        {
            var model = NeedleModelList.FirstOrDefault(t => t.Name.Equals(nNeedle.Name));
            if (model is null)
            {
                needle = nNeedle;
                break;
            }
        }
        return needle;
    }

    private void DeleteNeedle()
    {
        _dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Warning_Title, LanguageResource.Message_Warning_DeletionSelectionNeedle, arg =>
        {
            if (arg.Result == ButtonResult.OK && SelectedNeedleModel is not null)
            {
                _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_DELNEEDLE, SelectedNeedleModel.Name);
            }
        }, ConsoleSystemHelper.WindowHwnd);
    }

    private void SeriesMove(string param)
    {
        switch (param)
        {
            case CommandParameters.IMAGE_OPERATE_FORWARD: //前翻              
            case CommandParameters.IMAGE_OPERATE_BACK: //后翻
                _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_SERIESMOVE, param);
                break;
            case CommandParameters.IMAGE_OPERATE_LAST:   //上一个序列
                LastSeries();
                break;
            case CommandParameters.IMAGE_OPERATE_NEXT:  //下一个序列
                NextSeries();
                break;
            default:
                break;
        }
    }

    private void LastSeries()
    {
        if (SelectedSeries is not null && SeriesList.Count > 1)
        {
            int index = SeriesList.IndexOf(SelectedSeries);
            if (index >= 1)
            {
                SelectedSeries = SeriesList[index - 1];
            }
        }
    }

    private void NextSeries()
    {
        if (SelectedSeries is not null && SeriesList.Count > 1)
        {
            int index = SeriesList.IndexOf(SelectedSeries);
            if (index < SeriesList.Count - 1)
            {
                SelectedSeries = SeriesList[index + 1];
            }
        }
    }
}
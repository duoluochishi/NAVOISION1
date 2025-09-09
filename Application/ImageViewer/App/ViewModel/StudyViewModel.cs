//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ClientProxy.DataService;
using NV.CT.ImageViewer.Model;

namespace NV.CT.ImageViewer.ViewModel;

public class StudyViewModel : BaseViewModel
{
    private readonly IMapper _mapper;
    private readonly IViewerService _viewerService;

    private ObservableCollection<VStudyModel> _vStudyModels = new();
    public ObservableCollection<VStudyModel> VStudyModels
    {
        get => _vStudyModels;
        set => SetProperty(ref _vStudyModels, value);
    }

    private VStudyModel? _selectedItem;
    /// <summary>
    /// 默认的检查
    /// </summary>
    public VStudyModel? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public StudyViewModel(IViewerService viewerService, IMapper mapper)
    {
        _mapper = mapper;
        _viewerService = viewerService;

        if (NeedInit())
        {
            GetVStudyModelsByStudyId(Global.Instance.StudyId);
        }
        _viewerService.ViewerChanged += OnStudyChanged;
    }
    private void OnStudyChanged(object? sender, string parameters)
    {
        GetVStudyModelsByStudyId(Global.Instance.ParseParameters(parameters));
    }
    public virtual bool NeedInit()
    {
        return false;
    }
	public void GetVStudyModelsByStudyId(string studyId)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            VStudyModels.Clear();

            var result = _viewerService.Get(studyId);
            var vStudyModel = _mapper.Map<VStudyModel>(result.Patient);
            _mapper.Map(result.Study, vStudyModel);

            //if (vStudyModel is not null)
            //    SelectedItem = vStudyModel;

            VStudyModels.Add(vStudyModel);
            SelectedItem = VStudyModels[0];
        });
    }
}
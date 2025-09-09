//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Model;

namespace NV.CT.ImageViewer.ViewModel;

public class StudyListViewModel : BindableBase
{
    private readonly IMapper _mapper;
    private readonly IViewerService _viewerService;

    private ObservableCollection<VStudyModel> vStudyModels = new();
    public ObservableCollection<VStudyModel> VStudyModels
    {
        get => vStudyModels;
        set => SetProperty(ref vStudyModels, value);
    }

    private VStudyModel? selectedItem;
    public VStudyModel? SelectedItem
    {
        get => selectedItem;
        set => SetProperty(ref selectedItem, value);
    }

    public StudyListViewModel(IViewerService viewerService, IMapper mapper)
    {
        _mapper = mapper;
        _viewerService = viewerService;

        GetVStudyModelsByStudyId(Global.Instance.StudyId);
    }

    public void GetVStudyModelsByStudyId(string studyId)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            if (VStudyModels == null)
            {
                VStudyModels = new ObservableCollection<VStudyModel>();
            }
            VStudyModels.Clear();
            var result = _viewerService.Get(studyId);
            var vStudyModel = _mapper.Map<VStudyModel>(result.Patient);
            _mapper.Map(result.Study, vStudyModel);
            VStudyModels.Add(vStudyModel);
            SelectedItem = VStudyModels[0];
        });
    }
}
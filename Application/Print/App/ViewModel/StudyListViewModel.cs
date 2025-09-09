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
using NV.CT.Print.Events;
using NV.CT.Print.Models;
using System.Collections.Generic;

namespace NV.CT.Print.ViewModel
{
    public class StudyListViewModel : BaseViewModel
    {
        private readonly ILogger<StudyListViewModel>? _logger;

        private ObservableCollection<StudyBodyPartModel>? _studyBodyPartModelList = new ObservableCollection<StudyBodyPartModel>();
        public ObservableCollection<StudyBodyPartModel>? StudyBodyPartModelList
        {
            get => _studyBodyPartModelList;
            set => SetProperty(ref _studyBodyPartModelList, value);
        }   

        public StudyListViewModel(ILogger<StudyListViewModel> logger)
        {
            _logger = logger;
            EventAggregator.Instance.GetEvent<SelectedStudyChangedEvent>().Subscribe(ShowCurrentStudy);
        }

        private void ShowCurrentStudy(PrintingStudyModel printingStudyModel)
        {
            _logger?.LogDebug($"show study info with {printingStudyModel.BodyPart}");

            Application.Current?.Dispatcher?.Invoke(() =>
            {
                StudyBodyPartModelList?.Clear();
                StudyBodyPartModelList?.Add(new StudyBodyPartModel()
                {
                    StudyId = printingStudyModel.Id,
                    BodyPart = printingStudyModel.BodyPart,
                    ReconEndDate = printingStudyModel.StudyDate
                });
            });
        }
    }
}

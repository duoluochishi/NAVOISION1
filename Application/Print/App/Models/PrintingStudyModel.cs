//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace NV.CT.Print.Models
{
    public class PrintingStudyModel : BaseViewModel
    {
        private string _Id = string.Empty;
        public string Id
        {
            get => _Id;
            set => SetProperty(ref _Id, value);
        }

        private string _internalPatientId = string.Empty;
        public string InternalPatientId
        {
            get => _internalPatientId;
            set => SetProperty(ref _internalPatientId, value);
        }

        private string _studyInstanceUID = string.Empty;
        public string StudyInstanceUID
        {
            get => _studyInstanceUID;
            set => SetProperty(ref _studyInstanceUID, value);
        }

        private string _bodyPart = string.Empty;
        public string BodyPart
        {
            get => _bodyPart;
            set => SetProperty(ref _bodyPart, value);
        }

        private DateTime _studyDate;
        public DateTime StudyDate
        {
            get => _studyDate;
            set => SetProperty(ref _studyDate, value);
        }

        private DateTime? _examStartTime;
        public DateTime? ExamStartTime
        {
            get => _examStartTime;
            set => SetProperty(ref _examStartTime, value);
        }

        private DateTime? _examEndTime;
        public DateTime? ExamEndTime
        {
            get => _examEndTime;
            set => SetProperty(ref _examEndTime, value);
        }

        private PrintingPatientModel? _printingPatientModel;
        public PrintingPatientModel? PrintingPatientModel
        {
            get => _printingPatientModel;
            set => SetProperty(ref _printingPatientModel, value);
        }

        private List<PrintingSeriesModel>? _printingSeriesModelList;
        public List<PrintingSeriesModel>? PrintingSeriesModelList
        {
            get => _printingSeriesModelList;
            set => SetProperty(ref _printingSeriesModelList, value);
        }

    }
}

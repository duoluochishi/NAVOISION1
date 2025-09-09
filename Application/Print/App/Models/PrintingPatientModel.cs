//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Print.Models
{
    public class PrintingPatientModel : BaseViewModel
    {
        private string _Id = string.Empty;
        public string Id
        {
            get => _Id;
            set => SetProperty(ref _Id, value);
        }

        private string _patientName = string.Empty;
        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        private string _patientId = string.Empty;
        public string PatientId
        {
            get => _patientId;
            set => SetProperty(ref _patientId, value);
        }
    }
}

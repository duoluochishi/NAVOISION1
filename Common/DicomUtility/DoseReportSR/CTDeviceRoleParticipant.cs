//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.DoseReportSR
{
    public  class CTDeviceRoleParticipant
    {
        public string DeviceName { get; set; }
        public string Manufacturer { get; set; }

        public string ModelName { get; set; }

        public string SerialNumber { get; set; }

        public string ObserverUID { get; set; }
    }
}

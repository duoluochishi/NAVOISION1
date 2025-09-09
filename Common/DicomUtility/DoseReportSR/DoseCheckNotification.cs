//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class DoseCheckNotification
    {
        public bool DLPNotificationConfigured
        {
            get;set;
        }

        public bool CTDIvolNotificationConfigured
        {
            get;set;
        }
        public double DLPNotificationValue
        {
            get;set;
        }

        public double CTDIolNotificationValue
        {
            get;set;
        }

        public double DLPForwardEstimate
        {
            get;set;
        }
        public double CTDIvolForwardEstimate
        {
            get;set;
        }
        public string ReasonForProceeding
        {
            get; set;
        }


        public string PersonName
        {
            get;set;
        }
        public int PersonRole
        {
            get;set;
        }
    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class DoseReportSRData
    {
        public string DeviceObserverUid
        {
            get;set;
        }

        public DateTime StartDateTime
        {
            get;set;
        }
            
        public DateTime EndDateTime
        {
            get;set;
        }

        public string StudyInstanceUID
        {
            get;set;
        }

        public int TotalNumberOfIrrEvent
        { get; set; }
        public double DoseLengthProductTotal
        {
            get;set;
        }

        public string Comment { get; set; }


        public List<CTAcquisitionData> CTAcquisitionDatas
        {
            get;
        }

        public DoseReportSRData()
        {
            CTAcquisitionDatas = new List<CTAcquisitionData>();
        }
    }

}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.DoseReportSR
{
    public class CTDoseInfo
    {
        public double MeanCTDIvol
        {
            get;set;
        }

        public int CTDIwPhantomType
        {
            get;set;
        }

        public double DLP
        { get; set; }


        public DoseCheckAlert DoseCheckAlert { get; set; }
        public DoseCheckNotification DoseCheckNotification { get; set; }
    }
}

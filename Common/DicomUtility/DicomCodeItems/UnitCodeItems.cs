//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 17:42:24    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------



using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomCodeItems
{
    public class UnitCodeItems
    {
        public static readonly DicomCodeItem EventsUnit_DCM = new DicomCodeItem("{events}", "UCUM", "events", "1.4");
        public static readonly DicomCodeItem MgYCMUnit_DCM = new DicomCodeItem("mGy.cm", "UCUM", "mGy.cm", "1.4");
        public static readonly DicomCodeItem SecondUnit_DCM = new DicomCodeItem("s", "UCUM", "s", "1.4");
        public static readonly DicomCodeItem Millimetre_DCM = new DicomCodeItem("mm", "UCUM", "mm", "1.4");
        public static readonly DicomCodeItem Milliliter_DCM = new DicomCodeItem("ml", "UCUM", "ml", "1.4");
        public static readonly DicomCodeItem RatioUnit_DCM = new DicomCodeItem("{ratio}", "UCUM", "ratio", "1.4");
        public static readonly DicomCodeItem XRaysources_DCM = new DicomCodeItem("{X-Ray sources}", "UCUM", "X-Ray sources", "1.4");
        public static readonly DicomCodeItem KVUnit_DCM = new DicomCodeItem("kV", "UCUM", "kV", "1.4");
        public static readonly DicomCodeItem MAUnit_DCM = new DicomCodeItem("mA", "UCUM", "mA", "1.4");
        public static readonly DicomCodeItem MgYUnit_DCM = new DicomCodeItem("mGy", "UCUM", "mGy", "1.4");

    }
}

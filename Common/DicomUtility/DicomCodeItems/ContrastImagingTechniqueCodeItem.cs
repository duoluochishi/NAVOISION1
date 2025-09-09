//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/18 11:38:22    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom;
using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomCodeItems
{
    public class ContrastImagingTechniqueCodeItem
    {
        public static readonly DicomCodeItem DiagnosticWithContrast_SCT = new DicomCodeItem("27483000", "SCT", "Diagnostic radiography with​\r\ncontrast media");
        public static readonly DicomCodeItem CTWithOutContrast_SCT = new DicomCodeItem("399331006​", "SCT", "CT without contrast");

    }
}

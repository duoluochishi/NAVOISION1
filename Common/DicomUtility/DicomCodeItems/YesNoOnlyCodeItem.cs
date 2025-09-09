//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/18 11:44:08    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomCodeItems
{
    public class YesNoOnlyCodeItem
    {
        public static readonly DicomCodeItem Yes​_SCT = new DicomCodeItem("373066001", "SCT", "Yes");
        public static readonly DicomCodeItem No​_SCT = new DicomCodeItem("373067005", "SCT", "No");

        /// <summary>
        /// This coding scheme "SRT" is deprecated. The use of "SNOMED-RT style" code values is no longer
        /// authorized by SNOMED except for creation by legacy devices, legacy objects in archives,
        /// and receiving systems that need to understand them.
        /// </summary>
        public static readonly DicomCodeItem Yes​_SRT = new DicomCodeItem("R-0038D", "SRT", "Yes");
        public static readonly DicomCodeItem No​_SRT = new DicomCodeItem("R-00339", "SRT", "No");
    }
}

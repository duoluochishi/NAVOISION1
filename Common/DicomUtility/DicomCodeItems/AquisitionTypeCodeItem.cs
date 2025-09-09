//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/16 15:03:36    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomCodeItems
{
    public class AquisitionTypeCodeItem
    {
        public static readonly DicomCodeItem Sequenced_DCM​ = new DicomCodeItem("113804", "DCM", "Sequenced Acquisition");

        public static readonly DicomCodeItem Spiral_SCT​ = new DicomCodeItem("116152004", "SCT", "Spiral Acquisition");

        public static readonly DicomCodeItem ConstantAngle_DCM​ = new DicomCodeItem("113805", "DCM", "Constant Angle Acquisition");

        public static readonly DicomCodeItem Stationary_DCM​ = new DicomCodeItem("113806", "DCM", "Stationary Acquisition");

        public static readonly DicomCodeItem Free_DCM​ = new DicomCodeItem("113807", "DCM", "Free Acquisition");

        public static readonly DicomCodeItem ConeBeam_SCT​ = new DicomCodeItem("702569007", "SCT", "Cone Beam Acquisition");

    }
}

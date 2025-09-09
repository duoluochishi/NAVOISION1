//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/15 13:19:55    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using FellowOakDicom.StructuredReport;

namespace NV.CT.DicomUtility.DicomCodeItems
{
    public class TargetRegionCodeItem
    {
        public static readonly DicomCodeItem ABDOMEN_SCT = new DicomCodeItem("113345001", "SCT", "ABDOMEN");
        public static readonly DicomCodeItem ABDOMENPELVIS_SCT = new DicomCodeItem("416949008", "SCT", "ABDOMENPELVIS");
        public static readonly DicomCodeItem ARM_SCT = new DicomCodeItem("53120007", "SCT", "ARM");
        public static readonly DicomCodeItem BRAIN_SCT = new DicomCodeItem("12738006", "SCT", "BRAIN");
        public static readonly DicomCodeItem BREAST_SCT = new DicomCodeItem("76752008", "SCT", "BREAST");
        public static readonly DicomCodeItem CHEEK_SCT = new DicomCodeItem("60819002", "SCT", "CHEEK");
        public static readonly DicomCodeItem CHEST_SCT = new DicomCodeItem("51185008", "SCT", "CHEST");
        public static readonly DicomCodeItem CHESTABDOMEN_SCT = new DicomCodeItem("416550000", "SCT", "CHESTABDOMEN");
        public static readonly DicomCodeItem CLAVICLE_SCT = new DicomCodeItem("51299004", "SCT", "CLAVICLE");
        public static readonly DicomCodeItem ELBOW_SCT = new DicomCodeItem("16953009", "SCT", "ELBOW");
        public static readonly DicomCodeItem WHOLEBODY_SCT = new DicomCodeItem("38266002", "SCT", "WHOLEBODY");
        public static readonly DicomCodeItem EYE_SCT = new DicomCodeItem("40638003", "SCT", "EYE");
        public static readonly DicomCodeItem FEMUR_SCT = new DicomCodeItem("71341001", "SCT", "FEMUR");
        public static readonly DicomCodeItem FOOT_SCT = new DicomCodeItem("56459004", "SCT", "FOOT");
        public static readonly DicomCodeItem HAND_SCT = new DicomCodeItem("85562004", "SCT", "HAND");
        public static readonly DicomCodeItem HEAD_SCT = new DicomCodeItem("69536005", "SCT", "HEAD");
        public static readonly DicomCodeItem HEADNECK_SCT = new DicomCodeItem("774007", "SCT", "HEADNECK");
        public static readonly DicomCodeItem HEART_SCT = new DicomCodeItem("80891009", "SCT", "HEART");
        public static readonly DicomCodeItem HIP_SCT = new DicomCodeItem("24136001", "SCT", "HIP");
        //public static readonly DicomCodeItem IAC_SCT = new DicomCodeItem("IAC", "", "SCT", );
        public static readonly DicomCodeItem KIDNEY_SCT = new DicomCodeItem("64033007", "SCT", "KIDNEY");
        public static readonly DicomCodeItem KNEE_SCT = new DicomCodeItem("49076000", "SCT", "KNEE");
        public static readonly DicomCodeItem LARYNX_SCT = new DicomCodeItem("312535008", "SCT", "LARYNX");
        public static readonly DicomCodeItem LEG_SCT = new DicomCodeItem("61685007", "SCT", "LEG");
        public static readonly DicomCodeItem LIVER_SCT = new DicomCodeItem("10200004", "SCT", "LIVER");
        public static readonly DicomCodeItem LSPINE_SCT = new DicomCodeItem("122496007", "SCT", "LSPINE");
        public static readonly DicomCodeItem LUNG_SCT = new DicomCodeItem("43799004", "SCT", "LUNG");
        public static readonly DicomCodeItem NECK_SCT = new DicomCodeItem("45048000", "SCT", "NECK");
        public static readonly DicomCodeItem NECKCHEST_SCT = new DicomCodeItem("774007", "SCT", "NECKCHEST");
        public static readonly DicomCodeItem NOSE_SCT = new DicomCodeItem("53342003", "SCT", "NOSE");
        public static readonly DicomCodeItem ORBIT_SCT = new DicomCodeItem("363654007", "SCT", "ORBIT");
        public static readonly DicomCodeItem PATELLA_SCT = new DicomCodeItem("64234005", "SCT", "PATELLA");
        public static readonly DicomCodeItem PELVIS_SCT = new DicomCodeItem("118645006", "SCT", "PELVIS");
        public static readonly DicomCodeItem SHOULDER_SCT = new DicomCodeItem("16982005", "SCT", "SHOULDER");
        //public static readonly DicomCodeItem SKULL_SCT = new DicomCodeItem("SKULL", "", "SCT", );
        public static readonly DicomCodeItem SPINE_SCT = new DicomCodeItem("51282000", "SCT", "SPINE");
    }
}

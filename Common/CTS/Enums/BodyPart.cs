//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Enums
{
    public enum BodyPart
    {
        /// <summary>
        /// 未做任何选择
        /// </summary>
        None = 0,

        Spine = 0X100,
        Shoulder,
        Pelvis,
        Neck,
        Head,
        Leg, //LowerExtremities
        Arm, //UpperExtremities
        Abdomen,
        Breast,
        Chest,
        Lung,
        Iac,
        Eye,
        Nose,

        BBreast = 0X200,     //  “B”表示血管检查
        BHead,
        BNeck,
        BAbdomen,
        BArm,
        BLeg,
        BChest
    }
}
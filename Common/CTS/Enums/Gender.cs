//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Enums
{
    /// <summary>
    /// 患者性别
    /// Female(F) => 女性，Male(M) => 男性，Other(O) => 其他
    /// Tag(0010,0040)
    /// </summary>
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2,
    }
}
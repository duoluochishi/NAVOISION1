namespace NV.CT.Service.Common.Enums
{
    public enum InputType
    {
        /// <summary>
        /// 不做限制，任意类型
        /// </summary>
        None,

        /// <summary>
        /// 只能输入整数类型
        /// </summary>
        Integer,

        /// <summary>
        /// 只能输入非负整数类型
        /// </summary>
        UnsignedInteger,

        /// <summary>
        /// 只能输入小数类型
        /// </summary>
        Decimal,

        /// <summary>
        /// 只能输入非负小数类型
        /// </summary>
        UnsignedDecimal,
    }
}
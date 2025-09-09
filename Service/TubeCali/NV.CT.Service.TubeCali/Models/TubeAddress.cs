namespace NV.CT.Service.TubeCali.Models
{
    public class TubeAddress
    {
        /// <summary>
        /// Tube号，从1开始计数
        /// </summary>
        public int Number { get; init; }

        /// <summary>
        /// 状态寄存器地址
        /// </summary>
        public uint StatusAddress { get; init; }

        /// <summary>
        /// 电压寄存器地址
        /// </summary>
        public uint VoltageAddress { get; init; }

        /// <summary>
        /// 电流寄存器地址
        /// </summary>
        public uint CurrentAddress { get; init; }

        /// <summary>
        /// MS寄存器地址
        /// </summary>
        public uint MsAddress { get; init; }

        /// <summary>
        /// 校准操作寄存器地址
        /// </summary>
        public uint CaliAddress { get; init; }

        /// <summary>
        /// 校准操作值
        /// </summary>
        public uint CaliValue { get; init; }
    }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/15 10:39:18           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Models
{
    public class TablePositionInfo
    {
        /// <summary>
        /// 床X方向位置
        /// </summary>
        public int AxisXPosition { get; set; }

        public uint AxisXSpeed { get; set; }

        /// <summary>
        /// 床水平运动速度
        /// </summary>
        public uint HorizontalSpeed { get; set; }

        /// <summary>
        /// 床竖直运动速度
        /// </summary>
        public uint VerticalSpeed { get; set; }

        /// <summary>
        /// 床水平位置
        /// </summary>
        public int HorizontalPosition { get; set; }

        /// <summary>
        /// 床垂直位置
        /// </summary>
        public uint VerticalPosition { get; set; }

        /// <summary>
        /// 床是否锁住，不允许操作
        /// </summary>
        public bool Locked { get; set; }
    }
}

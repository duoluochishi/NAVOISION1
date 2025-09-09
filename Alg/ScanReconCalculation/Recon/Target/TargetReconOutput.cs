//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/14/04 14:02:21    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


namespace NV.CT.Alg.ScanReconCalculation.Recon.Target
{
    public class TargetReconOutput
    {
        /// <summary>
        /// 是否靶重建判断。
        /// 当前对靶重建的判断依据为：
        ///     1. centerx*centery != 0
        ///     2. 重建范围小于可重建范围
        ///     3. fov不是预定义的几种fov(506.88,337.92,253.44)
        /// </summary>
        public bool IsTargetRecon { get; set; }
        /// <summary>
        /// 要重建获得当前定义的重建区域图像所需要的数据对应床位范围，小锥角方向。
        /// </summary>
        public int TablePositionMin { get; set; }
        /// <summary>
        /// 要重建获得当前定义的重建区域图像所需要的数据对应床位范围，大锥角方向。
        /// </summary>
        public int TablePositionMax { get;set; }
        /// <summary>
        /// 靶重建图像中心点在设备坐标系下的坐标。X
        /// </summary>
        public int roiFovCenterX { get; set; }
        /// <summary>
        /// 靶重建图像中心点在设备坐标系下的坐标。Y
        /// </summary>
        public int roiFovCenterY { get; set; }

        //public int TotalFov { get; set; }     //当前totalFOV直接使用ScanFOV

    }
}

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.DicomUtility.BlackImageForSR
{
    public class ACQDoseInfoForBlackImage
    {
        public string SeriesInstanceUID { get; set; } = string.Empty;

        public int Index { get; set; } = -1;
        public string SeriesDescription { get; set; } = string.Empty;

        public string ScanMode { get; set; } = string.Empty;

        public double MAs { get; set; } = -1;

        public double KV { get; set; } = -1;

        public int Cycles { get; set; } = -1;
        public double RotateTime { get; set; } = -1;

        public double CTDIvol { get; set; } = -1;

        public double DLP { get; set; } = -1;

        public string PhantomType { get; set; } = string.Empty;
    }
}

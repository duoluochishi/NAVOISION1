//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/10 9:25:12     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension;
/// <summary>
/// 重建图像数量计算方法，根据当前协议中的重建任务参数计算图像数量。
/// </summary>
public static class ReconImageNumHelper
{
    public static int GetReconImageNum(ReconModel recon)
    {
        var reconLength = (recon.CenterFirstZ - recon.CenterLastZ);
        var increment = recon.ImageIncrement;
        var imageNum = Math.Abs((int)(reconLength / increment));
        imageNum = imageNum >= 1 ? imageNum : 1;

        return imageNum;
    }
}
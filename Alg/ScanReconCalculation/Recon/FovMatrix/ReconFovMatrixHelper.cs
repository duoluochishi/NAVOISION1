using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Alg.ScanReconCalculation.Recon.FovMatrix;
public static class ReconFovMatrixHelper
{
    public static readonly int DefinedMinPixelSpacing = SystemConfig.OfflineReconParamConfig.OfflineReconParam.DefinedMinPixelSpacing.Value;
    public static readonly int DefinedHDMinPixelSpacing = SystemConfig.OfflineReconParamConfig.OfflineReconParam.DefinedHDMinPixelSpacing.Value;

    public static readonly int PredefinedHeadFov = 337920;
    public static readonly int PreDefinedFullFov = 506880;

    /// <summary>
    /// 获取当前系统定义最小Matrix
    /// </summary>
    /// <returns></returns>
    public static int GetMinMatrix()
    {
        return SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.Min();
    }

    /// <summary>
    /// 获取当前系统定义最大Matrix
    /// </summary>
    /// <returns></returns>
    public static int GetMaxMatrix()
    {
        return SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.Max();
    }

    /// <summary>
    /// 获取当前像素定义下最小FOV,单位nm
    /// </summary>
    /// <returns></returns>
    public static int GetMinFov(bool isHD = false)
    {
        var targetPixelSpacing = isHD ? DefinedHDMinPixelSpacing : DefinedMinPixelSpacing;
        return GetMinMatrix() * targetPixelSpacing;
    }

    /// <summary>
    /// 获取当前像素定义下最大FOV,单位nm;
    /// 临时使用定义的全FOV，当前没有使用
    /// </summary>
    /// <returns></returns>
    public static int GetMaxFov(bool isHD = false)
    {
        return PreDefinedFullFov;
        //return GetMaxMatrix() * DefinedMinPixelSpacing;
    }

    /// <summary>
    /// 当前定义：系统有设置最小像素尺寸。
    /// 当参数中Fov发生变化时，若在当前矩阵下像素尺寸比最小像素尺寸小，则需要调整matrix以满足最小像素要求。
    /// 逻辑为得到能够满足最小像素尺寸的最大矩阵
    /// </summary>
    /// <param name="fov"></param>
    /// <param name="oriMatrix"></param>
    /// <returns></returns>
    public static int GetSuitableMatrix(double fov, int oriMatrix,bool isHD = false)
    {
        var targetPixelSpacing = isHD? DefinedHDMinPixelSpacing : DefinedMinPixelSpacing;
        var oriPixelSpacing = fov / oriMatrix;
        if (oriPixelSpacing > targetPixelSpacing)
        {
            return oriMatrix;
        }

        var matrixList = SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges.ToArray().OrderDescending();

        foreach (var matrix in matrixList)
        {
            var newPixelSpacing = fov / matrix;
            if (newPixelSpacing < targetPixelSpacing)
                continue;
            return matrix;
        }

        return matrixList.Min();
    }

    public static List<int> GetSuitableMatrixList(int currentFov, bool isHD = false)
    {
        var targetPixelSpacing = isHD ? DefinedHDMinPixelSpacing : DefinedMinPixelSpacing;

        List<int> list = new List<int>();
        foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges)
        {
            var oriPixelSpacing = currentFov / item;
            if (oriPixelSpacing >= targetPixelSpacing)
            {
                list.Add(item);
            }
        }
        return list;
    }

    public static double GetSuitableFOVTemp(double currentFov)
    {
        if(currentFov <= PredefinedHeadFov)
        {
            return PredefinedHeadFov;
        }
        return PreDefinedFullFov;        
    }
}
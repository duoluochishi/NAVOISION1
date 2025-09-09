//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/16 14:14:32     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Models;

public class PrintingImageProperty
{
    public string SeriesUID { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public List<MedROI> ROIList;

    public MedViewProperty ViewProperty;

    public PrintingImageProperty()
    {
        ROIList = new List<MedROI>();
        ViewProperty = new MedViewProperty();
    }
}

public class MedROI
{
    public RoiType RoiTyppe;

    public List<NVPoint> Points;

    public MedROI()
    {
        Points = new List<NVPoint>();
    }
}

public class NVPoint
{
    public double x;

    public double y;

    public double z;
}

public class MedViewProperty
{
    public double ZoomRatio;

    public double[] MotionVector;

    public bool Azimuth;

    public double RollAngleInDegree;

    public int Ww;

    public int Wl;

    public bool WwWlInit;

    public int ExternHeightOffset;

    public int ExternHeightDirection;

    public ImageFilter Filter;

    public LookupTable LookupTable;

    public MedViewProperty()
    {
        MotionVector = new double[3];
        WwWlInit = false;
        Filter = ImageFilter.ImageFilter_None;
        LookupTable = LookupTable.LUT_BW;
        ZoomRatio = 1.0;
        MotionVector[2] = 0.0;
        MotionVector[1] = 0.0;
        MotionVector[0] = 0.0;
        Azimuth = false;
        RollAngleInDegree = 0.0;
        ExternHeightOffset = 0;
        ExternHeightDirection = 0;
    }
}

public struct ItemRect
{
    public double PosX;

    public double PosY;

    public double Width;

    public double Height;

    public ItemRect()
    {
        PosX = 0.0;
        PosY = 0.0;
        Width = 0.0;
        Height = 0.0;
    }
}




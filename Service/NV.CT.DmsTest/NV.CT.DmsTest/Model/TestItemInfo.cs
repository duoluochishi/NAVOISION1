using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace NV.CT.DmsTest.Model
{

    public enum TestItemStaus
    {
        NotTest,
        Pass,
        Error,
    }
    public enum XRaySourceNumber
    { 
        All = 0,
        XRaySource01,
        XRaySource02,
        XRaySource03,
        XRaySource04,
        XRaySource05,
        XRaySource06,
        XRaySource07,
        XRaySource08,
        XRaySource09,
        XRaySource10,
        XRaySource11,
        XRaySource12,
        XRaySource13,
        XRaySource14,
        XRaySource15,
        XRaySource16,
        XRaySource17,
        XRaySource18,
        XRaySource19,
        XRaySource20,
        XRaySource21,
        XRaySource22,
        XRaySource23,
        XRaySource24
    }


    /// <summary>
    /// 测试项中核心扫描参数，当前仅记录用户关注的参数
    /// </summary>
    public partial class CoreScanParam : ObservableObject
    {
        public uint Kv { get; set; }

        public uint Ma { get; set; }

        public uint ExpTime { get; set; }

        public uint FrameTime { get; set; }

        public Gain Gain { get; set; }

        public uint AutoDelFrame { get; set; }

        public uint TotalFrame { get; set; }

        public Service.Common.Enums.EnableType PreOffset { get; set; }

        public XRaySourceNumber XRaySourceIndex { get; set; }

        [ObservableProperty]
        public string ?rawDataPath;

        public RawDataType RawDataType { get; set; }
    }

    public enum RawDataType
    {
        UnKnownData = 0, //未知数据
        HighKvExposureData,//模组一致性测试高kv数据
        LowKvDExposureata,//模组一致性测试低KV数据
        HighmAExposureData, //信号线性度测试高mA数据
        LowmAExposureData, //信号线性度测试低mA数据
        DarkData,//短时稳定性暗场测试数据
        GainSource1ExposureData, //短时稳定性测试1号源亮场数据
        GainSource7ExposureData, //短时稳定性测试7号源亮场数据
        GainSource13ExposureData, //短时稳定性测试7号源亮场数据
        GainSource19ExposureData, //短时稳定性测试7号源亮场数据
    }

    public class ItemThreshold
    {
        public float UpperLimit { get; set; }
        public float LowLimit { get; set; }
        public int PixelNumber { get; set; }    
    }


    public partial class TestItemInfo
    {
        
        public string TestItemName { get; set; } = string.Empty;
        public List<CoreScanParam> CoreScanParamList { get; set; } = new List<CoreScanParam>();
        public ItemThreshold ?ItemThreshold { get; set; }    
    }

    public partial class DMSTestConfig
    {
        public List<TestItemInfo> TestItemInfoList { get; set; } = new List<TestItemInfo>();
    }
   
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;
using NV.CT.FacadeProxy.Common.Enums;
using OxyPlot;
using NV.CT.DmsTest.Model;

namespace NV.CT.DmsTest.Wrapper
{
    /// <summary>
    /// DmsTest测试类型，
    /// </summary>
    public enum DmsTestType
    {
        //DarkTest = 1,
        //GainTest = 2,
        AfterglowTest,
        ModuleConsistencyTest,
        SignalLinearityTest,
        ShortTermStabilityTest, //短时稳定性测试
    }

    
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct RawDataInfo
    {
        public NV.CT.DmsTest.Model.RawDataType RawDataType;

        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 256)]
        public string RawDataPath;
    }



    /// <summary>
    /// 校准参数，用于调用校准算法生成校准表
    /// </summary>
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct DmsTestParams
    {
        /// <summary>
        ///  Dms测试类型
        /// </summary>
        public DmsTestType TestType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public RawDataInfo[] RawDataInfoList;

        /// <summary>
        /// char[256] 计算结果根路径
        /// </summary>
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 256)]
        public string CalcResultRootPath;
    }
    /// <summary>
    /// 算法返回值结构体
    /// </summary>
    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    public struct AlgoStatusRet
    {
        /// int
        public int Status;

        /// char[256]
        [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 256)]
        public string StatusCode;
    }


    public enum ModuleStatus : Int16
    {
        OK = 1,
        POK,
        NG,
    }
    /// <summary>
    /// DMS测试计算结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ALLModuleStatus
    {
        public ALLModuleStatus()
        {
            moduleStatuseList = new ModuleStatus[64];
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public ModuleStatus[] moduleStatuseList;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AlgItemThreshold
    {
        public float UpperLimit;
        public float LowLimit;
        public int PixelNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AlgTestItemsThreshold
    {
        public AlgItemThreshold ModuleConsistencyTest;
        public AlgItemThreshold SignalLinearityTest;
        public AlgItemThreshold ShortTermStabilityTest;
        public AlgItemThreshold AfterglowTest;
    }

    public class DmsTestALGWrapper
    {
        public const int SUCCESSFUL = 0;

        [DllImport("NV.CT.DMSTestAlg.dll", EntryPoint = "InitConfig", CallingConvention = CallingConvention.Cdecl)]
        public static extern AlgoStatusRet AlgInitConfig(AlgTestItemsThreshold algTestItemsThreshold);

        [DllImport("NV.CT.DMSTestAlg.dll", EntryPoint = "RunDmsTestCalc", CallingConvention = CallingConvention.Cdecl)]
        public static extern AlgoStatusRet RunDmsTestCalc(DmsTestParams param, DmstTestCallback callback, ref ALLModuleStatus res);

        [DllImport("NV.CT.DMSTestAlg.dll", EntryPoint = "AbortDmsTestCalc", CallingConvention = CallingConvention.Cdecl)]
        public static extern AlgoStatusRet AbortDmsTestCalc();

        /// <summary>
        /// 校准回调，反馈进度
        /// </summary>
        /// <param name="curProgress"></param>
        /// <param name="sumProgress"></param>
        public delegate void DmstTestCallback(int finishValue, int totalValue);

    }
}


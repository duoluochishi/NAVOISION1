using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Utils;
using System;
using System.Runtime.InteropServices;

namespace NV.CT.Service.HardwareTest.Attachments.LibraryCallers
{
    public static class CollimatorCalibrationLibraryCaller
    {
        [DllImport("CollimatorCalibration.dll", EntryPoint = "Initialize", CallingConvention = CallingConvention.StdCall)]
        public static extern CaliResponse Initialize(string xmlConfigFilePath);

        [DllImport("CollimatorCalibration.dll", EntryPoint = "CheckAllFullOpenRawDataFile", CallingConvention = CallingConvention.StdCall)]
        public static extern CaliResponse CheckAllFullOpenRawDataFile(string rawDataFilePath);

        [DllImport("CollimatorCalibration.dll", EntryPoint = "ConfigureCalibration", CallingConvention = CallingConvention.StdCall)]
        public static extern CaliResponse ConfigureCalibration(int targetPosition, int targetCollimatorID);

        [DllImport("CollimatorCalibration.dll", EntryPoint = "ApplyCalibration", CallingConvention = CallingConvention.StdCall)]
        public static extern NextIter ApplyCalibration(string path, ref CollimatorDataSet collimatorDataSet);

        [DllImport("CollimatorCalibration.dll", EntryPoint = "Reset", CallingConvention = CallingConvention.StdCall)]
        public static extern CaliResponse Reset();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CollimatorDataSet 
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public CollimatorData[] collimatorDatas;

        public CollimatorDataSet()
        {
            collimatorDatas = new CollimatorData[24];
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CollimatorData 
    {
        //波太
        public int bowtie;
        //前遮挡
        public int frontBlade;
        //后遮挡
        public int rearBlade;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NextIter 
    {
        //状态：-1 ~ 错误中断，0 ~ 继续迭代，1 ~ 迭代完成      
        public int status;
        //校准日志信息
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string message;
        //限束器位置
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public int[] collimatorTargetPosition;
        //限束器目标位置
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public double[] collimatorDectectorPosition;

        public NextIter()
        {
            status = 0;
            message = string.Empty;
            collimatorTargetPosition = new int[24];
            collimatorDectectorPosition = new double[24];
        }

        public override string ToString()
        {
            return $"[Iteration Status] - {Enum.GetName((IterativeStatus)status)}, [Message]: {message}, " +
                $"[Target Position]: {collimatorTargetPosition.ToFormatString()}, " +
                $"[Last Detector Position]: {collimatorDectectorPosition.ToFormatString()}.";
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CaliResponse 
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string message;
        [MarshalAs(UnmanagedType.U1)]
        public bool status;
    }

}

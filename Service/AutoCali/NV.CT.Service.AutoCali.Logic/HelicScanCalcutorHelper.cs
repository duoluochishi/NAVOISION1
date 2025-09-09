using Newtonsoft.Json;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Helic;
using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Helic;
using NV.CT.FacadeProxy.Common.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic
{
    public class HelicScanCalcutorHelper
    {
        public ScanOption ScanOption { get; set; } = ScanOption.Helical;

        public ScanMode ScanMode { get; set; } = ScanMode.Plain;

        public TableDirection TableDirection { get; set; } = TableDirection.In;

        public double ReconVolumeBeginPos { get; set; }

        public double ReconVolumeEndPos { get; set; }

        /// <summary>
        /// 帧间隔时间，单位us，默认10000us（即10ms）
        /// </summary>
        public double FrameTime { get; set; } = 10;//10ms

        public int FramesPerCycle { get; set; } = 360;

        public int PreIgnoredFrames { get; set; } = 0;

        /// <summary>
        /// 探测器每一排宽度（0.1mm），固定0.165 * 10
        /// </summary>
        public static readonly double SliceWidth = 0.165 * 10;//单位：0.1mm
        public static readonly double FullSliceWidth = 288 * SliceWidth;

        /// <summary>
        /// 探测器准直器开度排数，常用242排，有效范围：0 - 288
        /// </summary>
        public double CollimatedSliceWidth { get; set; }


        public double TableFeed { get; set; } = 300;

        public double TableAcc { get; set; } = 2000;

        public double ExposureTime { get; set; } = 5000;

        public double Pitch { get; set; } = 1.0;

        /// <summary>
        /// 每次曝光时使用的源个数，有效范围：1 - 24
        /// </summary>
        public int ExpSourceCount { get; set; } = 1;


        /// <summary>
        /// 系统支持的最大曝光源个数，有效范围：1 - 24
        /// </summary>
        public int TotalSourceCount { get; set; } = 24;

        public static void CalcTableControl()
        {
            //TableControlInput
            //{
            //    ""ScanOption"": 3,
            //    ""ScanMode"": 1,
            //    ""TableDirection"": 1,
            //    ""ReconVolumeBeginPos"": -10000,
            //    ""ReconVolumeEndPos"": -11000,
            //    ""FrameTime"": 10000,
            //    ""FramesPerCycle"": 360,
            //    ""PreIgnoredFrames"": 0,
            //    ""CollimatedSliceWidth"": 422.4,
            //    ""FullSliceWidth"": 475.2,
            //    ""TableFeed"": 300,
            //    ""TableAcc"": 2000,
            //    ""ExposureTime"": 5000,
            //    ""Pitch"": 1,
            //    ""ExpSouceCount"": 1,
            //    ""TotalSouceCount"": 24
            //}"
            string path = @"D:\CodeV08\20240402-mcs-calc-clinic-TableInput.txt";
            string strTableInput = File.ReadAllText(path);
            var tableInput = JsonConvert.DeserializeObject<TableControlInput>(strTableInput);

            double expLength = 102 * 10;// tableInput.ReconVolumeEndPos - tableInput.ReconVolumeBeginPos;
            tableInput.ReconVolumeEndPos = (tableInput.ReconVolumeBeginPos - expLength);
            tableInput.FramesPerCycle = 540;
            tableInput.CollimatedSliceWidth = 242 * HelicScanCalcutorHelper.SliceWidth;
            tableInput.Pitch = 0.5;

            //GantryControlInput
//            {
//                "ScanOption": 3,
//    "ScanMode": 1,
//    "TubePositions": [
//        0,
//        0
//    ],
//    "CurrentGantryPos": 52237,
//    "OilTem": [
//        29.700000762939453,
//        29.799999237060547,
//        29.600000381469727,
//        30.100000381469727,
//        30,
//        30,
//        30.399999618530273,
//        30.200000762939453,
//        30,
//        30.100000381469727,
//        29.799999237060547,
//        29.700000762939453,
//        29.5,
//        29.600000381469727,
//        29.600000381469727,
//        29.799999237060547,
//        30.200000762939453,
//        30.100000381469727,
//        30.299999237060547,
//        30,
//        30.200000762939453,
//        30,
//        29.700000762939453,
//        29.600000381469727
//    ],
//    "HeatCaps": [
//        3.614488363265991,
//        0,
//        0,
//        0,
//        3.614488363265991,
//        0,
//        0,
//        0,
//        3.614488363265991,
//        0,
//        0,
//        0,
//        3.6157002449035645,
//        0,
//        0,
//        0,
//        3.614488363265991,
//        0,
//        0,
//        0,
//        3.614488363265991,
//        0,
//        0,
//        0
//    ],
//    "PreIgnoredN": 0,
//    "FrameTime": 10000,
//    "FramesPerCycle": 360,
//    "TotalSouceCount": 24,
//    "ExpSouceCount": 1,
//    "NumOfScan": 1,
//    "TableFeed": 300,
//    "TableAcc": 2000,
//    "TableSpeed": 117.33333333333333,
//    "DataBeginPos": -9762.4,
//    "DataEndPos": -11184.8,
//    "GantryAcc": 2700
//}

            var tableOutput = new Alg.ScanReconCalculation.Scan.Table.Helic.HelicTableCalculator().CalculateTableControlInfo(tableInput);
            var jsonTableOutput = JsonConvert.SerializeObject(tableOutput);
            File.WriteAllText(path + "-ToOutput", jsonTableOutput);

            var input = new GantryControlInput();
            input.FrameTime = tableInput.FrameTime;
            input.FramesPerCycle = tableInput.FramesPerCycle;
            input.PreIgnoredN = 0;// ignoredN;
            input.TotalSourceCount = 1;// totalSouceCount;
            input.ExpSourceCount = 24;// expSouceCount;
            input.GantryAcc = 2700;// gantryAcc;
            input.NumOfScan = 1;// numOfScan;
            input.TableFeed = tableInput.TableFeed;//.tab;// tableFeed;
            input.TableAcc = tableInput.TableAcc;// tableAcc;

            var gantryOutput = new HelicGantryCalculator().GetGantryControlOutput(input);
            //return tableOutput;
        }
    }
}

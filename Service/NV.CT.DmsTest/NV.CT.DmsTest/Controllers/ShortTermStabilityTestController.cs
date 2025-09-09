using NV.CT.DmsTest.Model;
using NV.CT.DmsTest.Utilities;
using NV.CT.DmsTest.Wrapper;
using NV.CT.Service.Common.Models.ScanReconModels;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;



namespace NV.CT.DmsTest.Controllers
{
    internal class ShortTermStabilityTestController : AbstractTestController
    {
        public AutoResetEvent? AutoResetEvent;



        /// <summary>
        ///   检测测试环境是否就绪
        /// </summary>
        /// <returns></returns>
        public override bool CheckTestEnvironment()
        {

            return true;
        }

        /// <summary>
        /// 绘制图表
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<PlotModel> DrawChart()
        {
            List<PlotModel> plotModels = new List<PlotModel>();

            var createAndAddPlotModel = (string fileName) =>
            {

                var bytes = File.ReadAllBytes(fileName);

                float[] moduleRatio = new float[bytes.Length / sizeof(float)];
                Buffer.BlockCopy(bytes, 0, moduleRatio, 0, bytes.Length);

                var scatterSeries = new ScatterSeries();
                scatterSeries.MarkerType = MarkerType.Circle;
                PlotModel plotModel = new PlotModel();

                for (int i = 0; i < moduleRatio.Length; i++)
                {
                    scatterSeries.Points.Add(new ScatterPoint(i + 1, moduleRatio[i]));
                }
                plotModel.Series.Add(scatterSeries);
                plotModel.Title = fileName.Substring(0, fileName.Length - 4);
                plotModels.Add(plotModel);
            };
            string calcResultFolder = GenerateTestResultPath(TestItemInfo!.CoreScanParamList!.FirstOrDefault()!.RawDataPath!);

            string dark1sAnd31sModuleRelativeErrorFileName = Path.Combine(calcResultFolder, "Dark_(1s_31s)_relativeError.raw");
            if (File.Exists(dark1sAnd31sModuleRelativeErrorFileName))
            {
                createAndAddPlotModel(dark1sAnd31sModuleRelativeErrorFileName);
            }

            string dark1sAnd60sModuleRelativeErrorFileName = Path.Combine(calcResultFolder, "Dark_(1s_60s)_relativeError.raw");
            if (File.Exists(dark1sAnd60sModuleRelativeErrorFileName))
            {
                createAndAddPlotModel(dark1sAnd60sModuleRelativeErrorFileName);
            }

            string gain1sAnd31sModuleRelativeErrorFileName = Path.Combine(calcResultFolder, "Gain_(1s_31s)_relativeError.raw");
            if (File.Exists(gain1sAnd31sModuleRelativeErrorFileName))
            {
                createAndAddPlotModel(gain1sAnd31sModuleRelativeErrorFileName);
            }

            string gain1sAnd60sModuleRelativeErrorFileName = Path.Combine(calcResultFolder, "Gain_(1s_60s)_relativeError.raw");
            if (File.Exists(gain1sAnd60sModuleRelativeErrorFileName))
            {
                createAndAddPlotModel(gain1sAnd60sModuleRelativeErrorFileName);
            }

            return plotModels;
        }
        public override bool RunTest(bool isDebug)
        {
            string message = string.Empty;
            message = $"{TestItemInfo?.TestItemName} Start!";
            ConsoleLogPrinter.Info(message);
            var result = CheckTestEnvironment();
            if (!result)
            {
                return false;
            }

            AutoResetEvent = new AutoResetEvent(false);
            moduleTestStatusesList.Clear();
            bool currentTestStatus = true;
            Task t = new Task(() =>
            {

                if (!isDebug)
                {
                    //一个测试项有多组扫描，因此需要使用一个StudyId，
                    ScanReconParamModel scanReconParamModel = new ScanReconParamModel();
                    scanReconParamModel.Study = new StudyModel();
                    scanReconParamModel.Study.StudyID = $"{DateTime.UtcNow.ToString()}_{TestItemInfo!.TestItemName}";


                    foreach (var item in TestItemInfo!.CoreScanParamList!)
                    {
                        scanReconParamModel.ScanParameter = ProxyWrapper.ToFreeProtocolScanParam(item);
                        scanReconParamModel.ScanParameter.ScanUID = DateTime.UtcNow.ToString() + item.RawDataType.ToString();
                        result = ProxyWrapper.Instance.RunScan(scanReconParamModel);

                        if (!result)
                        {
                            Logger.Error(String.Format(" RunScan {0}", ProxyWrapper.Instance.GetErrorInfo()));
                            currentTestStatus = false;
                            return;
                        }
                    }
                }
                message = $"{TestItemInfo!.TestItemName} ALG Start !";
                ConsoleLogPrinter.Info(message);
                DmsTestParams dmsTestParams = new DmsTestParams();
                dmsTestParams.TestType = (DmsTestType)Enum.Parse(typeof(DmsTestType), TestItemInfo!.TestItemName);
                dmsTestParams.RawDataInfoList = new RawDataInfo[5];
                for (int i = 0; i < TestItemInfo!.CoreScanParamList.Count; i++)
                {
                    dmsTestParams.RawDataInfoList[i].RawDataType = TestItemInfo!.CoreScanParamList[i].RawDataType;
                    dmsTestParams.RawDataInfoList[i].RawDataPath = TestItemInfo!.CoreScanParamList[i].RawDataPath!;
                }

                // 获取上一级目录

                string calcResultPath = GenerateTestResultPath(TestItemInfo!.CoreScanParamList.FirstOrDefault()!.RawDataPath!);
                Directory.CreateDirectory(calcResultPath);
                dmsTestParams.CalcResultRootPath = calcResultPath;
                ALLModuleStatus aLLModuleStatus = new ALLModuleStatus();
                var res = DmsTestALGWrapper.RunDmsTestCalc(dmsTestParams, DmsTestCallback, ref aLLModuleStatus);
                ConverToTestStatus(aLLModuleStatus);
                if (res.Status != 0)
                {
                    Logger.Error(String.Format(" RunDmsTestCalc {0}", res.StatusCode));
                    currentTestStatus = false;
                    return;
                }

                this.TestItemStaus = Model.TestItemStaus.Pass;
                message = $"{TestItemInfo.TestItemName} ALG End !";
                ConsoleLogPrinter.Info(message);
            });
            t.Start();
            Task.WaitAny(t);
            message = $"{TestItemInfo!.TestItemName} End!";
            ConsoleLogPrinter.Info(message);
            return currentTestStatus;

        }
    }
}

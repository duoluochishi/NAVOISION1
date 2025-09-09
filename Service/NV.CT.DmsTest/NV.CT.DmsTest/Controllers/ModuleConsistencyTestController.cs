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
    public class ModuleConsistencyTestController : AbstractTestController
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
        /// 加载测试计算结果，并将测试结果绘制成图像
        /// 当前测试项的测试结果，是探测器64个模块的比值，
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<PlotModel> DrawChart()
        {

            List<PlotModel> plotModels = new List<PlotModel>();


            string calcResultFolder = GenerateTestResultPath(TestItemInfo!.CoreScanParamList.FirstOrDefault()!.RawDataPath!);

            string moduleRatioFileName = Path.Combine(calcResultFolder, "detectorModuleDiffKvRatio.raw");
            if (!File.Exists(moduleRatioFileName))
            {
                return plotModels;
            }
            var bytes = File.ReadAllBytes(moduleRatioFileName);

            float[] moduleRatio = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, moduleRatio, 0, bytes.Length);

            PlotModel plotModel = new PlotModel();
            var scatterSeries = new ScatterSeries();
            //scatterSeries.Title = TestItemInfo!.TestItemName;
            scatterSeries.MarkerType = MarkerType.Circle;
            scatterSeries.MarkerFill = OxyColors.Blue;
            scatterSeries.MarkerSize = 5;



            for (int i = 0; i < moduleRatio.Length; i++)
            {
                ScatterPoint scatterPoint = new ScatterPoint(i + 1, moduleRatio[i]);

                scatterSeries.Points.Add(scatterPoint);
            }

            plotModel.Series.Add(scatterSeries);

            var createAndAddLineSeries = (float limit) =>
            {
                LineSeries lineSeries = new LineSeries();
                lineSeries.LineStyle = LineStyle.Dash;
                lineSeries.Points.Add(new DataPoint(0, limit));
                lineSeries.Points.Add(new DataPoint(64, limit));

                plotModel.Series.Add(lineSeries);
            };

            createAndAddLineSeries(TestItemInfo!.ItemThreshold!.UpperLimit);
            createAndAddLineSeries(TestItemInfo.ItemThreshold.LowLimit);
            float upperLimit1 = (float)1.002;
            float lowerLimit1 = (float)0.998;
            createAndAddLineSeries(upperLimit1);
            createAndAddLineSeries(lowerLimit1);


            plotModels.Add(plotModel);


            return plotModels;
        }
        public override bool RunTest(bool isDebug)
        {
            string message = string.Empty;
            message = $"{TestItemInfo!.TestItemName} Start!";
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
                        //todo 赋值文件夹configMRS/config文件夹

                    }
                }
                message = $"{TestItemInfo.TestItemName} ALG Start !";
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
                try
                {
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
                }
                catch (Exception e)
                {
                    Logger.Error($"Failed to Execute 'RunDmsTestCalc', Exception:{e}");
                }
            });
            t.Start();
            Task.WaitAny(t);
            message = $"{TestItemInfo.TestItemName} End!";
            ConsoleLogPrinter.Info(message);
            return currentTestStatus;
        }

    }
}

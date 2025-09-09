using NV.CT.DmsTest.Utilities;
using NV.CT.DmsTest.Wrapper;
using System;
using System.Threading.Tasks;

namespace NV.CT.DmsTest.Controllers
{
    internal class AfterglowTestController : AbstractTestController
    {

        public AfterglowTestController() { }


        public override bool RunTest(bool isDebug)
        {
            string message = string.Empty;
            bool currentTestStatus = true;
            Task t = new Task(() =>
            {
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

                //string calcResultPath = GenerateTestResultPath(TestItemInfo!.CoreScanParamList.FirstOrDefault().RawDataPath);
                //Directory.CreateDirectory(calcResultPath);


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

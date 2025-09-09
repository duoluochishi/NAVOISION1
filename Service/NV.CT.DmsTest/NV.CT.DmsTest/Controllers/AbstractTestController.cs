using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.DmsTest.Model;
using NV.CT.DmsTest.Utilities;
using NV.CT.DmsTest.Wrapper;
using OxyPlot;
using System.Collections.Generic;
using System.IO;

namespace NV.CT.DmsTest.Controllers
{
    public partial  class AbstractTestController : ObservableObject
    {


        [ObservableProperty]
        public TestItemInfo? testItemInfo;


        public object? Charts { get; set; }

        [ObservableProperty]
        public bool isCheck = false;

        [ObservableProperty]
        public TestItemStaus testItemStaus = TestItemStaus.NotTest;

        public List<ModuleTestStatus> moduleTestStatusesList = new List<ModuleTestStatus>();

        protected static readonly DmsTestALGWrapper.DmstTestCallback DmsTestCallback = RecviveDmsTestAlgProgress;

        public virtual bool RunTest(bool isDebug)
        {
            return true;
        }

        public virtual bool StopTest()
        {
            return true;
        }

        public virtual bool CheckTestEnvironment()
        {
            return true;
        }


        public virtual IEnumerable<PlotModel> DrawChart()
        {
            
            return new List<PlotModel>();
        }

        /// <summary>
        /// 根据生数据的路径，找到上一级目录，并拼接成测试结果路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string GenerateTestResultPath(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            // 获取上一级目录
            DirectoryInfo parentDir = directoryInfo.Parent!;
            string calcResultPath = Path.Combine(parentDir!.FullName, "CalcResult");
            return calcResultPath;
        }
        



        protected static void RecviveDmsTestAlgProgress(int finishValue, int totalValue)
        {
            ConsoleLogPrinter.Info($" ALG Calc Progress {finishValue}/{totalValue} ");
        }

        /// <summary>
        /// 检测当前测试项各个模块，如果有一个模块不合格，则认为当前测试不通过。
        /// </summary>
        protected  void ConverToTestStatus(ALLModuleStatus aLLModuleStatus)
        {
            bool testStatus = true;

            ////更新当前测试测试结果
            //currentALLModuleTestStatusList.Clear();
            for (int j = 0; j < aLLModuleStatus.moduleStatuseList.Length; j++)
            {
                if (aLLModuleStatus.moduleStatuseList[j] == ModuleStatus.NG)
                {
                    testStatus &= false;
                }
                ModuleTestStatus moduleTestStatus = new ModuleTestStatus();
                moduleTestStatus.ChannelNo = j + 1; //探测器模组的编号默认从0开始，显示到界面上需要加1
                moduleTestStatus.CalcStatus = (CalcStatus)aLLModuleStatus.moduleStatuseList[j];
                moduleTestStatusesList.Add(moduleTestStatus);
            }
            if (testStatus)
            {
                TestItemStaus = TestItemStaus.Pass;
            }
            else
            {
                TestItemStaus = TestItemStaus.Error;
            }
        }
    }
}

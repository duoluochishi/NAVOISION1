using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.DmsTest.Controllers;
using NV.CT.DmsTest.Model;
using NV.CT.DmsTest.Tools;
using NV.CT.DmsTest.Utilities;
using NV.CT.DmsTest.View;
using NV.CT.DmsTest.Wrapper;
using NV.CT.Service.Common;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.DmsTest.ViewModel
{
    public partial class DmsTestVM : ObservableObject
    {

        [ObservableProperty]
        public List<AbstractTestController> testControllerList = new List<AbstractTestController>();

        [ObservableProperty]
        public ObservableCollection<ModuleTestStatus> currentALLModuleTestStatusList = new ObservableCollection<ModuleTestStatus>();

        [ObservableProperty]
        public string runLogMessage = string.Empty;

        [ObservableProperty]
        public ObservableCollection<CoreScanParam>? coreScanParamList = new ObservableCollection<CoreScanParam>();


        [ObservableProperty]
        private int testItemSelectedIndex = 0;


        [ObservableProperty]
        private bool isDebugMode = false;

        public DmsTestVM()
        {
            InitTestController();
            ProxyWrapper.Instance.InitRegisteEvent();
            StrongReferenceMessenger.Default.Register<string>(this, OnRecviverMessage);
        }



        private void InitTestController()
        {
            try
            {
                Logger.Info($"[{nameof(InitTestController)}] Entered.");
                //Debugger.Launch();
                var config = Config.Config.Instance.LoadConfig();
                for (int j = 0; j < config.TestItemInfoList!.Count; j++)
                {
                    var controller = ReflectionHelper.CreateInstance<AbstractTestController>("NV.CT.DmsTest." +
                             "Controllers." + config.TestItemInfoList[j].TestItemName + "Controller");
                    controller.TestItemInfo = config.TestItemInfoList[j];

                    controller.TestItemStaus = TestItemStaus.NotTest;
                    Logger.Info(String.Format("testItem Name {0}", controller.TestItemInfo.TestItemName));
                    TestControllerList!.Add(controller);
                }
                InitAlgConfig(config);
                CoreScanParamList = new ObservableCollection<CoreScanParam>(TestControllerList![TestItemSelectedIndex].TestItemInfo!.CoreScanParamList);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }
        /// <summary>
        /// 初始化算法配置
        /// </summary>
        /// <param name="dmsTestConfig"></param>
        private void InitAlgConfig(DMSTestConfig dmsTestConfig)
        {
            AlgTestItemsThreshold algTestItemsThreshold = new AlgTestItemsThreshold();

            var converToAlgItemThreshold = (ItemThreshold itemThreshold, out AlgItemThreshold algItemThreshold) =>
            {
                algItemThreshold.UpperLimit = itemThreshold.UpperLimit;
                algItemThreshold.LowLimit = itemThreshold.LowLimit;
                algItemThreshold.PixelNumber = itemThreshold.PixelNumber;
            };
            converToAlgItemThreshold(dmsTestConfig.TestItemInfoList[0].ItemThreshold!, out algTestItemsThreshold.ModuleConsistencyTest);
            converToAlgItemThreshold(dmsTestConfig.TestItemInfoList[1].ItemThreshold!, out algTestItemsThreshold.SignalLinearityTest);
            converToAlgItemThreshold(dmsTestConfig.TestItemInfoList[2].ItemThreshold!, out algTestItemsThreshold.ShortTermStabilityTest);
            converToAlgItemThreshold(dmsTestConfig.TestItemInfoList[3].ItemThreshold!, out algTestItemsThreshold.AfterglowTest);
            Wrapper.DmsTestALGWrapper.AlgInitConfig(algTestItemsThreshold);
        }



        #region "command"
        [RelayCommand]
        public void TestGo()
        {
            string message = string.Empty;

            //Logger.Info("TestGo Start!");
            ConsoleLogPrinter.Info("Test Start!");


            bool res = DialogService.Instance.ShowConfirm("请确认扫描门是否关闭");
            if (!res)
            {
                ConsoleLogPrinter.Warn("The door not colsed, Test finished!");
                return;
            }
            Task.Run(() =>
            {
                for (int i = 0; i < TestControllerList!.Count; i++)
                {
                    var item = TestControllerList![i];
                    if (!item.IsCheck)
                    {
                        continue;
                    }
                    item.moduleTestStatusesList.Clear();
                    TestItemSelectedIndex = i;
                    bool result = item.RunTest(IsDebugMode);
                    if (!result)
                    {
                        item.TestItemStaus = TestItemStaus.Error;
                        message = $"{item.TestItemInfo?.TestItemName} Run Failed";
                        Logger.Error(message);
                        continue;
                    }
                    //item.TestItemStaus = TestItemStaus.Pass;
                    CurrentALLModuleTestStatusList = new ObservableCollection<ModuleTestStatus>(item.moduleTestStatusesList);
                }
            });
            ConsoleLogPrinter.Info("Test End!");
        }



        [RelayCommand]
        private void TestStop()
        {
            var result = MessageBox.Show("提示信息", "请确认是否结束测试", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.OK)
            {
                return;
            }
            Logger.Info("User  Confirmation Stop Test!");
        }


        /// <summary>
        /// 测试项列表变化，切换对应的参数列表
        /// </summary>
        [RelayCommand]
        public void TestItemListSelection()
        {
            if (TestControllerList == null)
            {
                return;
            }
            CoreScanParamList = new ObservableCollection<CoreScanParam>(TestControllerList![TestItemSelectedIndex].TestItemInfo!.CoreScanParamList);
            CurrentALLModuleTestStatusList = new ObservableCollection<ModuleTestStatus>(TestControllerList![TestItemSelectedIndex].moduleTestStatusesList);
        }

        /// <summary>
        /// 生成测试项图像
        /// </summary>
        [RelayCommand]
        public void GenerateChart()
        {
            try
            {
                if (TestControllerList![TestItemSelectedIndex].TestItemStaus == TestItemStaus.Pass)
                {
                    var plotModels = TestControllerList![TestItemSelectedIndex].DrawChart();

                    TestItemChartPlotView testItemChartPlotView = new TestItemChartPlotView(plotModels);

                    testItemChartPlotView.Show();
                }
                else
                {
                    DialogService.Instance.ShowConfirm("Current TestItem Test Status Exception!");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        [RelayCommand]
        private void RunLogMessageClick()
        {
            StringBuilder.Clear();
            RunLogMessage = StringBuilder.ToString();
        }

        private StringBuilder StringBuilder = new StringBuilder();
        #endregion
        private void OnRecviverMessage(object recipient, string message)
        {

            StringBuilder.AppendLine(message);
            RunLogMessage = StringBuilder.ToString();
        }

    }
}

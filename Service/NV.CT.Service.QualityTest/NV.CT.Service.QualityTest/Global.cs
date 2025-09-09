using System;
using System.IO;
using NV.CT.Service.QualityTest.Enums;
using NV.MPS.Environment;

namespace NV.CT.Service.QualityTest
{
    public static class Global
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        /// <summary>
        /// 当前选中测试项目的类别
        /// </summary>
        public static QTType CurrentQTType { get; set; }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        #region Const

        public const string ServiceAppName = "QualityTest";
        public static readonly string ConfigPath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "QualityTest");
        public const string AppSettingFile = "appsetting.json";
        public const string ConfigNode = "Config";
        public const string PhantomNode = "Phantom";
        public const string ItemsNode = "Items";
        public const string ParamDisplayNode = "ParamDisplay";
        public static readonly string ItemParamPath = Path.Combine(ConfigPath, "ItemParam");
        private static readonly string DataFolderPath = Path.Combine(RuntimeConfig.Console.ServiceData.Path, "QualityTest");
        public static readonly string ReportLastSessionDatePath = Path.Combine(DataFolderPath, "LastSessionDate.txt");
        public static readonly string HistoryFolderPath = Path.Combine(DataFolderPath, "History", $"History_{DateTime.Now:yyyyMMdd-HH-mm-ss}");
        public static readonly string History_ItemsPath = Path.Combine(HistoryFolderPath, "Items.json");
        public static readonly string ReportFilePath = Path.Combine(DataFolderPath, "Report", $"Performance_{DateTime.Now:yyyyMMdd-HH-mm-ss}.html");

        #endregion
    }
}
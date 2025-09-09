using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace NV.CT.Service.HardwareTest.Models.Components.Detector
{
    public partial class DetectorSource : AbstractSource
    {
        public DetectorSource()
        {
            InitializeProperties();
        }

        #region Initialize

        private void InitializeProperties() 
        {
            ProcessBoards = new(
                Enumerable.Range(1, 4).Select(t => new ProcessBoard() {Index = t, Name = $"{nameof(ProcessBoard)}-{t.ToString("00")}" }));
            DetectBoards = new(
                Enumerable.Range(1, 4).Select(t => new DetectBoard() {Index = t, Name = $"{nameof(DetectBoard)}-{t.ToString("00")}" }));
            TemperatureControlBoard = new();
            TransmissionBoard = new();
        }

        #endregion

        #region Properties

        [ObservableProperty]
        private int index;
        //采集模式
        [ObservableProperty]
        private DetAcqMode acquisitionMode = DetAcqMode.ExternalTriggerMode;
        //4个处理板
        [XmlIgnore]
        public ObservableCollection<ProcessBoard> ProcessBoards { get; set; } = null!;
        //4个检出板
        public ObservableCollection<DetectBoard> DetectBoards {  get; set; } = null!;
        //1个温控板
        [ObservableProperty]
        private TemperatureControlBoard temperatureControlBoard = null!;
        //1个传输板
        [ObservableProperty]
        private TransmissionBoard transmissionBoard = null!;

        public string TemperatureInformation => 
            $"ProcessBoard: {string.Join(",", ProcessBoards.Select(t => t.Temperature))}; " +
            $"DetectBoard: Up {string.Join("_", DetectBoards.Select(t => t.UpTemperature))};  Down {string.Join("_", DetectBoards.Select(t => t.DownTemperature))}; " +
            $"TemperatureControlBoard: {string.Join(",", TemperatureControlBoard.PowerValues)};";

        #endregion

        public override void ResetOnlineStatus()
        {
            base.ResetOnlineStatus();
            //重置处理板状态
            ProcessBoards.ForEach(t => t.Status = PartStatus.Disconnection);
            //重置检出板状态
            DetectBoards.ForEach(t => t.Status = PartStatus.Disconnection);
            //温控板状态
            TemperatureControlBoard.Status = PartStatus.Disconnection;
            //传输板状态
            TransmissionBoard.Status = PartStatus.Disconnection;
        }

    }

    /// <summary>
    /// 处理板
    /// </summary>
    public partial class ProcessBoard : ObservableObject
    {
        [ObservableProperty]
        private int index;
        [ObservableProperty]
        private string name = nameof(ProcessBoard);
        [ObservableProperty]
        private float temperature;
        [ObservableProperty]
        private PartStatus status = PartStatus.Disconnection;
        [ObservableProperty]
        private string firmwareVersion = "0.0.0.0";
    }

    /// <summary>
    /// 检出板
    /// </summary>
    public partial class DetectBoard : ObservableObject
    {
        public DetectBoard()
        {
        }

        [ObservableProperty]
        private int index;
        [ObservableProperty]
        private string name = nameof(DetectBoard);
        [ObservableProperty]
        private float upTemperature;
        [ObservableProperty]
        private float downTemperature;
        [ObservableProperty]
        private PartStatus status = PartStatus.Disconnection;
        [ObservableProperty]
        private string firmwareVersion = "0.0.0.0";
    }

    /// <summary>
    /// 温控板
    /// </summary>
    public partial class TemperatureControlBoard : ObservableObject
    {
        public TemperatureControlBoard()
        {
            PowerValues = new int[4] { 0, 0, 0, 0 };
        }

        [ObservableProperty]
        public string name = nameof(TemperatureControlBoard);
        [ObservableProperty]
        public int[] powerValues = null!;
        [ObservableProperty]
        public float humidity;
        [ObservableProperty]
        public PartStatus status = PartStatus.Disconnection;
        [ObservableProperty]
        public string firmwareVersion = "0.0.0.0";
    }

    /// <summary>
    /// 传输板
    /// </summary>
    public partial class TransmissionBoard : ObservableObject
    {
        [ObservableProperty]
        public string name = nameof(TransmissionBoard);
        [ObservableProperty]
        public PartStatus status = PartStatus.Disconnection;
        [ObservableProperty]
        public string firmwareVersion = "0.0.0.0";
    }

}

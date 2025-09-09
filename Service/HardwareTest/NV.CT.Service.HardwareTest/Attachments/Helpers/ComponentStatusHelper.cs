using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Fields;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Models.Components.Gantry;
using NV.CT.Service.HardwareTest.Models.Components.Table;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;
using NV.CT.Service.HardwareTest.Share.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class ComponentStatusHelper
    {
        /// <summary>
        /// 更新部件状态
        /// </summary>
        /// <param name="source"></param>
        /// <param name="abstractPart"></param>
        private static void UpdateComponentStatus(AbstractSource source, AbstractPart abstractPart)
        {
            source.Online = (abstractPart.Status is PartStatus.Normal) ? XOnlineStatus.Online : XOnlineStatus.Offline;
            source.HasErrors = abstractPart.HasErrors;
            source.ErrorMessage = abstractPart.ErrorMessage;
        }

        /// <summary>
        /// 更新主要部件状态
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="deviceSystem"></param>
        public static void UpdateMajorComponentStatusByCycle(IEnumerable<AbstractSource> sources, DeviceSystem deviceSystem) 
        {
            foreach (AbstractSource source in sources) 
            {
                switch (source) 
                {
                    case PDUSource pdu: UpdateComponentStatus(pdu, deviceSystem.PDU); break;
                    case CTBoxSource ctbox: UpdateComponentStatus(ctbox, deviceSystem.CTBox); break;
                    case IFBoxSource ifbox: UpdateComponentStatus(ifbox, deviceSystem.IFBox); break;
                    case GantrySource gantry: UpdateComponentStatus(gantry, deviceSystem.Gantry); break;
                    case AuxBoardSource auxboard: UpdateComponentStatus(auxboard, deviceSystem.AuxBoard); break;
                    case ExtBoardSource extboard: UpdateComponentStatus(extboard, deviceSystem.ExtBoard); break;
                    case TableSource table: UpdateComponentStatus(table, deviceSystem.Table); break;
                    case ControlBoxSource controlbox: UpdateComponentStatus(controlbox, deviceSystem.ControlBox); break;
                }
            }
        }

        /// <summary>
        /// 更新tube板状态
        /// </summary>
        public static void UpdateTubeInterfaceStatusByCycle(IEnumerable<TubeInterfaceSource> sources, DeviceSystem deviceSystem) 
        {
            for (var i = 0; i < deviceSystem.TubeIntfs.Length; i++)
            {
                var source = sources.ElementAt(i);
                var tubeInterface = deviceSystem.TubeIntfs[i];
                UpdateComponentStatus(source, tubeInterface);
            }
        }

        /// <summary>
        /// 更新射线源热容/油温状态
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="deviceSystem"></param>
        public static void UpdateXRaySourceStatusByCycle(IEnumerable<XRayOriginSource> sources, DeviceSystem deviceSystem) 
        {         
            for (int i = 0; i < deviceSystem.XRaySources.Length; i++)
            {
                var source = sources.ElementAt(i);
                var tubeDevice = deviceSystem.XRaySources[i];
                //更新热容、油温
                source.HeatCapacity = tubeDevice.XRaySourceHeatCap;
                source.OilTemperature = tubeDevice.XRaySourceOilTemp * Coefficients.ReduceCoef_10;
            }
        }
   
        /// <summary>
        /// 更新采集卡状态
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="deviceSystem"></param>
        public static void UpdateAcquisitionCardStatusByCycle(IEnumerable<AcquisitionCardSource> sources, DeviceSystem deviceSystem) 
        {
            //探测器
            var detector = deviceSystem.Detector;
            //获取采集卡状态
            for (int i = 0; i < detector.AcqCards.Length; i++)
            {
                var source = sources.ElementAt(i);
                var acqCardDevice = detector.AcqCards[i];
                //更新
                source.Status = acqCardDevice.MemoryInterfaceStatus;
                source.Temperature = acqCardDevice.Temperature;
            }
        }

        /// <summary>
        /// 更新探测器状态
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="deviceSystem"></param>
        public static void UpdateDetectorStatusByCycle(IEnumerable<DetectorSource> sources, DeviceSystem deviceSystem) 
        {
            //探测器
            var detector = deviceSystem.Detector;
            //获取探测器状态
            for (int i = 0; i < detector.DetectorModules.Length; i++)
            {
                var source = sources.ElementAt(i);
                //Devices 反馈
                var processBoard = detector.DetectorModules[i].ProcessingBoards;
                var detectBoard = detector.DetectorModules[i].DetectBoards;
                var temperatureControlBoard = detector.DetectorModules[i].TemperatureControlBoard;
                var transmissionBoardStatus = detector.DetectorModules[i].TransmissionBoardStatus;
                //4块处理板温度、状态
                for (int p = 0; p < source.ProcessBoards.Count(); p++) 
                {
                    source.ProcessBoards[p].Temperature = processBoard[p].Temperature * Coefficients.ReduceCoef_10;
                    source.ProcessBoards[p].Status = processBoard[p].Status;
                }
                //4块检出板上行、下行温度
                for (int d = 0; d < source.DetectBoards.Count(); d++)
                {
                    source.DetectBoards[d].UpTemperature = detectBoard[d].Chip1Temperature * Coefficients.ReduceCoef_10;
                    source.DetectBoards[d].DownTemperature = detectBoard[d].Chip2Temperature * Coefficients.ReduceCoef_10;
                }
                //温控板功率、湿度、状态
                source.TemperatureControlBoard.PowerValues = temperatureControlBoard.Powers.ToArray();
                source.TemperatureControlBoard.Humidity = temperatureControlBoard.Humidity * Coefficients.ReduceCoef_10;
                source.TemperatureControlBoard.Status = temperatureControlBoard.Status;
                //传输板状态
                source.TransmissionBoard.Status = transmissionBoardStatus;
                //模式
                source.AcquisitionMode = detector.DetectorModules[i].DetAcqMode;
            }
        }

        /// <summary>
        /// 更新床状态
        /// </summary>
        /// <param name="source"></param>
        /// <param name="deviceSystem"></param>
        public static void UpdateTableStatusByCycle(TableSource source, DeviceSystem deviceSystem)
        {
            var tableDevice = deviceSystem.Table;
            //系数
            const float TransfromCoef = 0.001f;
            //更新扫描床状态
            source.HorizontalVelocity = tableDevice.HorizontalSpeed * TransfromCoef;
            source.HorizontalPosition = tableDevice.HorizontalPosition * TransfromCoef;
            source.VerticalVelocity = tableDevice.VerticalSpeed * TransfromCoef;
            source.VerticalPosition = tableDevice.VerticalPosition * TransfromCoef;
            source.AxisXVelocity = tableDevice.AxisXSpeed * TransfromCoef;
            source.AxisXPosition = tableDevice.AxisXPosition * TransfromCoef;
            //更新在线状态
            UpdateComponentStatus(source, tableDevice);
        }

        /// <summary>
        /// 更新机架状态
        /// </summary>
        /// <param name="source"></param>
        /// <param name="deviceSystem"></param>
        public static void UpdateGantryStatusByCycle(GantrySource source, DeviceSystem deviceSystem)
        {
            var gantryDevice = deviceSystem.Gantry;
            //更新扫描架状态
            source.Position = gantryDevice.Position * 0.01;
            source.Velocity = gantryDevice.Speed * 0.01;
            //更新在线状态
            UpdateComponentStatus(source, gantryDevice);
        }

    }
}

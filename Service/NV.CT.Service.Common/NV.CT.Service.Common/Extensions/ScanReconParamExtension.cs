using System;
using System.ComponentModel;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Alg.ScanReconCalculation.Recon.Target;
using NV.CT.Alg.ScanReconCalculation.Scan.Common;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Offset;
using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common.Enums;
using NV.CT.Service.Common.Helper;
using NV.CT.Service.Common.Models.ScanReconModels;
using NV.CT.Service.Common.Utils;
using NV.CT.Service.Models;
using NV.CT.ServiceFramework;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.Common.Extensions
{
    public static class ScanReconParamExtension
    {
        #region ToModel

        public static StudyModel ToModel(this Study study)
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<StudyModel>(study);
        }

        public static ScanParamModel ToModel(this ScanParam scanParam)
        {
            return Global.Instance.ServiceProvider.GetRequiredService<IMapper>().Map<ScanParamModel>(scanParam);
        }

        #endregion

        #region Amend

        /// <summary>
        /// 修正 <seealso cref="ScanParamModel.ScanLength"/>
        /// </summary>
        public static void AmendScanLength(this ScanParamModel model)
        {
            model.ToTableControlInput(out var scanLength);
            model.ScanLength = Math.Round(scanLength, 3);
        }

        /// <summary>
        /// 修正 <seealso cref="ScanParamModel.EffectiveFrames"/>
        /// </summary>
        public static void AmendEffectiveFrames(this ScanParamModel model)
        {
            /*
             * 轴扫/定位扫/双平片正侧同时曝光: ScanTotalFrames = FramesPerCycle * 扫描圈数，非整数圈要向上取整进行修正
             * 螺旋扫: 不需要修正
             */

            switch (model.ScanOption)
            {
                case ScanOption.Surview:
                case ScanOption.DualScout:
                case ScanOption.Axial:
                {
                    model.EffectiveFrames = (uint)Math.Ceiling((double)model.EffectiveFrames / model.FramesPerCycle) * model.FramesPerCycle;
                    break;
                }
            }
        }

        /// <summary>
        /// 修正 <seealso cref="ScanParamModel.AutoDeleteNum"/>
        /// </summary>
        public static void AmendAutoDeleteNum(this ScanParamModel model)
        {
            model.AutoDeleteNum = model.ScanOption switch
            {
                ScanOption.Surview or ScanOption.DualScout => 0,
                _ => model.AutoDeleteNum
            };
        }

        /// <summary>
        /// 修正 <seealso cref="ScanParamModel.FramesPerCycle"/>
        /// </summary>
        public static void AmendFramesPerCycle(this ScanParamModel model)
        {
            model.FramesPerCycle = model.ScanOption switch
            {
                ScanOption.Surview => 1,
                ScanOption.DualScout => 2,
                _ => model.FramesPerCycle
            };
        }

        public static void AmendExposureMode(this ScanParamModel model)
        {
            model.ExposureMode = model.ScanOption switch
            {
                ScanOption.Surview or ScanOption.DualScout => ExposureMode.Single,
                _ => model.ExposureMode
            };
        }

        public static void AmendWhenScanOptionChanged(this ScanParamModel model)
        {
            model.AmendAutoDeleteNum();
            model.AmendFramesPerCycle();
            model.AmendExposureMode();
        }

        #endregion

        #region Calculate

        /// <summary>
        /// 根据 <seealso cref="ScanParamModel.EffectiveFrames"/> 计算 <seealso cref="ScanParamModel.ScanLength"/>
        /// </summary>
        public static void CalculateScanLength(this ScanParamModel model)
        {
            var effectiveWidth = Math.Round(model.CollimatorZ * ConstUtil.PerCollimatorSliceWidth, 3);

            switch (model.ScanOption)
            {
                case ScanOption.Surview:
                case ScanOption.DualScout:
                case ScanOption.Axial:
                {
                    var num = (uint)Math.Ceiling((double)model.EffectiveFrames / model.FramesPerCycle);
                    model.ScanLength = Math.Round((num - 1) * model.TableFeed + effectiveWidth, 3);
                    break;
                }
                case ScanOption.Helical:
                {
                    model.ScanLength = Math.Round((double)model.EffectiveFrames / model.FramesPerCycle * (effectiveWidth * model.Pitch), 3);
                    break;
                }
            }
        }

        /// <summary>
        /// 根据 <seealso cref="ScanParamModel.ScanLength"/> 计算 <seealso cref="ScanParamModel.EffectiveFrames"/>
        /// </summary>
        public static void CalculateEffectiveFrames(this ScanParamModel model)
        {
            var effectiveWidth = Math.Round(model.CollimatorZ * ConstUtil.PerCollimatorSliceWidth, 3);

            switch (model.ScanOption)
            {
                case ScanOption.Surview:
                case ScanOption.DualScout:
                case ScanOption.Axial:
                {
                    uint num;

                    if (CommonCalHelper.IsDoubleMod(model.ScanLength - effectiveWidth, model.TableFeed))
                    {
                        num = (uint)Math.Round((model.ScanLength - effectiveWidth) / model.TableFeed) + 1;
                    }
                    else
                    {
                        var tableFeedNum = Math.Round((model.ScanLength - effectiveWidth) / model.TableFeed, 3);
                        num = (uint)Math.Ceiling(tableFeedNum) + 1;
                    }

                    model.EffectiveFrames = num * model.FramesPerCycle;
                    break;
                }
                case ScanOption.Helical:
                {
                    model.EffectiveFrames = (uint)(model.ScanLength / (effectiveWidth * model.Pitch) * model.FramesPerCycle);
                    break;
                }
            }
        }

        public static GenericResponse CalculateTotalFramesAndDisplayFrames(this ScanParamModel model)
        {
            try
            {
                var tableOutput = model.CalculateTableControlInfo();
                model.NumOfScan = tableOutput.NumOfScan;
                model.TotalFrames = (uint)tableOutput.TotalFrames;
                model.UpdateDisplayFrames();
                return new(true, string.Empty);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.Common, "ScanParamModel calculate error when total frames and display frames.", ex);
                return new(false, ex.Message);
            }
        }

        /// <summary>
        /// 当 <seealso cref="ScanParamModel"/> 中的某些属性发生变化时，计算其它受影响的属性
        /// <para>适用于 <seealso cref="ScanParamModel"/> 的 <seealso cref="INotifyPropertyChanged.PropertyChanged"/> 事件</para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="propertyName">变化的属性的名称</param>
        /// <param name="isUpdateDisplayFrames">是否更新 <see cref="ScanParamModel.DisplayFrames"/></param>
        public static GenericResponse CalculateWhenPropertyChanged(this ScanParamModel model, string? propertyName, bool isUpdateDisplayFrames = false)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || model.IsCalculating)
            {
                return new(true, string.Empty);
            }

            try
            {
                model.IsCalculating = true;

                switch (propertyName)
                {
                    case nameof(ScanParamModel.ScanOption):
                    {
                        model.AmendWhenScanOptionChanged();
                        model.AmendScanLength();
                        model.CalculateEffectiveFrames();
                        break;
                    }
                    case nameof(ScanParamModel.ScanLength):
                    case nameof(ScanParamModel.CollimatorZ):
                    case nameof(ScanParamModel.TableFeed):
                    case nameof(ScanParamModel.FramesPerCycle):
                    {
                        model.AmendScanLength();
                        model.CalculateEffectiveFrames();
                        break;
                    }
                    case nameof(ScanParamModel.EffectiveFrames):
                    {
                        model.AmendEffectiveFrames();
                        model.CalculateScanLength();
                        break;
                    }
                    case nameof(ScanParamModel.Pitch):
                    {
                        // model.CalculateScanLength();
                        model.CalculateEffectiveFrames();
                        break;
                    }
                }

                if (isUpdateDisplayFrames)
                {
                    var res = model.CalculateTotalFramesAndDisplayFrames();

                    if (!res.status)
                    {
                        return res;
                    }
                }

                return new(true, string.Empty);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.Common, "ScanParamModel calculate error when property changed.", ex);
                return new(false, ex.Message);
            }
            finally
            {
                model.IsCalculating = false;
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// 更新 <seealso cref="ScanReconParamModel"/> 的部分参数
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isUpdateReconSeriesParams"></param>
        public static void Update(this ScanReconParamModel model, bool isUpdateReconSeriesParams = true)
        {
            model.Study?.Update();
            model.ScanParameter.Update();

            if (isUpdateReconSeriesParams && model.ReconSeriesParams is { Count: > 0 })
            {
                foreach (var recon in model.ReconSeriesParams)
                {
                    recon.Update(model.ScanParameter);
                }
            }
        }

        /// <summary>
        /// 更新 <seealso cref="StudyModel"/> 的部分参数，例如UID、时间相关等
        /// </summary>
        /// <param name="study"></param>
        public static void Update(this StudyModel study)
        {
            var now = DateTime.Now;
            study.StudyID = UIDHelper.CreateStudyID();
            study.StudyInstanceUID = UIDHelper.CreateStudyInstanceUID();
            study.StudyDate = now;
            study.StudyTime = now;
        }

        /// <summary>
        /// 自动计算并且更新 <seealso cref="ScanParamModel"/> 的部分参数，例如床控相关参数、机架相关参数、UID等
        /// </summary>
        /// <param name="scan"></param>
        public static void Update(this ScanParamModel scan)
        {
            var tableOutput = scan.CalculateTableControlInfo();
            var gantryOutput = scan.GetGantryControlOutput(tableOutput);
            scan.ScanUID = IdGenerator.Next(4);
            scan.ReconVolumeStartPosition = tableOutput.ReconVolumeBeginPos.Micron2Millimeter();
            scan.ReconVolumeEndPosition = tableOutput.ReconVolumeEndPos.Micron2Millimeter();
            scan.TableStartPosition = tableOutput.TableBeginPos.Micron2Millimeter();
            scan.TableEndPosition = tableOutput.TableEndPos.Micron2Millimeter();
            scan.ExposureStartPosition = tableOutput.DataBeginPos.Micron2Millimeter();
            scan.ExposureEndPosition = tableOutput.DataEndPos.Micron2Millimeter();
            scan.TableSpeed = tableOutput.TableSpeed.Micron2Millimeter();
            scan.TableAccelerationTime = tableOutput.TableAccTime.Microsecond2Millisecond();
            scan.TotalFrames = (uint)tableOutput.TotalFrames;
            scan.SmallAngleDeleteLength = tableOutput.SmallAngleDeleteLength.Micron2Millimeter();
            scan.LargeAngleDeleteLength = tableOutput.LargeAngleDeleteLength.Micron2Millimeter();
            scan.PreOffsetFrames = (uint)OffsetCalculator.GetPreOffset(scan.ScanOption, scan.ScanMode);
            scan.PostOffsetFrames = (uint)OffsetCalculator.GetPostOffset(scan.ScanOption, scan.FrameTime.Millimeter2Micron());
            scan.GantryStartPosition = gantryOutput.GantryStartPos.ReduceHundred();
            scan.GantryEndPosition = gantryOutput.GantryEndPos.ReduceHundred();
            scan.GantryDirection = gantryOutput.GantryDirection == GantryControlOutput.GantryDirectionDec ? GantryDirection.CounterClockwise : GantryDirection.Clockwise;
            scan.GantryAccelerationTime = gantryOutput.GantryAccTime.Microsecond2Millisecond();
            scan.GantrySpeed = gantryOutput.GantrySpeed.ReduceHundred();
            scan.TubeNumbers.Clear();

            foreach (var tubeNum in gantryOutput.SelectedTube)
            {
                scan.TubeNumbers.Add(tubeNum);
            }
        }

        /// <summary>
        /// 更新 <seealso cref="ReconSeriesParamModel"/> 的部分参数，例如ID、UID、时间相关、CenterFirstZ、CenterLastZ等
        /// </summary>
        /// <param name="recon"></param>
        /// <param name="scan">关联的 <seealso cref="ScanParamModel"/></param>
        public static void Update(this ReconSeriesParamModel recon, ScanParamModel scan)
        {
            var now = DateTime.Now;
            recon.ReconID = IdGenerator.Next(5);
            recon.SeriesInstanceUID = UIDHelper.CreateSeriesInstanceUID();
            recon.SOPClassUID = UIDHelper.CreateSOPClassUID();
            recon.SOPClassUIDHeader = IdGenerator.Next(7);
            recon.SeriesDate = now;
            recon.SeriesTime = now;
            recon.AcquisitionDate = now;
            recon.AcquisitionTime = now;
            recon.AcquisitionDateTime = now;
            recon.CenterFirstZ = scan.ReconVolumeStartPosition;
            recon.CenterLastZ = scan.ReconVolumeEndPosition;
            var targetReconOutput = recon.GetTargetReconParams(scan);
            recon.IsTargetRecon = targetReconOutput.IsTargetRecon ? YesNoType.Yes : YesNoType.No;
            recon.MinTablePosition = targetReconOutput.TablePositionMin.Micron2Millimeter();
            recon.MaxTablePosition = targetReconOutput.TablePositionMax.Micron2Millimeter();
            recon.ROIFovCenterX = targetReconOutput.roiFovCenterX.Micron2Millimeter();
            recon.ROIFovCenterY = targetReconOutput.roiFovCenterY.Micron2Millimeter();
        }

        #endregion

        #region Method

        private static TargetReconOutput GetTargetReconParams(this ReconSeriesParamModel recon, ScanParamModel scan)
        {
            var targetReconInput = new TargetReconInput(scan.ScanOption,
                                                        recon.PatientPosition,
                                                        SystemConfig.DetectorConfig.Detector.Width.Value,
                                                        Math.Round(scan.CollimatorZ * ConstUtil.PerCollimatorSliceWidth, 3).Millimeter2Micron(),
                                                        scan.ReconVolumeStartPosition.Millimeter2Micron(),
                                                        scan.ReconVolumeEndPosition.Millimeter2Micron(),
                                                        recon.CenterFirstX.Millimeter2Micron(),
                                                        recon.CenterFirstY.Millimeter2Micron(),
                                                        recon.CenterFirstZ.Millimeter2Micron(),
                                                        recon.CenterLastX.Millimeter2Micron(),
                                                        recon.CenterLastY.Millimeter2Micron(),
                                                        recon.CenterLastZ.Millimeter2Micron(),
                                                        recon.FovLengthHor.Millimeter2Micron(),
                                                        recon.FovLengthVert.Millimeter2Micron(),
                                                        scan.SmallAngleDeleteLength.Millimeter2Micron(),
                                                        scan.LargeAngleDeleteLength.Millimeter2Micron());
            return TargetReconCalculator.Instance.GetTargetReconParams(targetReconInput);
        }

        /// <summary>
        /// 转换为<see cref="TableControlInput"/>，同时提供修正后的ScanLength
        /// </summary>
        /// <param name="model"></param>
        /// <param name="scanLength">修正后的ScanLength，单位: mm</param>
        /// <returns></returns>
        private static TableControlInput ToTableControlInput(this ScanParamModel model, out double scanLength)
        {
            var collimatorSliceWidth = Math.Round(model.CollimatorZ * ConstUtil.PerCollimatorSliceWidth, 3);
            var tableInput = new TableControlInput
            {
                ScanOption = model.ScanOption,
                ScanMode = model.ScanMode,
                TableDirection = model.TableDirection,
                ReconVolumeBeginPos = model.ReconVolumeStartPosition.Millimeter2Micron(),
                FrameTime = model.FrameTime.Millisecond2Microsecond(),
                FramesPerCycle = (int)model.FramesPerCycle,
                PreIgnoredFrames = (int)model.AutoDeleteNum,
                CollimatedSliceWidth = collimatorSliceWidth.Millimeter2Micron(),
                TableFeed = model.TableFeed.Millimeter2Micron(),
                TableAcc = model.TableAcceleration.Millimeter2Micron(),
                ExposureTime = model.ExposureTime.Millisecond2Microsecond(),
                Pitch = model.Pitch,
                ExpSourceCount = (int)model.ExposureMode,
                TotalSourceCount = (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount,
                Loops = model.Loops,
                ObjectFov = (uint)((double)model.ObjectFov).Millimeter2Micron(),
                CollimatorZ = (int)model.CollimatorZ,
                PostDeleteLegnth = model.PostDeleteLength,
                PreDeleteRatio = CommonCalcuteHelper.CONST_PreDeleteRatio,
                BodyPart = model.BodyPart,
            };
            var volumeLength = ScanTableCalculator.Instance.TryCorrectReconVolumnLength(tableInput, model.ScanLength.Millimeter2Micron());
            var factor = CommonCalHelper.GetTableDirectionFactor(model.TableDirection);
            tableInput.ReconVolumeEndPos = volumeLength * factor + tableInput.ReconVolumeBeginPos;
            scanLength = volumeLength.Micron2Millimeter();
            return tableInput;
        }

        private static TableControlOutput CalculateTableControlInfo(this ScanParamModel model)
        {
            var tableInput = model.ToTableControlInput(out _);
            return ScanTableCalculator.Instance.CalculateTableControlInfo(tableInput);
        }

        private static GantryControlOutput GetGantryControlOutput(this ScanParamModel model, TableControlOutput tableOutput)
        {
            var tubes = AcqReconProxy.Instance.CurrentDeviceSystem.XRaySources.OrderBy(i => i.Number).ToList();
            var gantryInput = new GantryControlInput
            {
                ScanOption = model.ScanOption,
                ScanMode = model.ScanMode,
                TubePositions = model.TubePositions.ToArray(),
                CurrentGantryPos = AcqReconProxy.Instance.CurrentDeviceSystem.Gantry.Position,
                OilTem = tubes.Select(i => (double)i.XRaySourceOilTemp).ToArray(),
                HeatCaps = tubes.Select(i => (double)i.XRaySourceHeatCap).ToArray(),
                PreIgnoredN = (int)model.AutoDeleteNum,
                FrameTime = model.FrameTime.Millisecond2Microsecond(),
                FramesPerCycle = (int)model.FramesPerCycle,
                ExpSourceCount = (int)model.ExposureMode,
                TotalSourceCount = (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount,
                NumOfScan = tableOutput.NumOfScan,
                TableFeed = model.TableFeed.Millimeter2Micron(),
                TableAcc = model.TableAcceleration.Millimeter2Micron(),
                TableSpeed = tableOutput.TableSpeed,
                DataBeginPos = tableOutput.DataBeginPos,
                DataEndPos = tableOutput.DataEndPos,
                GantryAcc = model.GantryAcceleration.ExpandHundred(),
                Loops = model.Loops,
                LoopTime = model.LoopTime,
            };
            return ScanGantryCalculator.Instance.GetGantryControlOutput(gantryInput);
        }

        #endregion
    }
}
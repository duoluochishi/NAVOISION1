using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.TubeWarmUp.DAL.Dtos;
using NV.CT.Service.TubeWarmUp.Models;
using NV.MPS.Configuration;
using System;

namespace NV.CT.Service.TubeWarmUp.Services.Adapter
{
    public class WarmUpTaskAdapter
    {
        public WarmUpTask Task { get; private set; }

        public WarmUpTaskAdapter(WarmUpTask task)
        {
            this.Task = task;
            ScanUID = Generate16UID();
        }

        public string ScanUID;

        private static string Generate16UID()
        {
            string timeStamp = DateTime.UtcNow.ToString("yyyymmddHHmmssff");
            //防止连续生成相同的uid
            System.Threading.Thread.Sleep(15);
            return timeStamp;
        }

        public WarmUpParam GetWarmUpParam(ScanParamDto scanParam)
        {
            var para = new WarmUpParam();
            para.ScanUID = this.ScanUID;
            para.kV[0] = (uint)this.Task.KV;
            para.mA[0] = (uint)this.Task.MA;

            para.ExposureTime = scanParam.ExposureTime;
            para.FrameTime = scanParam.FrameTime;
            para.FramesPerCycle = scanParam.FramesPerCycle;
            para.ScanOption = scanParam.ScanOption;
            para.ScanMode = scanParam.ScanMode;
            para.ExposureMode = scanParam.ExposureMode;
            para.ReconVolumeStartPosition = scanParam.ScanPositionStart;//current table pos
            para.ReconVolumeEndPosition = scanParam.ScanPositionEnd;
            para.ExposureStartPosition = DeviceSystem.Instance.Table.HorizontalPosition;
            para.ExposureEndPosition = DeviceSystem.Instance.Table.HorizontalPosition;

            #region table

            para.TableStartPosition = DeviceSystem.Instance.Table.HorizontalPosition;
            para.TableEndPosition = DeviceSystem.Instance.Table.HorizontalPosition;
            para.TableDirection = scanParam.TableDirection;
            para.TableSpeed = scanParam.TableSpeed;
            para.TableFeed = scanParam.TableFeed;

            #endregion table

            para.ExposureDelayTime = scanParam.DelayTime;
            para.AutoScan = scanParam.AutoScan;
            para.Pitch = scanParam.Pitch;
            para.Gain = scanParam.Gain;
            para.TableHeight = scanParam.TableHeight;
            para.PreOffsetFrames = scanParam.PreOffsetFrames;
            para.PostOffsetFrames = scanParam.PostOffsetFrames;
            para.CollimatorOpenMode = (scanParam.CollimitorX == 1) ? CollimatorOpenMode.NearSmallAngle : CollimatorOpenMode.NearCenter;
            para.CollimatorZ = scanParam.CollimitorZ;

            #region gantry

            para.GantryAcceleration = 0;
            para.GantryAccelerationTime = 0;
            para.GantrySpeed = default;
            para.GantryDirection = default;
            para.GantryStartPosition = GetGantryPosition();
            para.GantryEndPosition = GetGantryPosition();

            #endregion gantry

            para.TotalFrames = scanParam.TotalFrames;
            para.BowtieEnable = false;

            #region voice

            para.PreVoiceID = 0;
            para.PostVoiceID = 0;
            para.PreVoiceDelayTime = 0;

            #endregion voice

            para.WarmUp = true;
            return para;
            uint GetGantryPosition()
            {
                return (int)DeviceSystem.Instance.Gantry.Position > SystemConfig.GantryConfig.Gantry.Angle.Max ? (uint)SystemConfig.GantryConfig.Gantry.Angle.Max : ((int)DeviceSystem.Instance.Gantry.Position < SystemConfig.GantryConfig.Gantry.Angle.Min ? (uint)SystemConfig.GantryConfig.Gantry.Angle.Min : DeviceSystem.Instance.Gantry.Position);
            }
        }

        public DataAcquisitionParams GetDataAcquisitionParams(ScanParamDto scanParam)
        {
            var para = new DataAcquisitionParams();
            para.ScanUID = this.ScanUID;
            para.ExposureParams.kVs[0] = (uint)this.Task.KV;
            para.ExposureParams.mAs[0] = (uint)this.Task.MA / 1000;

            para.ExposureParams.ExposureTime = scanParam.ExposureTime / 1000;
            para.ExposureParams.FrameTime = scanParam.FrameTime / 1000;
            para.ExposureParams.FramesPerCycle = scanParam.FramesPerCycle;
            para.ExposureParams.ScanOption = scanParam.ScanOption;
            para.ExposureParams.ScanMode = scanParam.ScanMode;
            para.ExposureParams.ExposureMode = scanParam.ExposureMode;

            para.ExposureParams.ExposureDelayTime = scanParam.DelayTime / 1000;
            para.ExposureParams.AutoScan = scanParam.AutoScan;
            //para.ExposureParams.pit = scanParam.Pitch;
            para.DetectorParams.CurrentGain = scanParam.Gain;
            //para.PreOffsetFrames = scanParam.PreOffsetFrames;
            //para.PostOffsetFrames = scanParam.PostOffsetFrames;
            //(scanParam.CollimitorX == 1) ? CollimatorOpenMode.NearSmallAngle : CollimatorOpenMode.NearCenter;
            para.ExposureParams.CollimatorOpenMode = scanParam.CollimitorX;
            para.ExposureParams.CollimatorOpenWidth = scanParam.CollimitorZ;
            para.ExposureParams.CollimatorSwitch = true;
            para.ExposureParams.TotalFrames = scanParam.FramesPerCycle;
            //para.WarmUp = true;

            return para;
        }
    }
}
using NV.CT.CTS;
using NV.CT.DicomUtility.Graphic;
using NV.CT.FacadeProxy.Common.Enums;
using TubePos = NV.CT.FacadeProxy.Common.Enums.TubePosition;


namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension.ModificationRule
{
    public class TopoTubePositionTypeLinkedRule:ILinkedModificationRule
    {

        public bool CanAccept(BaseModel model, string parameterName)
        {
            var scanModel = model as ScanModel;
            if (scanModel is not null && scanModel.ScanOption is ScanOption.Surview or ScanOption.DualScout)
            {
                if (parameterName == ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE)
                {
                    return true;
                }
            }
            return false;
        }

        public Dictionary<BaseModel, List<ParameterModel>> GetLinkedModificationParam(BaseModel model, ParameterModel parameter)
        {
            //修改TubePositionTypeLinkedRule会导致以下情况发生：
            //修改TubePosition
            //修改ScanOption
            //若为正、侧位，确保只有一个RTDRecon，且RTD重建符合条件。
            //若为双侧位，则确保有两个rtdRecon，且rtd重建符合条件。

            var result = new Dictionary<BaseModel, List<ParameterModel>>();
            var scanModel = model as ScanModel;
            var tubePositionType = ParameterConverter.Convert<TubePositionType>(parameter.Value);
            result.Add(scanModel, GetChangedScanParameters(scanModel,tubePositionType));
            
            var recons = scanModel.Children;
            var patientPosition = scanModel.Parent.Parent.PatientPosition;
            var protocolHostService = Global.ServiceProvider?.GetService<IProtocolHostService>();
            //handle recon
            switch (tubePositionType)
            {
                case TubePositionType.Normal:
                    if (recons.Count == 2)
                    {
                        protocolHostService.DeleteRecon(scanModel.Descriptor.Id, recons[1].Descriptor.Id);
                    }
                    result.Add(recons[0], GetChangedTopoRTDReconDirection(patientPosition,TubePos.Angle0));
                    break;
                case TubePositionType.Lateral:
                    if (recons.Count == 2)
                    {
                        protocolHostService.DeleteRecon(scanModel.Descriptor.Id, recons[1].Descriptor.Id);
                    }
                    result.Add(recons[0], GetChangedTopoRTDReconDirection(patientPosition,TubePos.Angle270));
                    break;
                case TubePositionType.DualScout:
                    if(recons.Count == 1)
                    {
                        protocolHostService.CopyRecon(scanModel.Descriptor.Id, recons[0].Descriptor.Id, true);
                    }
                    result.Add(recons[0],GetChangedTopoRTDReconDirection(patientPosition, TubePos.Angle0));
                    result.Add(recons[1], GetChangedTopoRTDReconDirection(patientPosition, TubePos.Angle270));
                    break;
            }

            return result;
        }

        private List<ParameterModel> GetChangedScanParameters(ScanModel scanModel, TubePositionType tubePositionType)
        {
            var result = new List<ParameterModel>();

            var scanOption = scanModel.ScanOption;
            var tubePositions = scanModel.TubePositions;
            var kVs = scanModel.Kilovolt;
            var mAs = scanModel.Milliampere;

            //handle scan tube position and scan option
            switch (tubePositionType)
            {
                case TubePositionType.Normal:
                    scanOption = ScanOption.Surview;
                    tubePositions[0] = TubePosition.Angle0;
                    tubePositions[1] = TubePosition.Angle0;
                    kVs[1] = 0;
                    mAs[1] = 0;
                    break;
                case TubePositionType.Lateral:
                    scanOption = ScanOption.Surview;
                    tubePositions[0] = TubePosition.Angle270;
                    tubePositions[1] = TubePosition.Angle0;
                    kVs[1] = 0;
                    mAs[1] = 0;
                    break;
                case TubePositionType.DualScout:
                    scanOption = ScanOption.DualScout;
                    tubePositions[0] = TubePosition.Angle0;
                    tubePositions[1] = TubePosition.Angle270;
                    kVs[1] = kVs[0];
                    mAs[1] = mAs[0];
                    break;
            }
            result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_OPTION, Value = scanOption.ToString() }) ;
            result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_TUBE_POSITIONS, Value = $"{JsonConvert.SerializeObject(tubePositions)}" });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_KILOVOLT, Value = $"{JsonConvert.SerializeObject(kVs)}" });
            result.Add(new ParameterModel { Name = ProtocolParameterNames.SCAN_MILLIAMPERE, Value = $"{JsonConvert.SerializeObject(mAs)}" });

            return result;
        }

        private List<ParameterModel> GetChangedTopoRTDReconDirection(PatientPosition patientPosition,TubePos tubeposition)
        {
            var dir = ScanReconCoordinateHelper.GetDefaultTopoReconOrientation(patientPosition, tubeposition);
            return new List<ParameterModel>
            {
                new ParameterModel { Name = ProtocolParameterNames.RECON_IS_RTD, Value = true.ToString() },
                new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X, Value = dir[0].ToString() },
                new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y, Value = dir[1].ToString() },
                new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z, Value = dir[2].ToString() },
                new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X, Value = dir[3].ToString() },
                new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y, Value = dir[4].ToString() },
                new ParameterModel { Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z, Value = dir[5].ToString() },
            };
        }


    }
}

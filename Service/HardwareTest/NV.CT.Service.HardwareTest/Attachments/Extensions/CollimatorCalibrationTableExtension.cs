using NV.CT.FacadeProxy.Models.MotionControl.Collimator;
using NV.CT.Service.HardwareTest.Attachments.LibraryCallers;
using System.Collections.Generic;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class CollimatorCalibrationTableExtension
    {
        public static void Update(
            this CollimatorCalibrationTable calibrationTable, CollimatorMotorType motorType, IEnumerable<CollimatorData> collimatorDatas) 
        {
            switch (motorType)
            {
                case CollimatorMotorType.FrontBlade:
                    {
                        calibrationTable.FrontBladeMoveSteps = GetCollimatorMoveSteps(motorType).ToArray();
                    }
                    break;
                case CollimatorMotorType.RearBlade:
                    {
                        calibrationTable.RearBladeMoveSteps = GetCollimatorMoveSteps(motorType).ToArray();
                    }
                    break;
                case CollimatorMotorType.Bowtie:
                    {
                        calibrationTable.BowtieMoveSteps = GetCollimatorMoveSteps(motorType).ToArray();
                    }
                    break;
            }

            IEnumerable<uint> GetCollimatorMoveSteps(CollimatorMotorType motorType)
            {
                foreach (var data in collimatorDatas)
                {
                    switch (motorType)
                    {
                        case CollimatorMotorType.FrontBlade: yield return (uint)data.frontBlade; break;
                        case CollimatorMotorType.RearBlade: yield return (uint)data.rearBlade; break;
                        case CollimatorMotorType.Bowtie: yield return (uint)data.bowtie; break;
                    }
                }
            }
        }
    }
}

using NV.CT.FacadeProxy.Common.Fields;
using NV.CT.FacadeProxy.Models.MotionControl.Collimator;
using NV.CT.FacadeProxy.Models.MotionControl.Gantry;
using NV.CT.FacadeProxy.Models.MotionControl.Table;
using NV.CT.Service.HardwareTest.Categories;
using System;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class BaseCategoryExtension
    {
        public static TableParams ToProxyTableParams(this TableBaseCategory tableBaseCategory) 
        {
            TableParams tableParams = new TableParams();

            tableParams.Direction = tableBaseCategory.Direction;
            tableParams.HorizontalPosition = Convert.ToUInt32(Math.Abs(tableBaseCategory.HorizontalPosition) * TableConstants.Thousand);
            tableParams.HorizontalVelocity = Convert.ToUInt32(Math.Abs(tableBaseCategory.HorizontalVelocity) * TableConstants.Thousand);
            tableParams.VerticalPosition = Convert.ToUInt32(Math.Abs(tableBaseCategory.VerticalPosition) * TableConstants.Thousand);
            tableParams.VerticalVelocity = Convert.ToUInt32(Math.Abs(tableBaseCategory.VerticalVelocity) * TableConstants.Thousand);
            tableParams.AxisXPosition = Convert.ToUInt32(Math.Abs(tableBaseCategory.AxisXPosition) * TableConstants.Thousand);
            tableParams.AxisXVelocity = Convert.ToUInt32(Math.Abs(tableBaseCategory.AxisXVelocity) * TableConstants.Thousand);

            return tableParams;
        }

        public static GantryParams ToProxyGantryParams(this GantryBaseCategory gantryBaseCategory) 
        {
            GantryParams gantryParams = new GantryParams();

            gantryParams.MoveDirection = gantryBaseCategory.CurrentRotateDirection;
            gantryParams.MoveMode = gantryBaseCategory.CurrentMoveMode;
            gantryParams.Velocity = gantryBaseCategory.MaximumVelocity;
            gantryParams.TargetPosition = gantryBaseCategory.TargetAngle;

            return gantryParams;
        }

        public static CollimatorParams ToProxyCollimatorParams(this CollimatorBaseCategory collimatorBaseCategory) 
        {
            CollimatorParams collimatorParams = new CollimatorParams();

            collimatorParams.SourceIndex = collimatorBaseCategory.SourceIndex;
            collimatorParams.MessageType = collimatorBaseCategory.MessageType;
            collimatorParams.MotorType = collimatorBaseCategory.MotorType;
            collimatorParams.TargetPosition = collimatorBaseCategory.MoveStep;

            return collimatorParams;
        }

    }
}

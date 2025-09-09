using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Models.MotionControl.Gantry;

namespace NV.CT.Service.HardwareTest.Categories
{
    public partial class GantryBaseCategory : ObservableObject
    {
        [ObservableProperty]
        private uint maximumVelocity;
        [ObservableProperty]
        private uint targetAngle;
        [ObservableProperty]
        private GantryDirection currentRotateDirection = GantryDirection.Clockwise;
        [ObservableProperty]
        private GantryMoveMode currentMoveMode = GantryMoveMode.PositionMode;
    }
}

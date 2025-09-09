using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.FacadeProxy.Common.Enums.Collimator;
using System;

namespace NV.CT.Service.HardwareTest.Models.Components.Collimator
{
    public partial class CollimatorOpenType : ObservableObject
    {
        public CollimatorOpenType(
            CollimatorOpenMode openMode, CollimatorOpenWidth openWidth)
        {
            this.OpenMode = openMode;
            this.OpenWidth = openWidth;
            this.FrontBladeTargetPosition = 2000;
            this.RearBladeTargetPosition = 2266;
        }

        [ObservableProperty]
        private uint frontBladeTargetPosition;
        [ObservableProperty]
        private uint rearBladeTargetPosition;
        [ObservableProperty]
        private CollimatorOpenMode openMode;
        [ObservableProperty]
        private CollimatorOpenWidth openWidth;

        public uint FrontBladeWidth => OpenMode == CollimatorOpenMode.NearSmallAngle ? 288 - (uint)OpenWidth : (288 / 2 - (uint)OpenWidth / 2);
        public uint RearBladeWidth => OpenMode == CollimatorOpenMode.NearSmallAngle ? 288 : (288 / 2 + (uint)OpenWidth / 2);

        public static readonly float Unit = 0.165f;
        public string Name => $"{(uint)OpenWidth} × {CollimatorOpenType.Unit} - {Enum.GetName(OpenMode)}";
    }

}

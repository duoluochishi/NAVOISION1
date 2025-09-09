using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;

namespace NV.CT.Service.HardwareTest.Models.Components.Table
{
    public partial class TableSource : AbstractSource
    {
        public TableSource() : base()
        {
            this.Name = "Table";
        }

        [ObservableProperty]
        private float horizontalPosition;
        [ObservableProperty]
        private float horizontalVelocity;
        [ObservableProperty]
        private float verticalPosition;
        [ObservableProperty]
        private float verticalVelocity;
        [ObservableProperty]
        private float axisXPosition;
        [ObservableProperty]
        private float axisXVelocity;
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;

namespace NV.CT.Service.HardwareTest.Models.Components.Gantry
{
    public partial class GantrySource : AbstractSource
    {
        public GantrySource() : base()
        {
            this.Name = "Gantry";
        }

        [ObservableProperty]
        private string name = string.Empty;
        [ObservableProperty]
        private double position;
        [ObservableProperty]
        private double velocity;
    }
}

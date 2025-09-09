using CommunityToolkit.Mvvm.ComponentModel;
using NV.CT.Service.HardwareTest.Attachments.LibraryCallers;
using NV.CT.Service.HardwareTest.Models.Foundations.Abstractions;

namespace NV.CT.Service.HardwareTest.Models.Components.Collimator
{
    public partial class CollimatorSource : AbstractSource
    {
        [ObservableProperty]
        private uint bowtieMoveStep;
        [ObservableProperty]
        private uint frontBladeMoveStep;
        [ObservableProperty]
        private uint rearBladeMoveStep;

        public CollimatorData ToCollimatorData => new CollimatorData 
        {
            bowtie = (int)BowtieMoveStep,
            frontBlade = (int)FrontBladeMoveStep,
            rearBlade = (int)RearBladeMoveStep
        };
    }
}

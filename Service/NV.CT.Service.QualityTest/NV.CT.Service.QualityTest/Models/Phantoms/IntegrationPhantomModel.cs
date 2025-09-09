namespace NV.CT.Service.QualityTest.Models.Phantoms
{
    public sealed class IntegrationPhantomModel
    {
        public int PhantomLocation { get; set; }
        public int Water30Distance { get; init; }
        public int Water20Distance { get; init; }
        public int PhysicalDistance { get; init; }
        public int AirDistance { get; init; }
    }
}
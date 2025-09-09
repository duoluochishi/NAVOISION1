using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models.ItemEntryValidate;
using NV.CT.Service.QualityTest.Models.ItemEntryValue;

namespace NV.CT.Service.QualityTest.Models.ItemEntryParam
{
    public sealed class SingleValueParamModel : ItemEntryParamBaseModel
    {
        public WaterPhantomLayerType? WaterLayer { get; init; }
        public ImagePercentModel ImagePercent { get; init; } = null!;
        public ExtremumValidate ValidateParam { get; init; } = null!;
        public SingleValueValue Value { get; } = new();

        public override bool Validate()
        {
            var values = new[] { Value.FirstValue, Value.MediumValue, Value.LastValue };
            return ValidateParam.Validate(values);
        }

        public override void Clear()
        {
            Value.Clear();
        }
    }
}
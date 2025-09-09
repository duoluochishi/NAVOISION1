using NV.CT.Service.QualityTest.Enums;
using NV.CT.Service.QualityTest.Models.ItemEntryValidate;
using NV.CT.Service.QualityTest.Models.ItemEntryValue;

namespace NV.CT.Service.QualityTest.Models.ItemEntryParam
{
    public sealed class HomogeneityParamModel : ItemEntryParamBaseModel
    {
        public WaterPhantomLayerType WaterLayer { get; init; }
        public ImagePercentModel ImagePercent { get; init; } = null!;
        public ExtremumValidate ValidateParam { get; init; } = null!;
        public HomogeneityValue Value { get; } = new();

        public override bool Validate()
        {
            var values = new[]
            {
                Value.FirstOClock3Value,
                Value.FirstOClock6Value,
                Value.FirstOClock9Value,
                Value.FirstOClock12Value,
                Value.MediumOClock3Value,
                Value.MediumOClock6Value,
                Value.MediumOClock9Value,
                Value.MediumOClock12Value,
                Value.LastOClock3Value,
                Value.LastOClock6Value,
                Value.LastOClock9Value,
                Value.LastOClock12Value
            };

            return ValidateParam.Validate(values);
        }

        public override void Clear()
        {
            Value.Clear();
        }
    }
}
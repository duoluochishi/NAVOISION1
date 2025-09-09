using NV.CT.Service.QualityTest.Models.ItemEntryValidate;
using NV.CT.Service.QualityTest.Models.ItemEntryValue;

namespace NV.CT.Service.QualityTest.Models.ItemEntryParam
{
    public sealed class MTFParamModel : ItemEntryParamBaseModel
    {
        public ImagePercentModel ImagePercent { get; init; } = null!;
        public MTFValidate ValidateParam { get; init; } = null!;
        public MTFValue Value { get; } = new();

        public override bool Validate()
        {
            return ValidateParam.Validate(Value);
        }

        public override void Clear()
        {
            Value.Clear();
        }
    }
}
using NV.CT.Service.QualityTest.Models.ItemEntryValidate;
using NV.CT.Service.QualityTest.Models.ItemEntryValue;

namespace NV.CT.Service.QualityTest.Models.ItemEntryParam
{
    public sealed class IntegrationPhantomParamModel : ItemEntryParamBaseModel
    {
        public IntegrationPhantomValidate ValidateParam { get; init; } = null!;
        public IntegrationPhantomValue Value { get; } = new();

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
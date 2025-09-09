namespace NV.CT.Service.QualityTest.Models.ItemEntryValidate
{
    public abstract class ValidateBase
    {
        protected double CoerceValue(double value)
        {
            return value switch
            {
                >= 99999 => double.MaxValue,
                <= -99999 => double.MinValue,
                _ => value,
            };
        }
    }
}
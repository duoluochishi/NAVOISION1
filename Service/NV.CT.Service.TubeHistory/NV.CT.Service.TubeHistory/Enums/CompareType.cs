using System.ComponentModel;

namespace NV.CT.Service.TubeHistory.Enums
{
    public enum CompareType
    {
        [Description(">")]
        GreaterThan,

        [Description(">=")]
        GreaterThanOrEqual,

        [Description("=")]
        Equal,

        [Description("<=")]
        LessThanOrEqual,

        [Description("<")]
        LessThan,
    }
}
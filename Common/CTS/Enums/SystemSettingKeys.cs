using System.ComponentModel;

namespace NV.CT.CTS.Enums;

public enum SystemSettingKeys
{
    [Description("KV")]
    Kv,
    [Description("Topogram length")]
    TopogramLength,
    [Description("Slice thickness")]
    SliceThickness,
    [Description("Recon matrix")]
    ReconMatrix,
    [Description("Pitch")]
    Pitch,
}

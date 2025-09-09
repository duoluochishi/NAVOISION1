namespace NV.CT.Protocol.Models;

public class RgtScanModel
{
    public string Protocol { get; set; } = string.Empty;

    public string ScanType { get; set; } = string.Empty;

    public uint Kv { get; set; }
    public decimal Ma { get; set; }

    public string PatientPosition { get; set; } = string.Empty;

    public uint ScanLength { get; set; }

    public float CTDlvol { get; set; }
    public float DLP { get; set; }

    public decimal ScanTime { get; set; }

    public decimal DelayTime { get; set; }

    public float ExposureTime { get; set; }
    public float FrameTime { get; set; }
    public string BodyPart { get; set; } = string.Empty;
}
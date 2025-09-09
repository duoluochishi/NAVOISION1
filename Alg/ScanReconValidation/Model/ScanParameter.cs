namespace NV.CT.Alg.ScanReconValidation.Model;

public class ScanParameter
{
	public int TableStartPos { get; set; }
	public int TableEndPos { get; set; }
	public string ScanOption { get; set; } = string.Empty;
	public string ScanMode { get; set; } = string.Empty;
	public List<int> KV { get; set; } = new();
	public List<int> MA { get; set; } = new();
	public int TableFeed { get; set; }


	//public int ExposureTime { get; set; }
	//public int FrameTime { get; set; }
	//public int ExposureDelayTime { get; set; }
	//public bool AutoScan { get; set; }
	//public int FramesPerCycle { get; set; }
	//public string ExposureMode { get; set; } = string.Empty;
	//public string TriggerMode { get; set; } = string.Empty;
	//public int TriggerBegin { get; set; }
	//public int TriggerEnd { get; set; }
	//public int CollimatorZ { get; set; }
	//public int CollimatorX { get; set; }
	//public bool BowtieEnable { get; set; }
	//public int GantryStartPos { get; set; }
	//public int GantryEndPos { get; set; }
	//public int GantryDirection { get; set; }
	//public int GantryAcceleration { get; set; }
	//public int GantryAccTime { get; set; }
	//public int GantrySpeed { get; set; }
	//public int ExposureStartPos { get; set; }
	//public int ExposureEndPos { get; set; }
	//public string TableDirection { get; set; } = string.Empty;
	//public int TableAcceleration { get; set; }
	//public int TableAccTime { get; set; }
	//public int TableHorizontal { get; set; }
	//public int TableSpeed { get; set; }
	//public int XRayFocus { get; set; }
	//public int PreVoiceId { get; set; }
	//public int PostVoiceId { get; set; }
	//public int PreVoicePlayTime { get; set; }
	//public int PreVoiceDelayTime { get; set; }
	//public int WarmUp { get; set; }
	//public List<int> TubeNumber { get; set; } = new();
	//public int PreOffsetFrame { get; set; }
	//public int PostOffsetFrame { get; set; }
	//public int ScanLoops { get; set; }
	//public int ScanLooptime { get; set; }
	//public int AutoDeleteNum { get; set; }
	//public int TotalFrames { get; set; }
	//public string ScanUID { get; set; } = string.Empty;
	//public string Gain { get; set; } = string.Empty;
	//public string BodyPart { get; set; } = string.Empty;
	//public string Binning { get; set; } = string.Empty;
	//public List<string> TubePosition { get; set; } = new();
	//public int Pitch { get; set; }
	//public int ScanNumber { get; set; }
	//public string RawDataType { get; set; } = string.Empty;
	//public string ContrastBolusAgent { get; set; } = string.Empty;
	//public object ContrastBolusVolume { get; set; }
	//public object ContrastFlowRate { get; set; }
	//public object ContrastFlowDuration { get; set; }
	//public object ContrastBolusIngredientConcentration { get; set; }
	//public float ScanFov { get; set; }
	//public string RawDataDirectory { get; set; } = string.Empty;
	//public int CollimatorSliceWidth { get; set; }
	//public int ReconVolumeStartPos { get; set; }
	//public int ReconVolumeEndPosition { get; set; }
	//public int MultiParamInfo { get; set; }
	//public int Reserved005 { get; set; }
	//public int Reserved006 { get; set; }
	//public int Reserved007 { get; set; }
	//public int TableHeight { get; set; }
	//public int RDelay { get; set; }
	//public int TDelay { get; set; }
	//public int SpotDelay { get; set; }
	//public int CollimatorSpotDelay { get; set; }
	//public string FunctionMode { get; set; } = string.Empty;
}

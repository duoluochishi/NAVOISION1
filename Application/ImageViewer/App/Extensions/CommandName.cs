namespace NV.CT.ImageViewer.Extensions;

public class CommandName
{
	#region ROI
	public const string Create_Length_ROI = "Create_Length_ROI";
	public const string Create_Angle_ROI = "Create_Angle_ROI";
	public const string Create_Circle_ROI = "Create_Circle_ROI";
	public const string Create_Arrow_ROI = "Create_Arrow_ROI";

	public const string Create_Rect_ROI = "Create_Rect_ROI";
	public const string Create_Freehand_ROI = "Create_Freehand_ROI";

	public const string Create_Text_ROI = "Create_Text_ROI";
	public const string Create_Grid_ROI = "Create_Grid_ROI";
	public const string Create_Sign_ROI = "Create_Sign_ROI";

	public const string Remove_All_ROI2D = "Remove_All_ROI2D";
	public const string Remove_All_ROI3D = "Remove_All_ROI3D";
	#endregion

	#region view command

	public const string Layout = "Layout";
	public const string Move = "Move";
	public const string Zoom = "Zoom";
	public const string Wwwl = "Wwwl";
    public const string RotateMode = "RotateMode";
    public const string Rotate = "Rotate";
	public const string InitialState = "InitialState";
	public const string Film = "Film";
    public const string Scroll = "Scroll";
    public const string FlipHorizontal = "FlipHorizontal";
	public const string FlipVertical = "FlipVertical";
	public const string HiddenText = "HiddenText";
	public const string Screenshot = "Screenshot";
	public const string ReverseSequence = "ReverseSequence";
	public const string Invert = "Invert";
	public const string Dicomtag = "Dicomtag";
	public const string Kernel = "Kernel";
    public const string Synchronization = "Synchronization";
    public const string ArchiveCommand = "ArchiveCommand";
    public const string PrintCommand = "PrintCommand";
    public const string BrowseCommand = "BrowseCommand";
    public const string TableBrowseCommand = "TableBrowseCommand";
	public const string BrowseRawDataCommand = "BrowseRawDataCommand";
	public const string TableBrowseRawDataCommand = "TableBrowseRawDataCommand";

    public const string COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_DOWN = "PreviewMouseLeftButtonDown";
    public const string COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_UP = "PreviewMouseLeftButtonUp";
    public const string COMMAND_VALUE_CHANGED = "ValueChanged";
    public const string COMMAND_MOUSE_LEAVE = "MouseLeave";

    public const string ShowPostProcessSettingCommand = "ShowPostProcessSettingCommand";
    public const string ApplyPostProcessCommand = "ApplyPostProcessCommand";
    public const string COMMAND_ADD = "AddCommand";
    public const string COMMAND_REMOVE = "RemoveCommand";
    public const string COMMAND_CLOSE = "CloseCommand";
    public const string COMMAND_SELECT = "SelectCommand";
    public const string CancelPostProcessCommand = "CancelPostProcessCommand";
    #endregion

    #region 3D only
    public const string Rotate2D = "Rotate2D";
	public const string Rotate3D = "Rotate3D";
	public const string HideAxis = "HideAxis";

	public const string MPR = "MPR";
	public const string MIP = "MIP";
	public const string MinIP = "MinIP";
	public const string AVG = "AVG";
	public const string VR = "VR";
	public const string SSD = "SSD";
	public const string CPR = "CPR";
	public const string VRTReset = "VRTReset";

	public const string ClipBox = "ClipBox";
    public const string CutMode = "CutMode";
    public const string SelectedCut = "SelectedCut";
	public const string UnselectedCut = "UnselectedCut";
	public const string UndoVolumeCut = "UndoVolumeCut";
	public const string RedoVolumeCut = "RedoVolumeCut";
	public const string AdvancePreset = "AdvancePreset";
    public const string RemoveTable = "RemoveTable";

    public const string MPRRecon = "MPRRecon";
    public const string MultiAngleRecon = "MultiAngleRecon";
    public const string FreeSliceRecon = "FreeSliceRecon";
    public const string RGBDataRecon = "RGBDataRecon";
    public const string BatchRecon = "BatchRecon";
    public const string BatchSave = "BatchSave";

    #endregion


    #region Film

    public const string ShowFilm = "ShowFilm";
	public const string AdjustFrameRate = "AdjustFrameRate";
	public const string Play = "Play";
	public const string Pause = "Pause";
	public const string Resume = "Resume";
	public const string PrevFrame = "PrevFrame";
	public const string NextFrame = "NextFrame";
    public const string BeginFrame = "BeginFrame";
    public const string EndFrame = "EndFrame";
	#endregion

}
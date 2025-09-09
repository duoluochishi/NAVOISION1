namespace NV.CT.RGT.Model;

public class CommandParameters
{
    #region  ImageOperater
    public const string IMAGE_OPERATE_SELECT = "Select";
    public const string IMAGE_OPERATE_HU = "HU";
    public const string IMAGE_OPERATE_ZOOM = "Zoom";
    public const string IMAGE_OPERATE_MOVE = "Move";
    public const string IMAGE_OPERATE_ROTATE = "Rotate";
    public const string IMAGE_OPERATE_WL = "WL";
    public const string IMAGE_OPERATE_ROI = "ROI";
    public const string IMAGE_OPERATE_LENGTH = "Length";
    public const string IMAGE_OPERATE_ANGLE = "Angle";
    public const string IMAGE_OPERATE_ARROW = "Arrow";
    public const string IMAGE_OPERATE_REVERSE = "Reverse";
    public const string IMAGE_OPERATE_REWORK = "Rework";
    public const string IMAGE_OPERATE_CROP = "Crop";
    #endregion

    #region IMAGE
    public const string IMAGE_OPERATE_SETAXES = "SetAxes";
    public const string IMAGE_OPERATE_MPR = "MPR";
    public const string IMAGE_OPERATE_MPRTHIN = "MIPThin";
    public const string IMAGE_OPERATE_SETFOVSEGMENT = "SetFoVSegment";
    public const string IMAGE_OPERATE_FOVSETTINGS = "FovSettings";
    public const string IMAGE_OPERATE_RESCANDETAILS = "ReScanDetails";
    #endregion

    #region Human_Body_Name
    public const string HUMAN_BODY_HEAD = "Head";
    public const string HUMAN_BODY_NECK = "Neck";
    public const string HUMAN_BODY_SHOULDER = "Shoulder";
    public const string HUMAN_BODY_BREAST = "Breast";
    public const string HUMAN_BODY_ABDOMEN = "Abdomen";
    public const string HUMAN_BODY_HAND = "Hand";
    public const string HUMAN_BODY_PELVIS = "Pelvis";
    public const string HUMAN_BODY_LEG = "Leg";
    public const string HUMAN_BODY_SPINE = "Spine";
    public const string HUMAN_BODY_VEINHEAD = "VeinHead";
    public const string HUMAN_BODY_BHEAD = "BHead";
    public const string HUMAN_BODY_VEINNECK = "VeinNeck";
    public const string HUMAN_BODY_BNECK = "BNeck";
    public const string HUMAN_BODY_VEINBREAST = "VeinBreast";
    public const string HUMAN_BODY_BBREAST = "BBreast";
    public const string HUMAN_BODY_VEINABDOMEN = "VeinAbdomen";
    public const string HUMAN_BODY_BABDOMEN = "BAbdomen";
    public const string HUMAN_BODY_VEINLEG = "VeinLeg";
    public const string HUMAN_BODY_BLEG = "BLeg";
    public const string HUMAN_BODY_VEINHAND = "VeinHand";
    public const string HUMAN_BODY_BHAND = "BHand";

    public const string HUMAN_BODY_CHILDHEAD = "ChildHead";
    public const string HUMAN_BODY_CHILDNECK = "ChildNeck";
    public const string HUMAN_BODY_CHILDSHOULDER = "ChildShoulder";
    public const string HUMAN_BODY_CHILDBREAST = "ChildBreast";
    public const string HUMAN_BODY_CHILDABDOMEN = "ChildAbdomen";
    public const string HUMAN_BODY_CHILDHAND = "ChildHand";
    public const string HUMAN_BODY_CHILDPELVIS = "ChildPelvis";
    public const string HUMAN_BODY_CHILDLEG = "ChildLeg";
    public const string HUMAN_BODY_CHILDSPINE = "ChildSpine";
    public const string HUMAN_BODY_CHILDVEINHEAD = "ChildVeinHead";
    public const string HUMAN_BODY_CHILDVEINNECK = "ChildVeinNeck";
    public const string HUMAN_BODY_CHILDVEINBREAST = "ChildVeinBreast";
    public const string HUMAN_BODY_CHILDVEINABDOMEN = "ChildVeinAbdomen";
    public const string HUMAN_BODY_CHILDVEINLEG = "ChildVeinLeg";
    public const string HUMAN_BODY_CHILDVEINHAND = "ChildVeinHand";
    #endregion

    #region Command String
    public const string COMMAND_TOOLS = "ToolsCommand";
    public const string COMMAND_SWITCH_VIEWS = "SwitchViews";

    public const string COMMAND_RECON_ON = "ReconOnCommand";
    public const string COMMAND_RECON_CUT = "ReconCutCommand";
    public const string COMMAND_RECON_REPEAT = "ReconRepeatCommand";
    public const string COMMAND_RECON_CANCEL = "ReconCancelCommand";
    public const string COMMAND_RECON_ITEMCLICK = "ReconItemClicked";

    public const string COMMAND_MAX = "MaxCommand";
    public const string COMMAND_MIN = "MinCommand";
    public const string COMMAND_CLOSE = "CloseCommand";
    public const string COMMAND_IS_SMART_POSITION = "IsSmartPositioningCommand";

    public const string COMMAND_CONFIRM = "ConfirmCommand";
    public const string COMMAND_RECON_ALL = "ReconAllCommand";
    public const string COMMAND_GO = "GoCommand";
    public const string COMMAND_CANCEL = "CancelCommand";

    public const string COMMAND_SAVE = "SaveCommand";

    public const string COMMAND_SELECT_PROTOCOL = "SelectProtocolCommand";

    public const string COMMAND_HUMAN_BODY_CLICK = "HumanBodyClickCommand";
    public const string COMMAND_ENHANCED_CUSTOMIZED = "EnhancedCustomizedCommand";
    public const string COMMAND_ADD_TASK = "AddTaskCommand";
    public const string COMMAND_REPLACE_TASK = "ReplaceTaskCommand";
    public const string COMMAND_TOP = "TopCommand";
    public const string COMMAND_UNPIN = "UnpinCommand";
    public const string COMMAND_PROTOCOL_ITEM_DOUBLE_CLICK = "ProtocolItemDoubleClickCommand";
    public const string COMMAND_SEARCH = "SearchCommand";

    public const string COMMAND_MARKABLE_TEXT_BOX_GOT_FOCUS = "MarkableTextBoxGotFocus";
    public const string COMMAND_MARKABLE_TEXT_BOX_TEXT_CHANGED = "MarkableTextBoxTextChanged";

    public const string COMMAND_CORRECTION_LENGTH = "CorrectionLengthCommand";
    public const string COMMAND_MARKABLE_TEXT_BOX_LOST_FOCUS = "MarkableTextBoxLostFocus";

    public const string COMMAND_BACK_PROTOCOL_SELECT = "BackProtocolSelectCommand";
    public const string COMMAND_SAVE_AS = "SaveAsCommand";
    public const string COMMAND_OPEN_RECON = "OpenReconCommand";
    public const string COMMAND_SCAN_CLOSED = "ScanClosedCommand";
    public const string COMMAND_GET_PROTOCOL_CONTENT = "GetProtocolContentCommand";
    public const string COMMAND_OPEN_CAMERA = "OpenCameraCommand";
    public const string COMMAND_CUT = "CutCommand";
    public const string COMMAND_COPY = "CopyCommand";
    public const string COMMAND_PASTE = "PasteCommand";
    public const string COMMAND_REPEAT = "RepeatCommand";
    public const string COMMAND_TASK_LIST_ITEM = "TaskListItemCommand";

    public const string COMMAND_SELECT_CHANGED = "SelectionChangedCommand";

    public const string COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_DOWN = "PreviewMouseLeftButtonDown";
    public const string COMMAND_PREVIEW_MOUSE_LEFT_BUTTON_UP = "PreviewMouseLeftButtonUp";
    public const string COMMAND_VALUE_CHANGED = "ValueChanged";

    public const string COMMAND_LOAD = "LoadCommand";
    public const string COMMAND_PASSWORDCHANGED = "PasswordChangedCommand";

    public const string COMMAND_APPLIEDTOALLPROTOCOLS = "AppliedToAllProtocolsCommand";
    #endregion
}
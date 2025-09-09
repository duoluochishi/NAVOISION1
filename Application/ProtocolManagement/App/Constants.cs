
using System.CodeDom;

namespace NV.CT.ProtocolManagement
{

    public static class Constants
    {
        public const string COMMAND_SAVE = "SaveCommand";
        public const string COMMAND_PROTOCOL_TREE_SELECTED_CHANGED = "ProtocolTreeSelectedChangedCommand";
        public const string COMMAND_RIGHT_MENU_CLICK = "RightMenuClickCommand";
        public const string COMMAND_SWITCH_PROTOCOL_FILTER = "SwitchProtocolFilterCommand";
        public const string COMMAND_SWITCH_EMERGENCY_PROTOCOL_FILTER = "SwitchEmergencyProtocolFilterCommand";
        public const string COMMAND_CHECKED = "CheckedCommand";

        public const string PRECISION_FORMAT_2 = "0.##";
        public const string PRECISION_FORMAT_3 = "0.###";
        public const string PRECISION_FORMAT_6 = "0.######";

        public const int HUNDRED_UNIT = 100; //100换算单位
        public const int MILLIMETER_UNIT = 1000; //毫米换算单位
        public const int MICROSECOND_UNIT = 1000000; //微秒换算单位

    }

}
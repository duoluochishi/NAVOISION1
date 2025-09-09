//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/12/1 13:55:07     V1.0.0       an.hu
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Print
{
    public class PrintConstants
    {
        public const string COMMAND_ADD_NEW = "AddNewCommand";
        public const string COMMAND_SAVE = "SaveCommand";
        public const string COMMAND_CLOSE = "CloseCommand";
        public const string COMMAND_DELETE = "DeleteCommand";

        public const string COMMAND_PROTOCOL_SELECTION_CHANGED = "ProtocolSelectionChangedCommand";
        public const string COMMAND_BODYPART_SELECTION_CHANGED = "BodyPartSelectionChangedCommand";
        public const string COMMAND_NAME_CHANGED = "NameChangedCommand";

        public const string COMMAND_SET_PRINT_PROTOCOL = "SetPrintProtocolCommand";
        public const string COMMAND_OPERATE_IMAGE = "OperateImageCommand";
        public const string COMMAND_PRINT = "PrintCommand";
        public const string COMMAND_IMAGE_VIEW_MOUSE_DOUBLE = "ImageViewMouseDoubleClickCommand";

        public const string COMMAND_PREVIEW = "PreviewCommand";
        public const string COMMAND_CANCEL = "CancelCommand";
        public const string COMMAND_SELECT_ALL_CHANGED = "SelectAllChangedCommand";
        public const string COMMAND_IMAGE_SELECTION_CHANGED = "ImagesSelectionChangedCommand";

        public const string PAGE_SIZE_DISPLAY_8X10 = "8x10";
        public const string PAGE_SIZE_VALUE_8X10 = "8INX10IN";
        public const string PAGE_SIZE_DISPLAY_10X12 = "10x12";
        public const string PAGE_SIZE_VALUE_10X12 = "10INX12IN";
        public const string PAGE_SIZE_DISPLAY_11X14 = "11x14";
        public const string PAGE_SIZE_VALUE_11X14 = "11INX14IN";
        public const string PAGE_SIZE_DISPLAY_14X17 = "14x17";
        public const string PAGE_SIZE_VALUE_14X17 = "14INX17IN";

    }
}

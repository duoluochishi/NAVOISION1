//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/23 16:31:55           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.InterventionalScan.Models;

public class CommandParameters
{
    #region  ImageOperater
    public const string IMAGE_OPERATE_LAYOUT = "Layout";
    public const string IMAGE_OPERATE_ZOOM = "Zoom";
    public const string IMAGE_OPERATE_MOVE = "Move";
    public const string IMAGE_OPERATE_WL = "WL";
    public const string IMAGE_OPERATE_ROI = "ROI";
    public const string IMAGE_OPERATE_RESET = "Reset";

    public const string IMAGE_OPERATE_ADD = "Add";
    public const string IMAGE_OPERATE_DELETE = "Delete";

    public const string IMAGE_OPERATE_FORWARD = "Forward";
    public const string IMAGE_OPERATE_BACK = "Back";
    public const string IMAGE_OPERATE_LAST = "Last";
    public const string IMAGE_OPERATE_NEXT = "Next";

    public const string IMAGE_OPERATE_DISTANCE = "Distance";
    public const string IMAGE_OPERATE_ANGLE = "Angle";
    public const string IMAGE_OPERATE_ARROW = "Arrow";
    public const string IMAGE_OPERATE_RECTANGLE = "Rectangle";
    public const string IMAGE_OPERATE_CIRCLE = "Circle";
    public const string IMAGE_OPERATE_SERIESMOVE = "SeriesMove";

    public const string IMAGE_OPERATE_AddNEEDLE = "AddNeedle";
    public const string IMAGE_OPERATE_DELNEEDLE = "DELNeedle";
    public const string IMAGE_OPERATE_SELECTNEEDLE = "SelectNeedle";
    public const string IMAGE_OPERATE_INITNEEDLE = "InitNeedle";
    public const string IMAGE_OPERATE_SETSLICEINDEX = "SetSliceIndex";

    public const string IMAGE_LAYOUT1_1 = "1*1";
    public const string IMAGE_LAYOUT1_2 = "1*2";
    public const string IMAGE_LAYOUT1_3 = "1*3";
    public const string IMAGE_LAYOUT1_4 = "1*4";
    public const string IMAGE_LAYOUT1_5 = "1*5";
    public const string IMAGE_LAYOUT1_6 = "1*6";
    public const string IMAGE_LAYOUT1_7 = "1*7";
    #endregion

    #region Command String
    public const string COMMAND_SETLAYOUT = "SetLayoutCommand";
    public const string COMMAND_MOVE = "MoveCommand";
    public const string COMMAND_ZOOM = "ZoomCommand";
    public const string COMMAND_SETWWWL = "SetWwWlCommand";
    public const string COMMAND_SETROI = "SetROICommand";
    public const string COMMAND_SERIESMOVE = "SeriesMoveCommand";
    public const string COMMAND_NEEDLEOPT = "NeedleOptCommand";
    public const string COMMAND_RESET = "ResetCommand";
    #endregion
}
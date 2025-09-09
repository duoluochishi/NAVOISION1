using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.Extensions
{
    public class StateButton
    {
        public string ButtonName { get; set; } = string.Empty;
        public bool ButtonState { get; set; } = false;
    }
    public enum StateButtonType2D
    {
        tbFlipVertical, 
        tbFlipHorizontal, 
        tbInvert, 
        tbHideTexts, 
        tbReverse
    }
    public enum StateButtonType3D
    {
        tbHideTexts,
        tbAxis,
        txtCut,
        tbClipBox,
        tbRemoveTable,
        txtInvert
    }
}

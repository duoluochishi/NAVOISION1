using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.Extensions
{
    public class ConstName2D
    {
        #region ButtonName
        public const string tbInitialState = "tbInitialState";
        public const string tbFlipHorizontal = "tbFlipHorizontal";
        public const string tbFlipVertical = "tbFlipVertical";
        public const string tbHideTexts = "tbHideTexts";
        public const string tbReverse = "tbReverse";
        public const string tbInvert = "tbInvert";
        public const string txtKernel = "txtKernel";
        public const string txtLayout = "txtLayout";
        public const string txtWWWL = "txtWWWL";
        #endregion
        #region ButtonList
        public static  List<string> switchButtonsNameList =new()  { "txtMove", "tbZoom", "txtWWWL", "txtRotate", "tbScroll" };
        public static  List<string> stateButtonsNameList = new() { "tbFlipVertical", "tbFlipHorizontal", "tbInvert", "tbHideTexts", "tbReverse" };
        public static  List<string> functionalButtonsNameList = new() { "tbInitialState", "tbTags", "tbCine", "tbRework","txtLayout","txtKernel","txtGrid", "txtScreenshot", "txtSynchronization" };
        public static Dictionary<string, bool> StateButtonDictionary = new Dictionary<string, bool> 
        { 
            { "tbFlipVertical", true },
            { "tbFlipHorizontal", true },
            { "tbInvert", true },
            { "tbHideTexts", true },
            { "tbReverse", true },
        };
        #endregion
    }
}

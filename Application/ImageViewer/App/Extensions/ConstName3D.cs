using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.ImageViewer.Extensions
{
    public class ConstName3D
    {
        public const string tbHideTexts = "tbHideTexts";
        public const string tbAxis = "tbAxis";
        public const string txtWWWL = "txtWWWL";
        public const string txtRotate = "txtRotate";
        public const string txtMMPR = "txtMMPR";
        public const string txtCut = "txtCut";
        public const string tbClipBox = "tbClipBox";
        public const string tbRemoveTable = "tbRemoveTable";
        public const string txtInvert = "txtInvert";
        public static List<string> switchButtonsNameList = new() { "txtMove", "tbZoom", "txtWWWL", "txtRotate"};
        public static List<string> stateButtonsNameList = new() {  "tbHideTexts", "tbAxis", "txtCut", "tbClipBox", "tbRemoveTable","txtInvert" };
        public static List<string> functionalButtonsNameList = new() { "tbInitialState",  "tbRework", "txtLayout",  "txtScreenshot", "tbVRTReset", "txtPreset"};
        public static Dictionary<string, bool> StateButtonDictionary = new Dictionary<string, bool>
        {
            { "tbHideTexts", true },
            { "tbAxis", true },
            { "txtCut", true },
            { "tbClipBox", true },
            { "tbRemoveTable",true},
            { "txtInvert",true}
        };
    }
}

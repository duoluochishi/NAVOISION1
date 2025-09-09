//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/11 9:59:28           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS.Enums;
using NVCTImageViewerInterop;

using System.Collections.Generic;

namespace NV.CT.DicomImageViewer;
public class ImageSettingToOverlayText
{
    public static (OverlayTextStyle textStyle, List<OverlayText> texts) Get(ImageAnnotationSetting fourCornersConfig)
    {
        OverlayTextStyle overlayTextStyle = new OverlayTextStyle();
        List<OverlayText> overlayTexts = new List<OverlayText>();
        if (fourCornersConfig is not null)
        {
            overlayTextStyle.FontTTFPath = string.Empty;
            overlayTextStyle.FontSize = fourCornersConfig.FontSize;
            overlayTextStyle.FontColorR = fourCornersConfig.FontColorR;
            overlayTextStyle.FontColorG = fourCornersConfig.FontColorG;
            overlayTextStyle.FontColorB = fourCornersConfig.FontColorB;
            foreach (var item in fourCornersConfig.AnnotationItemSettings)
            {
                OverlayText overlayText = new OverlayText();
                overlayText.Name = item.Name.Replace("WW/WL", "WW");
                overlayText.Row = item.Row;
                overlayText.Column = item.Column;
                overlayText.DicomGroupNumber = item.DicomTagGroup;
                overlayText.DicomElementNumber = item.DicomTagId;
                overlayText.PreText = item.TextPrefix;
                overlayText.PostText = item.TextSuffix;
                overlayText.Visibility = item.Visibility;
                switch (item.TextSource)
                {
                    case FourCornerTextSource.DicomTag:
                        overlayText.Source = OverlayTextSource.Overlay_DICOMTAG;
                        break;
                    case FourCornerTextSource.UserDefined:
                    default:
                        overlayText.Source = OverlayTextSource.Overlay_USERDEFINED;
                        break;
                }
                overlayText.Alignment = GetAlignment(item.Location);
                if (overlayText.Source == OverlayTextSource.Overlay_USERDEFINED)
                {
                    overlayText.InterMode = GetInterMode(overlayText.Name);
                }
                overlayTexts.Add(overlayText);
            }
        }
        return (overlayTextStyle, overlayTexts);
    }

    private static OverlayTextPosition GetAlignment(FourCornersLocation fourCornersLocation)
    {
        OverlayTextPosition overlayTextPosition = OverlayTextPosition.LowerEdge;
        switch (fourCornersLocation)
        {
            case FourCornersLocation.LeftTop:
                overlayTextPosition = OverlayTextPosition.UpperLeft;
                break;
            case FourCornersLocation.LeftCenter:
                overlayTextPosition = OverlayTextPosition.LeftEdge;
                break;
            case FourCornersLocation.LeftBottom:
                overlayTextPosition = OverlayTextPosition.LowerLeft;
                break;
            case FourCornersLocation.RightTop:
                overlayTextPosition = OverlayTextPosition.UpperRight;
                break;
            case FourCornersLocation.RightCenter:
                overlayTextPosition = OverlayTextPosition.RightEdge;
                break;
            case FourCornersLocation.RightBottom:
                overlayTextPosition = OverlayTextPosition.LowerRight;
                break;
            case FourCornersLocation.CenterTop:
                overlayTextPosition = OverlayTextPosition.UpperEdge;
                break;
            case FourCornersLocation.CenterBottom:
            default:
                overlayTextPosition = OverlayTextPosition.LowerEdge;
                break;
        }
        return overlayTextPosition;
    }

    private static OverlayTextInterMode GetInterMode(string name)
    {
        OverlayTextInterMode overlayTextInterMode = OverlayTextInterMode.Overlay_WL;
        if (name.Equals(OverlayNames.OVERLAY_WL) )
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_WL;
        }
        else if (name.Equals(OverlayNames.OVERLAY_WW) || name.Equals(OverlayNames.OVERLAY_WW_WL))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_WW;
        }
        else if (name.Equals(OverlayNames.OVERLAY_ORIENTATION_CHAR_LEFT))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_OrientationCharLeft;
        }
        else if (name.Equals(OverlayNames.OVERLAY_ORIENTATION_CHAR_BOTTOM))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_OrientationCharBottom;
        }
        else if (name.Equals(OverlayNames.OVERLAY_ORIENTATION_CHAR_TOP))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_OrientationCharTop;
        }
        else if (name.Equals(OverlayNames.OVERLAY_ORIENTATION_CHAR_RIGHT))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_OrientationCharRight;
        }
        //else if (name.Equals(OverlayNames.OVERLAY_SLICE_LOCATION))
        //{
        // overlayTextInterMode = OverlayTextInterMode.Overlay_SliceLocation;
        //}
        else if (name.Equals(OverlayNames.OVERLAY_SLICE_NUMBER))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_SliceNumber;
        }
        else if (name.Equals(OverlayNames.OVERLAY_SLICE_THICKNESS))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_SliceThickness;
        }
        else if (name.Equals(OverlayNames.OVERLAY_ZOOM_RATIO))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_ZoomRatio;
        }
        else if (name.Equals(OverlayNames.OVERLAY_PET_PIXEL_BODY_POS))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_PixelBodyPos;
        }
        else if (name.Equals(OverlayNames.OVERLAY_PET_PIXEL_VALUE))
        {
            overlayTextInterMode = OverlayTextInterMode.Overlay_PixelValue;
        }
        return overlayTextInterMode;
    }
}
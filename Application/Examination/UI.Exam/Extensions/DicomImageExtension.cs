//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/20 9:55:30           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DicomImageViewer;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;
using NVCTImageViewerInterop;
using ParameterNames = NV.CT.Protocol.Models.ProtocolParameterNames;

namespace NV.CT.UI.Exam.Extensions;

public class DicomImageExtension
{
    public static void LoadTopoImage(TopoImageViewer topoImageViewer, ReconModel reconModel, string imageInfo, bool isDynamic = false)
    {
        if (reconModel is null)
        {
            return;
        }
        string filePath = imageInfo;
        if (string.IsNullOrEmpty(imageInfo) || !File.Exists(imageInfo))
        {
            if (Directory.Exists(imageInfo))
            {
                filePath = Directory.GetFiles(imageInfo, "*.dcm").FirstOrDefault() ?? "";
            }
        }

        if (isDynamic)
        {
            topoImageViewer.LoadImageWithFilePath(filePath, reconModel.ImageMatrixVertical, GetDirectionByScan(reconModel));
        }
        else
        {
            topoImageViewer.LoadImageWithFilePath(filePath);
        }
        topoImageViewer.SetWindowWidthLevel(reconModel.ViewWindowWidth, reconModel.ViewWindowCenter);
    }

    public static void LoadTomoImage(IImageOperationService imageOperationService, TomoImageViewer tomoImageViewer, TopoImageViewer topoImageViewer, ReconModel reconModel, string imageInfo, ref bool isTomoLoading)
    {
        if (isTomoLoading)
        {
            return;
        }
        isTomoLoading = true;
        if (!string.IsNullOrEmpty(imageInfo) && Directory.Exists(imageInfo))
        {
            if (tomoImageViewer.CurrentContentPath != imageInfo)
            {
                tomoImageViewer.LoadImageWithDirectoryPath(imageInfo);

                int imageCount = Directory.GetFiles(imageInfo, "*.dcm").Count();
                imageOperationService.SetImageCount(imageCount);
                tomoImageViewer.SetWindowWidthLevel(reconModel.ViewWindowWidth, reconModel.ViewWindowCenter);
            }
            topoImageViewer.ShowScanLine(true);
        }
        else
        {
            tomoImageViewer.ClearView();
            topoImageViewer.ShowScanLine(false);
        }
        isTomoLoading = false;
    }

    public static void LoadPlanBox(TopoImageViewer topoImageViewer, ScanModel tomoScanModel, ReconModel? reconModel)
    {
        if (reconModel is null || reconModel.Status != PerformStatus.Performed || string.IsNullOrEmpty(reconModel.ImagePath))
        {
            return;
        }
        if ((File.GetAttributes(reconModel.ImagePath) & FileAttributes.Directory) == FileAttributes.Directory)
        {
            if (!Directory.Exists(reconModel.ImagePath) || Directory.GetFiles(reconModel.ImagePath).Length == 0)
            {
                return;
            }
        }
        else
        {
            if (!File.Exists(reconModel.ImagePath))
            {
                return;
            }
        }

        List<LocationParam> lists = new List<LocationParam>();
        int i = 1;
        foreach (var recon in tomoScanModel.Children)
        {
            LocationParam parm = new LocationParam();
            parm.ScanID = tomoScanModel.Descriptor.Id;
            parm.LocationSeriesUID = recon.Descriptor.Id;
            if (recon.IsRTD)
            {
                parm.LocationSeriesName = "RT";
            }
            else
            {
                parm.LocationSeriesName = tomoScanModel.ScanNumber + "-" + i;
                i++;
            }
            parm.CenterFirstX = recon.CenterFirstX;
            parm.CenterFirstY = recon.CenterFirstY;
            parm.CenterFirstZ = recon.CenterFirstZ;
            parm.CenterLastX = recon.CenterLastX;
            parm.CenterLastY = recon.CenterLastY;
            parm.CenterLastZ = recon.CenterLastZ;
            parm.FoVLengthHor = recon.FOVLengthHorizontal;
            parm.FoVLengthVer = recon.FOVLengthVertical;
            parm.FOVDirectionHorX = recon.FOVDirectionHorizontalX;
            parm.FOVDirectionHorY = recon.FOVDirectionHorizontalY;
            parm.FOVDirectionHorZ = recon.FOVDirectionHorizontalZ;
            parm.FOVDirectionVerX = recon.FOVDirectionVerticalX;
            parm.FOVDirectionVerY = recon.FOVDirectionVerticalY;
            parm.FOVDirectionVerZ = recon.FOVDirectionVerticalZ;

            parm.IsSquareFixed = recon.FOVLengthHorizontal == recon.FOVLengthVertical;
            parm.IsChild = !recon.IsRTD;
            parm.Status = recon.Status;
            parm.FailureReasonType = recon.FailureReason;
            lists.Add(parm);
        }
        if (lists.Count > 0)
        {
            switch (tomoScanModel.ScanOption)
            {
                case ScanOption.NVTestBolusBase:
                case ScanOption.NVTestBolus:
                case ScanOption.TestBolus:
                    topoImageViewer.SetLocationScanMode(LOCATION_SCAN_MODE.LowDoseSacn, ProtocolParameterNames.RECON_DEFAULT_ROI_RADIUS);
                    break;
                default:
                    topoImageViewer.SetLocationScanMode(LOCATION_SCAN_MODE.NormalSacn, 1);
                    break;
            }
            topoImageViewer.LoadPlanBox(lists);
        }

        if (tomoScanModel.ScanOption == ScanOption.NVTestBolus
                || tomoScanModel.ScanOption == ScanOption.TestBolus)
        {
            topoImageViewer.SetLocalizerLocked(true);
        }
        else
        {
            topoImageViewer.SetLocalizerLocked(false);
        }
    }

    public static void HandlePerformedTopoSelection(TopoImageViewer topoImageViewer, ScanModel? tomoScanModel, ReconModel? reconModel, int tablePosition)
    {
        if (reconModel is null || reconModel.Status != PerformStatus.Performed || !Directory.Exists(reconModel.ImagePath))
        {
            return;
        }
        var imageFile = Directory.GetFiles(reconModel.ImagePath, "*.dcm").FirstOrDefault();
        if (File.Exists(imageFile))
        {
            if (imageFile != topoImageViewer.CurrentContentPath)
            {
                topoImageViewer.ClearLocalizer();
                LoadTopoImage(topoImageViewer, reconModel, imageFile);
            }
            if (tomoScanModel is not null)
            {
                LoadPlanBox(topoImageViewer, tomoScanModel, reconModel);
            }
            else
            {
                topoImageViewer.ClearLocalizer();
            }
            topoImageViewer.SetTableShow(true);
            SetTablePositionOnCurrentSelectedTopo(topoImageViewer, reconModel, tablePosition);
        }
    }

    public static void HandleUnPerformedTopoSelection(TopoImageViewer topoImageViewer)
    {
        topoImageViewer.ClearLocalizer();
        topoImageViewer.ClearView();
    }

    public static void HandleUnperformedTomoSelection(TomoImageViewer tomoImageViewer, TopoImageViewer topoImageViewer)
    {
        tomoImageViewer.ClearView();
        //todo：比较好的是设置不显示，回头查看逻辑。
        topoImageViewer.SetScanLinePosition(-2200000);
    }

    public static void SetTablePositionOnCurrentSelectedTopo(TopoImageViewer topoImageViewer, ReconModel? reconModel, int tablePosition)
    {
        if (topoImageViewer is null || reconModel is null || reconModel.Status is not PerformStatus.Performed)
        {
            return;
        }
        var patientPosition = reconModel.Parent.Parent.Parent.PatientPosition;
        var tablePositionInPatient = CoordinateConverter.Instance.TransformSliceLocationDeviceToPatient(patientPosition, UnitConvert.Micron2Millimeter((double)tablePosition));
        topoImageViewer.SetTablePosition(tablePositionInPatient);
    }

    public static void HandleWindowWidthLevelChanged(IProtocolHostService protocolHostService, ReconModel? reconModel, double wl, double ww)
    {
        if (reconModel is not null)
        {
            List<ParameterModel> parameterModels = new List<ParameterModel>
            {
                new ParameterModel{Name =ParameterNames.RECON_VIEW_WINDOW_CENTER,Value= ((int)wl).ToString()},
                new ParameterModel{Name = ParameterNames.RECON_VIEW_WINDOW_WIDTH,Value = ((int)ww).ToString()},
            };
            protocolHostService.SetParameters(reconModel, parameterModels);
        }
    }

    private static int GetDirectionByScan(ReconModel reconModel)
    {
        int direction = 1;
        switch (reconModel.ImageOrder)
        {
            case ImageOrders.HeadFoot:
                direction = 1;
                break;
            default:
                direction = -1;
                break;
        }
        return direction;
    }

    public static void SetTestBolusCycleROIsByBase(IProtocolHostService protocolHostService, ReconModel testBolusReconModel)
    {
        testBolusReconModel.CycleROIs = new List<CycleROIModel>();
        var testBolusScan = protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(testBolusReconModel.Parent.Descriptor.Id));
        int index = protocolHostService.Models.IndexOf(testBolusScan);
        if (index > 0 && protocolHostService.Models.Count >= index - 1)
        {
            var pairs = protocolHostService.Models[index - 1];
            if (pairs.Scan is ScanModel scan
                && scan.Status == PerformStatus.Performed
                && scan.ScanOption == ScanOption.NVTestBolusBase
                && scan.Children.FirstOrDefault(t => t.IsRTD && t.Status == PerformStatus.Performed) is ReconModel reconBolusBase)
            {
                foreach (CycleROIModel cycleROIModel in reconBolusBase.CycleROIs)
                {
                    testBolusReconModel.CycleROIs.Add(cycleROIModel);
                }
            }
        }
    }
}
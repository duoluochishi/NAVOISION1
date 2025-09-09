//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
//  2023/2/3 9:26:10        V1.0.0      Jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Language;
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Globalization;

namespace NV.CT.Examination.ApplicationService.Impl;

public class ImageOperationService : IImageOperationService
{
    public bool IsInverted { get; private set; } = false;

    public event EventHandler<EventArgs<string>>? ClickToolButtonChanged;

    public event EventHandler<EventArgs<int>>? SetImageSliceLocationChanged;

    public event EventHandler<EventArgs<int>>? ImageSliceIndexChanged;

    public event EventHandler<EventArgs<int>>? ImageCountChanged;

    public event EventHandler<EventArgs<bool>>? SwitchViewsChanged;

    public event EventHandler<EventArgs<double>>? CenterPositionChanged;
    public event EventHandler<EventArgs<string>>? OnSelectionReconIDChanged;
    public event EventHandler<EventArgs<bool>>? OnROICreateSucceedEvent;
    public event EventHandler<EventArgs<string>>? TimeDensityInfoChangedNotify;
    public event EventHandler<EventArgs<(string commandStr, string param)>>? CommondToTimeDensityEvent;

    public event EventHandler<EventArgs<string>>? TimeDensityRoiRemoved;

    public event EventHandler? TimeDensityDeleteAllRoi;

    private readonly IProtocolHostService _protocolHostService;
    private readonly IDialogService _dialogService;

    public ImageOperationService(IProtocolHostService protocolHostService,
        IDialogService dialogService)
    {
        _protocolHostService = protocolHostService;
        _dialogService = dialogService;
    }

    public double CurrentPositon { get; private set; }
    public void DoToolsBarCommand(string commandStr)
    {
        ClickToolButtonChanged?.Invoke(this, new CTS.EventArgs<string>(commandStr));
    }

    public void SetImageSliceLocation(int index)
    {
        SetImageSliceLocationChanged?.Invoke(this, new CTS.EventArgs<int>(index));
    }

    public void SetInverted()
    {
        IsInverted = !IsInverted;
    }

    public void SetImageSliceIndex(int index)
    {
        ImageSliceIndexChanged?.Invoke(this, new CTS.EventArgs<int>(index));
    }

    public void SetImageCount(int maxNumber)
    {
        IsInverted = false;
        ImageCountChanged?.Invoke(this, new CTS.EventArgs<int>(maxNumber));
    }

    public void SwitchViews()
    {
        SwitchViewsChanged?.Invoke(this, new CTS.EventArgs<bool>(true));
    }

    public void SetCenterPositon(double currentPositon)
    {
        CurrentPositon = currentPositon;
    }

    public void SetCurrentToCenterPositon()
    {
        bool rFlag = false;
        if (BeginRangeJudgmentIsConsistent())
        {
            _dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Confirm_Title, LanguageResource.Message_Confirm_CompletedInterventionTaskContinue, arg =>
            {
                if (arg.Result == ButtonResult.Cancel || arg.Result == ButtonResult.Close)
                {
                    rFlag = true;
                }
            }, ConsoleSystemHelper.WindowHwnd);
        }
        if (rFlag)
        {
            return;
        }
        var list = _protocolHostService.Models.ToList().FindAll(t => t.Scan.IsIntervention && t.Scan.Status == PerformStatus.Unperform).ToList();
        if (list is not null && list.Count > 0)
        {
            foreach (var item in list)
            {
                List<ParameterModel> parameterModels = new List<ParameterModel>();

                int length = UnitConvert.Micron2Millimeter((int)item.Scan.ScanLength);
                double beginRange = item.Scan.TableDirection == TableDirection.In ? CurrentPositon + length / 2 : CurrentPositon - length / 2;
                double endRange = item.Scan.TableDirection == TableDirection.In ? beginRange - length : beginRange + length;
                parameterModels.Add(new ParameterModel
                {
                    Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION,
                    Value = UnitConvert.Millimeter2Micron(beginRange).ToString(CultureInfo.InvariantCulture)
                });
                parameterModels.Add(new ParameterModel
                {
                    Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION,
                    Value = UnitConvert.Millimeter2Micron(endRange).ToString(CultureInfo.InvariantCulture)
                });
                _protocolHostService.SetParameters(item.Scan, parameterModels);
                ScanLengthHelper.GetCorrectedScanLength(_protocolHostService, item.Scan);
            }
        }
        CenterPositionChanged?.Invoke(this, new EventArgs<double>(CurrentPositon));
    }

    /// <summary>
    /// 判断已完成的介入扫描中心帧是否一致
    /// </summary>
    /// <returns></returns>
    private bool BeginRangeJudgmentIsConsistent()
    {
        return _protocolHostService.Models.Select(t => t.Scan).Any(t => t.IsIntervention && t.Status == PerformStatus.Performed && GetBeginRangeJudgmentIsConsistent(t));
    }

    private bool GetBeginRangeJudgmentIsConsistent(ScanModel scanModel)
    {
        int length = UnitConvert.Micron2Millimeter((int)scanModel.ScanLength);
        double beginRange = scanModel.TableDirection == TableDirection.In ? CurrentPositon + length / 2 : CurrentPositon - length / 2;
        return UnitConvert.Millimeter2Micron(beginRange) != scanModel.ReconVolumeStartPosition;
    }

    public void SetSelectionReconID(string reconID)
    {
        OnSelectionReconIDChanged?.Invoke(this, new CTS.EventArgs<string>(reconID));
    }

    public void SetROICreateSucceedEvent(bool success)
    {
        OnROICreateSucceedEvent?.Invoke(this, new CTS.EventArgs<bool>(success));
    }

    public void SetTimeDensityInfoChanged(string param)
    {
        TimeDensityInfoChangedNotify?.Invoke(this, new CTS.EventArgs<string>(param));
    }

    public void SetCommondToTimeDensity(string commandStr, string param)
    {
        CommondToTimeDensityEvent?.Invoke(this, new EventArgs<(string commandStr, string param)>((commandStr, param)));
    }

    public void SetTimeDensityRoiRemoved(string id)
    {
        TimeDensityRoiRemoved?.Invoke(this, new EventArgs<string>(id));
    }

    public void DeleteAllTimeDensityRoi()
    {
        TimeDensityDeleteAllRoi?.Invoke(this, new EventArgs());
    }
}
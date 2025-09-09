//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.Language;
using System.Collections.Generic;
using NV.MPS.Environment;

namespace NV.CT.ConfigManagement.ViewModel;

public class RTDReconParamViewModel : ReconParamViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<RTDReconParamViewModel> _logger;   

	public RTDReconParamViewModel(
        IDialogService dialogService,
        ILogger<RTDReconParamViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;       
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        ReconParamInfo node = SystemConfig.RTDReconParamConfig.RTDReconParam;      
        foreach (var dItem in ReconTypes)
        {
            dItem.IsChecked = false;
            foreach (var item in node.ReconType.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }      
        foreach (var dItem in SliceThicknes)
        {
            dItem.IsChecked = false;
            foreach (var item in node.SliceThickness.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }       
        foreach (var dItem in ImageIncrements)
        {
            dItem.IsChecked = false;
            foreach (var item in node.ImageIncrement.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }     
        foreach (var dItem in ReconMatrixs)
        {
            dItem.IsChecked = false;
            foreach (var item in node.ReconMatrix.Ranges)
            {
                if (item.Equals(dItem.Key))
                {
                    dItem.IsChecked = true;
                }
            }
        }
        DefinedMinPixelSpacing = node.DefinedMinPixelSpacing.Value;
	}

    public void Saved()
    {
        ReconParamInfo node = SystemConfig.RTDReconParamConfig.RTDReconParam;      
        node.ReconType.Ranges = new List<string>();
        foreach (var dItem in ReconTypes)
        {
            if (dItem.IsChecked)
            {
                node.ReconType.Ranges.Add(dItem.Key);
            }
        }      
        node.SliceThickness.Ranges = new List<int>();
        foreach (var dItem in SliceThicknes)
        {
            if (dItem.IsChecked)
            {
                node.SliceThickness.Ranges.Add(dItem.Key);
            }
        }       
        node.ImageIncrement.Ranges = new List<int>();
        foreach (var dItem in ImageIncrements)
        {
            if (dItem.IsChecked)
            {
                node.ImageIncrement.Ranges.Add(dItem.Key);
            }
        }      
        node.ReconMatrix.Ranges = new List<int>();
        foreach (var dItem in ReconMatrixs)
        {
            if (dItem.IsChecked)
            {
                node.ReconMatrix.Ranges.Add(dItem.Key);
            }
        }
        node.DefinedMinPixelSpacing.Value = (int)DefinedMinPixelSpacing;
		SystemConfig.RTDReconParamConfig.RTDReconParam = node;
        bool saveFlag = SystemConfig.SaveRTDReconParamConfig();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
}
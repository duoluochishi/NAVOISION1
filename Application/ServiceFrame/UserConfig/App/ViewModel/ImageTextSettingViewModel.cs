
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.Language;
using NV.CT.UI.ViewModel;
using NV.CT.UserConfig.ApplicationService.Impl;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using System;
using System.Linq;

namespace NV.CT.UserConfig.ViewModel;
public class ImageTextSettingViewModel : BaseViewModel
{
	private readonly ImageTextSettingService _imageTextSettingService;
	private readonly IDialogService _dialogService;
	private int maxCountItems = 6;

	public ImageTextSettingViewModel(IDialogService dialogService,
		ImageTextSettingService imageTextSettingService)
	{
		_dialogService = dialogService;
		_imageTextSettingService = imageTextSettingService;
		Commands.Add("LoadImageAnnotationCommand", new DelegateCommand<string>(LoadImageAnnotationCommand));
		Commands.Add("SaveImageAnnotationCommand", new DelegateCommand(SaveImageAnnotationCommand));
	}

	private void LoadImageAnnotationCommand(string name)
	{
		_imageTextSettingService.SwitchViewType(ImageAnnotationSettingNames.ScanTopo);
	}

	private void SaveImageAnnotationCommand()
	{
		if (!LimitationRowCount())
		{
			_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", $"No more than {maxCountItems} rows.",
				 arg => { }, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		_imageTextSettingService.SaveConfig();
		_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", LanguageResource.Message_Info_SaveSuccessfullyPara,
				  arg => { }, ConsoleSystemHelper.WindowHwnd);
	}

	private bool LimitationRowCount()
	{
		ImageAnnotationConfig imageAnnotationConfig = _imageTextSettingService.CurrentImageAnnotationConfig;
		if (!LimitationScanTopoRowCount(imageAnnotationConfig))
		{
			return false;
		}
		//ScanTomoSettings		
		if (!LimitationScanTomoRowCount(imageAnnotationConfig))
		{
			return false;
		}
		//PrintSettings			
		if (!LimitationPrintRowCount(imageAnnotationConfig))
		{
			return false;
		}
		//MPRSettings			
		if (!LimitationMPRRowCount(imageAnnotationConfig))
		{
			return false;
		}
		//ViewSettings			
		if (!LimitationViewRowCount(imageAnnotationConfig))
		{
			return false;
		}
		//VRSettings			
		if (!LimitationVRRowCount(imageAnnotationConfig))
		{
			return false;
		}
		return true;
	}

	private bool LimitationScanTopoRowCount(ImageAnnotationConfig imageAnnotationConfig)
	{
		int maxCount = imageAnnotationConfig.ScanTopoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ScanTopoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ScanTopoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ScanTopoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		return true;
	}

	private bool LimitationScanTomoRowCount(ImageAnnotationConfig imageAnnotationConfig)
	{
		int maxCount = imageAnnotationConfig.ScanTomoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ScanTomoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ScanTomoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ScanTomoSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		return true;
	}

	private bool LimitationPrintRowCount(ImageAnnotationConfig imageAnnotationConfig)
	{
		int maxCount = imageAnnotationConfig.PrintSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.PrintSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.PrintSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.PrintSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		return true;
	}

	private bool LimitationMPRRowCount(ImageAnnotationConfig imageAnnotationConfig)
	{
		int maxCount = imageAnnotationConfig.MPRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.MPRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.MPRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.MPRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		return true;
	}

	private bool LimitationViewRowCount(ImageAnnotationConfig imageAnnotationConfig)
	{
		int maxCount = imageAnnotationConfig.ViewSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ViewSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ViewSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.ViewSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		return true;
	}

	private bool LimitationVRRowCount(ImageAnnotationConfig imageAnnotationConfig)
	{
		int maxCount = imageAnnotationConfig.VRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.VRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.LeftBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.VRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightTop && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		maxCount = imageAnnotationConfig.VRSettings.AnnotationItemSettings.Count(t => t.Location == FourCornersLocation.RightBottom && t.Visibility && t.Column == 0);
		if (maxCount > maxCountItems)
		{
			return false;
		}
		return true;
	}
}
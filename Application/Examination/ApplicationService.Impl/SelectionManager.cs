//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.CTS;
using System.Collections.Generic;
using NV.CT.Examination.ApplicationService.Contract.Interfaces;
using NV.MPS.Exception;

namespace NV.CT.Examination.ApplicationService.Impl;

public class SelectionManager : ISelectionManager
{
	private readonly IProtocolHostService _protocolHostService;
	public event EventHandler<EventArgs<(ReconModel Recon, string Image)>>? ReconImageLoaded;
	/// <summary>
	/// 当前选中对象 (FOV,Measurement,Scan)
	/// </summary>
	public (FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan) CurrentSelection { get; private set; }

	/// <summary>
	/// 当前选中 Recon
	/// </summary>
	public ReconModel CurrentSelectionRecon { get; private set; }

	/// <summary>
	/// 当前选中 TopoScan
	/// </summary>
	public ScanModel LastSelectionTopoScan { get; private set; }

	/// <summary>
	/// 当前选中 TomoScan
	/// </summary>
	public ScanModel LastSelectionTomoScan { get; private set; }

	public event EventHandler<EventArgs<ScanModel>>? SelectionScanChanged;
	public event EventHandler<EventArgs<ReconModel>>? SelectionReconChanged;
	public event EventHandler? SelectionCleared;

	public SelectionManager(IProtocolHostService protocolHostService)
	{
		_protocolHostService = protocolHostService;
		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
		_protocolHostService.PerformStatusChanged += ProtocolHostService_PerformStatusChanged;
	}

	/// <summary>
	/// scan、measurement状态发生变化时的默认选中过程。 
	/// Scan状态发生变化时：
	///     当扫描状态变化为Performing时，强制尝试选中当前该扫描。
	///     若不是autoscan，选择不应发生变化，因为在load前已经选中一次了。
	///     若是autoscan，选择发生变化，切换到当前扫描任务。
	/// Measurement状态发生变化时：
	///     若状态从Unperformed变为Performing，强制尝试选中当前Measurement的第一个扫描。
	///     若定位像状态从Performing变为Performed，尝试选中下一个未完成的断层扫描。
	///     若断层扫描状态从Performing变为performed，维持当前选择状态。
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void ProtocolHostService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		//Scan Performing
		if (e.Data.NewStatus == PerformStatus.Performing && e.Data.Model is ScanModel scanModel)
		{
			var thisTuple = _protocolHostService.Models.FirstOrDefault(n => n.Scan.Descriptor.Id == scanModel.Descriptor.Id);

			if (thisTuple.Measurement is null || thisTuple.Scan is null)
				return;

			SelectScan(thisTuple.Frame.Descriptor.Id, thisTuple.Measurement.Descriptor.Id
					, thisTuple.Scan.Descriptor.Id);
		}
		//Measurement done
		else if (e.Data.NewStatus == PerformStatus.Performed && e.Data.Model is MeasurementModel measurementModel)
		{
			var thisTuple = _protocolHostService.Models.FirstOrDefault(n => n.Measurement.Descriptor.Id == measurementModel.Descriptor.Id);

			if (thisTuple.Measurement is null)
				return;

			var nextMeasurement = thisTuple.Frame.Children.FindNext(n => n.Descriptor.Id == measurementModel.Descriptor.Id);
			if (nextMeasurement is null)
				return;

			var firstScanInNextMeasurement = nextMeasurement.Children.FirstOrDefault();
			if (firstScanInNextMeasurement is null)
				return;

			if ((thisTuple.Scan.ScanImageType is ScanImageType.Topo)
				&& firstScanInNextMeasurement.ScanImageType is ScanImageType.Tomo)
			{
				SelectScan(firstScanInNextMeasurement.Parent.Parent.Descriptor.Id, firstScanInNextMeasurement.Parent.Descriptor.Id
					, firstScanInNextMeasurement.Descriptor.Id);
			}
		}
	}

	private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		switch (e.Data.ChangeType)
		{
			case StructureChangeType.Add:
				break;
			case StructureChangeType.Delete:
				if (e.Data.Current is not null)
				{
					StructureChangeTypedDelete(e.Data.Current);
				}
				break;
			case StructureChangeType.Modify:
				break;
			case StructureChangeType.Replace:
				break;
			case StructureChangeType.None:
				break;
		}
	}

	private void StructureChangeTypedDelete(BaseModel baseModel)
	{
		if (baseModel is ReconModel && CurrentSelection.Scan is not null)
		{
			SelectRecon(CurrentSelection.Scan.Children.FirstOrDefault());
		}
		else if (baseModel is ScanModel)
		{
			//先清空相关选择
			var scanModel = baseModel as ScanModel;
			if (CurrentSelection.Scan == scanModel)
			{
				CurrentSelection = default;
			}
			if (scanModel?.ScanImageType == ScanImageType.Topo)
			{
				LastSelectionTopoScan = null;
			}
			else if (scanModel?.ScanImageType == ScanImageType.Tomo)
			{
				LastSelectionTomoScan = null;
			}
			SelectScan();   //采用默认选中。
		}
		else if (baseModel is MeasurementModel)
		{
			//先清空相关选择
			var allModels = _protocolHostService.Models;
			if (!allModels.Any(x => x.Scan == LastSelectionTopoScan))
			{
				LastSelectionTopoScan = null;
			}
			if (!allModels.Any(x => x.Scan == LastSelectionTomoScan))
			{
				LastSelectionTomoScan = null;
			}
			SelectScan();   //采用默认选中。
		}
	}

	/// <summary>
	/// 如果有用户选中的定位像,不要改变.
	/// 如果选择断层,此时没有默认的定位像,找离这个断层最近的定位像并赋值,如果已有定位像,不要赋值!!!
	/// </summary>
	public void SelectScan(string forId, string measurementId, string scanId)
	{
		if (_protocolHostService.Instance is null || string.IsNullOrEmpty(scanId))
		{
			//TODO: message 处理成语言资
			throw new NanoException("", "No protocol, or Scan is not exist!");
		}

		var currentItem = _protocolHostService.Models.FirstOrDefault(n => n.Scan.Descriptor.Id == scanId);

		if (currentItem.Scan is null)
		{
			//TODO: message 处理成语言资源
			throw new NanoException("", $"Scan (Id:{scanId}) is not exist!");
		}

		if (currentItem == CurrentSelection)
		{
			return;
		}

		CurrentSelection = currentItem;

		if (CurrentSelection.Scan.ScanImageType == ScanImageType.Topo && CurrentSelection.Scan != LastSelectionTopoScan)
		{
			LastSelectionTopoScan = CurrentSelection.Scan;
		}

		//如果是Tomo,当前选中扫描与之前选中扫描不一致,则赋值
		if (CurrentSelection.Scan.ScanImageType == ScanImageType.Tomo && CurrentSelection.Scan != LastSelectionTomoScan)
		{
			LastSelectionTomoScan = CurrentSelection.Scan;

			//如果这时定位像是空,则找到当前Tomo最近的定位像并赋值
			if (LastSelectionTopoScan == null)
			{
				var wholeList = _protocolHostService.Models.ToList();
				//stop index
				var stopIndex = wholeList.FindIndex(n => n == CurrentSelection);
				//match condition
				var targetLastTopo = wholeList.Take(stopIndex).LastOrDefault(n => n.Scan.ScanImageType == ScanImageType.Topo);
				if (targetLastTopo.Scan != null)
				{
					LastSelectionTopoScan = targetLastTopo.Scan;
				}
			}
		}

		if (!CurrentSelection.Scan.Children.Contains(CurrentSelectionRecon))
		{
			SelectRecon(CurrentSelection.Scan.Children.FirstOrDefault());
		}

		SelectionScanChanged?.Invoke(this, new EventArgs<ScanModel>(CurrentSelection.Scan));
	}

	/// <summary>
	/// 默认选中次序：
	///     第一个待扫描的scan
	///     最后一个已完成的扫描
	///     空
	/// </summary>
	public void SelectScan()
	{
		var selectionItem = _protocolHostService.Models.FirstOrDefault(n => n.Scan.Status == PerformStatus.Unperform);
		if (selectionItem.Scan is not null)
		{
			//如果未完成扫描未非定位像扫描，这找寻是否存在已经完成的定位像，如果存在并选中它。
			if (!(selectionItem.Scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.Surview || selectionItem.Scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.DualScout))
			{
				if (LastSelectionTopoScan is null)
				{
					var selectionTopoItem = _protocolHostService.Models.FirstOrDefault(n => n.Scan.Status == PerformStatus.Performed && (n.Scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.Surview || n.Scan.ScanOption == FacadeProxy.Common.Enums.ScanOption.DualScout));
					{
						if (selectionTopoItem.Scan is not null)
						{
							SelectScan(selectionTopoItem.Frame.Descriptor.Id, selectionTopoItem.Measurement.Descriptor.Id, selectionTopoItem.Scan.Descriptor.Id);
						}
					}
				}
			}
			SelectScan(selectionItem.Frame.Descriptor.Id, selectionItem.Measurement.Descriptor.Id, selectionItem.Scan.Descriptor.Id);
			return;
		}
		selectionItem = _protocolHostService.Models.LastOrDefault(n => n.Scan.Status == PerformStatus.Performed);
		if (selectionItem.Scan is not null)
		{
			SelectScan(selectionItem.Frame.Descriptor.Id, selectionItem.Measurement.Descriptor.Id, selectionItem.Scan.Descriptor.Id);
			return;
		}
		// Clear Selection??
		CurrentSelection = default;
		LastSelectionTomoScan = null;
		LastSelectionTopoScan = null;
		CurrentSelectionRecon = null;

		SelectionScanChanged?.Invoke(this, null);
		ReconImageLoaded?.Invoke(this, null);
	}

	public void SelectRecon(ReconModel reconModel)
	{
		if (CurrentSelectionRecon is not null &&
			CurrentSelectionRecon.Descriptor.Id == reconModel.Descriptor.Id)
		{
			return;
		}
		CurrentSelectionRecon = reconModel;
		SelectionReconChanged?.Invoke(this, new EventArgs<ReconModel>(CurrentSelectionRecon));
	}

	public void SelectReconWithImage(ReconModel reconModel, string imagePath)
	{
		CurrentSelectionRecon = reconModel;
		ReconImageLoaded?.Invoke(this, new EventArgs<(ReconModel Recon, string Image)>((CurrentSelectionRecon, imagePath)));
	}

	public void Clear()
	{
		CurrentSelection = default;
		CurrentSelectionRecon = null;
		LastSelectionTopoScan = null;
		LastSelectionTomoScan = null;

		SelectionCleared?.Invoke(this, null);
	}

	public void ClearSelectedRecon()
	{
		CurrentSelectionRecon = null;
	}
}
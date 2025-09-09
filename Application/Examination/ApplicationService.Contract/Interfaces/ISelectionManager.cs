//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.CTS;
using NV.CT.Protocol.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface ISelectionManager
{
	(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan) CurrentSelection { get; }

	ScanModel LastSelectionTopoScan { get; }

	ScanModel LastSelectionTomoScan { get; }

	/// <summary>
	/// 默认方法
	/// </summary>
	void SelectScan();

	void SelectScan(string forId, string measurementId, string scanId);

	event EventHandler<EventArgs<ScanModel>> SelectionScanChanged;

	ReconModel CurrentSelectionRecon { get; }

	void SelectRecon(ReconModel reconModel);
	void SelectReconWithImage(ReconModel reconModel, string imagePath);

	event EventHandler<EventArgs<ReconModel>> SelectionReconChanged;

	void Clear();
	void ClearSelectedRecon();

	event EventHandler? SelectionCleared;
}
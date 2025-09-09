//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/15 13:18:36           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
public interface ITablePositionService
{
	TablePositionInfo CurrentTablePosition { get; }

	GantryPositionInfo CurrentGantryPosition { get; }

	PartStatus CurrentTableStatus { get; }

	PartStatus CurrentGantryStatus { get; }

	event EventHandler<EventArgs<TablePositionInfo>> TablePositionChanged;

	event EventHandler<EventArgs<GantryPositionInfo>> GantryPositionChanged;

	(bool, string) CheckPosition(int horizontal, int vertical);

	bool ValidatePosition(int horizontal, int vertical);

	bool Move(int horizontal, int vertical);

	bool ResetAxisX();

	bool CheckISOCenterWithAxisX();

	(Tuple<int, int> horizontalRange, Tuple<int, int> verticalRange) GetValidRange();

	void CancelMove();

	event EventHandler TableArrived;

	event EventHandler<PartStatus> TableStatusChanged;

	event EventHandler<PartStatus> GantryStatusChanged;

	event EventHandler<bool> TableLocked;
}
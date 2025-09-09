//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Prism.Events;

namespace NV.CT.Recon.Extensions;

/// <summary>
/// pb变化事件
/// </summary>
public class SelectedPlanningBaseChangedEvent : PubSubEvent<ScanReconModel>
{
}

/// <summary>
/// rr变化事件
/// </summary>
public class SelectedReconRangeChangedEvent : PubSubEvent<ScanReconModel>
{
}

/// <summary>
/// 加载完成事件
/// </summary>
public class ReconRangeLoadCompletedEvent : PubSubEvent { }

/// <summary>
/// 加载完成事件
/// </summary>
public class PlanningBaseLoadCompletedEvent : PubSubEvent { }

public class CommandNameChangedEvent : PubSubEvent<CommandModel> { }

public class CommandModel
{
	public string CommandName { get; set; } = string.Empty;
	public ScanReconModel? CommandData { get; set; }
}

/// <summary>
/// 某个重建完成后
/// </summary>
public class ReconFinishedEvent:PubSubEvent<ScanReconModel> { }
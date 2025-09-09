//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/20 9:59:55     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.ProtocolService.Contract;

public class ProtocolPresentationModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsAdult { get; set; }

    public CTS.Enums.BodyPart BodyPart { get; set; }

    public bool IsEnhanced { get; set; }

    public bool IsIntervention { get; set; }

    public PatientPosition PatientPosition { get; set; }

    public bool IsEmergency { get; set; }

    public bool IsFactory { get; set; }

    public string Description { get; set; } = string.Empty;

    public bool IsTopping { get; set; } = false;

    public bool IsDefaultMatch { get; set; } = false;

    public List<MeasurementPresentationModel> Measurements { get; set; } = new();
}
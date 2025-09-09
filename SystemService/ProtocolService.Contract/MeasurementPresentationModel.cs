//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/23 16:19:44    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.ProtocolService.Contract;

public class MeasurementPresentationModel
{
    public string Id { get; set; }

    public string Name { get; set; }

    public List<ScanPresentationModel> Scans { get; set; } = new();
}

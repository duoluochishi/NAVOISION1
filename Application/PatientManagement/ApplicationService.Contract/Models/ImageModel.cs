//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.PatientManagement.ApplicationService.Contract.Models;

public class ImageModel
{
    public string Id { get; set; } = string.Empty;
    public string InternalSeriesId { get; set; } = string.Empty;
    public string ImageNumber { get; set; } = string.Empty;
    public DateTime ImageTime { get; set; }
    public string Path { get; set; } = string.Empty;
}
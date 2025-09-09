//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:54:59    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.PatientBrowser.ApplicationService.Contract.Models;
public class ImageModel
{
    public string PrimaryImageID { get; set; }

    public string ForeignSeriesID { get; set; }

    public string ImageNumber { get; set; }
    public DateTime ImageTime { get; set; }
}
//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/23  13:36:45     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using NV.CT.CTS.Models;

namespace NV.CT.ConfigService.Contract.Models
{
    public class PrintingImageUpdateInfo
    {
        public string StudyId { get; set; } = string.Empty;

        public List<PrintingImageProperty> PrintingImageList { get; set; } = new();


    }
}

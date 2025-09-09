//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/5 14:14:32     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Models;

public class OfflineDiskInfo
{
    public string Name { get; set; }

    public uint TotalSize { get; set; }

    public uint FreeSize { get; set; }

    public double FreeSpaceRate => FreeSize * 100.0 / TotalSize;
}

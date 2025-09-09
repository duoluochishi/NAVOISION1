//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:44:23    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using FellowOakDicom;
using System.Reflection;

namespace NV.CT.DicomUtility.Transfer
{
    public class TransferSyntaxMapper
    {
        public static DicomTransferSyntax GetMappedTransferSyntax(SupportedTransferSyntax syntax)
        {
            const BindingFlags binding = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            var ts = (DicomTransferSyntax)typeof(DicomTransferSyntax).GetField(syntax.ToString(), binding).GetValue(0);
            return ts;
        }
    }
}

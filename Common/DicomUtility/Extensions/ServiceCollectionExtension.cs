//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/11/29 13:28:44     V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using NV.CT.DicomUtility.UtilityConfig;

namespace NV.CT.DicomUtility.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void DicomUtilityConfigInitialization(this IServiceCollection services)
        {
            DicomUtilityConfig.InitializeDicomUtilityConfig();
        }
    }
}

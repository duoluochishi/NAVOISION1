//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/11 9:59:28           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Autofac;

namespace NV.CT.DicomImageViewer;
public class DicomImageViewerlModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<TopoImageViewer>().SingleInstance();
        builder.RegisterType<TomoImageViewer>().SingleInstance();
    }
}
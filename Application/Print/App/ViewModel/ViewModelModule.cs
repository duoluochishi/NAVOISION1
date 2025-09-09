//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.Print.ApplicationService.Contract.Interfaces;
using NV.CT.Print.ApplicationService.Impl;
using NV.CT.Print.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Print.ViewModel
{
    public class ViewModelModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PrintConfigService>().As<IPrintConfigService>().SingleInstance();
            builder.RegisterType<MainPrintControlViewModel>().SingleInstance();
            builder.RegisterType<ImageOperationViewModel>().SingleInstance();
            builder.RegisterType<StudyListViewModel>().SingleInstance();
            builder.RegisterType<SeriesListViewModel>().SingleInstance();
            builder.RegisterType<ProtocolSettingsViewModel>().SingleInstance();

            builder.RegisterType<OperateCommandsViewModel>().SingleInstance();
            builder.RegisterType<PrintCommandsViewModel>().SingleInstance();
            builder.RegisterType<SelectPagesViewModel>().SingleInstance();
            builder.RegisterType<SelectPrinterViewModel>().SingleInstance();
            builder.RegisterType<ImageOverviewViewModel>().SingleInstance();

            builder.RegisterType<CustomWWWLWindow>().SingleInstance();



        }
    }
}

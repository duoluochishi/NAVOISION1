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
global using Autofac;
global using Autofac.Extensions.DependencyInjection;
global using AutoMapper;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using NV.CT.ClientProxy;
global using NV.CT.CTS;
global using NV.CT.CTS.Enums;
global using NV.CT.CTS.Extensions;
global using NV.CT.CTS.Helpers;
global using NV.CT.Logging;
global using NV.CT.PatientManagement.Extensions;
global using NV.CT.PatientManagement.View.English;
global using NV.CT.PatientManagement.ViewModel;
global using NV.CT.PatientManagement.ApplicationService.Impl.Extensions;
global using NV.CT.UI.Controls;
global using NV.CT.UI.Controls.Common;
global using NV.CT.UI.ViewModel;
global using NV.CT.WorkflowService.Contract;
global using Prism.Commands;
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Windows;
global using System.Windows.Controls;
global using System.Windows.Interop;
global using System.Windows.Threading;

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
global using System;
global using System.Collections.ObjectModel;
global using System.Diagnostics;
global using System.IO;
global using System.Linq;
global using System.Windows;
global using System.Windows.Interop;
global using System.Windows.Threading;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Prism.Commands;

global using Autofac;
global using Autofac.Extensions.DependencyInjection;

global using NV.CT.AppService.Contract;
global using NV.CT.ClientProxy;
global using NV.MPS.Communication;
global using NV.CT.ConfigService.Contract;
global using NV.CT.CTS;
global using NV.CT.CTS.Enums;
global using NV.CT.CTS.Extensions;
global using NV.CT.CTS.Helpers;
global using NV.CT.Logging;
global using NV.CT.UI.Controls;
global using NV.CT.UI.Controls.Common;
global using NV.CT.UI.ViewModel;
global using NV.CT.WorkflowService.Contract;




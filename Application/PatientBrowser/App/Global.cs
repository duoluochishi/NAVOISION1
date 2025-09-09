//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.MPS.Communication;
using NV.CT.PatientBrowser.ApplicationService.Impl.Extensions;
using NV.CT.PatientBrowser.ViewModel;
using System;
using System.Linq;
using System.Reflection;

namespace NV.CT.PatientBrowser;

public class Global
{
    public IServiceProvider ServiceProvider { get; set; }
    private static readonly Global instance = new Global();
    public IServiceCollection Services;
    public bool IsAddEmergencyPatient = false;
    public string Illegalcharacter = "^[0-9a-zA-Z\u4E00-\u9FA5]+$";
    public string IllegalcharacterAllowSingleQuotes = "^[0-9a-zA-Z\u4E00-\u9FA5']+$";
    public string IllegalcharacterAllowQuotes = @"^[0-9a-zA-Z\u4E00-\u9FA5'+\-=\._]+$";
    public string IllegalcharacterAllowUnderline = "^[0-9a-zA-Z\u4E00-\u9FA5_]+$";

    public static Global Instance
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// 注册ViewModel模型
    /// </summary>
    public static void RegisterTypes()
    {
        ContainerBuilder builder = new ContainerBuilder();
        builder.AddApplicationServiceContainer();
        builder.RegisterType<WorkListViewModel>().SingleInstance();
        builder.RegisterType<PatientInfoViewModel>().SingleInstance();
    }

    /// <summary>
    ///克隆
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public static void Clone(object source, object target)
    {
        var sourceType = source.GetType();
        var targetType = target.GetType();
        var sourceProps = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var targetProps = (from t in targetType.GetProperties()
                           where t.CanWrite && (t.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                           select t).ToList();
        foreach (var prop in sourceProps)
        {
            var value = prop.GetValue(source, null);
            var tProp = targetProps
                .FirstOrDefault(p => p.Name == prop.Name &&
                    p.PropertyType.IsAssignableFrom(prop.PropertyType));
            if (tProp != null)
                tProp.SetValue(target, value, null);
        }
    }

    private static MCSServiceClientProxy? _serviceClientProxy;
    private static ClientInfo? _clientInfo;

    private SyncServiceClientProxy? _syncClientProxy;

    public void Subscribe()
    {
        var tag = $"[PatientBrowser]-{DateTime.Now:yyyyMMddHHmmss}";
        _clientInfo = new() { Id = $"{tag}" };

        _serviceClientProxy = Program.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);

        _syncClientProxy = ServiceProvider?.GetRequiredService<SyncServiceClientProxy>();
        _syncClientProxy?.Subscribe(_clientInfo);
    }

    public void Unsubscribe()
    {
        _serviceClientProxy?.Unsubscribe(_clientInfo);
        _syncClientProxy?.Unsubscribe(_clientInfo);
    }
}
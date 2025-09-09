//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using System.Windows.Media;

namespace NV.CT.ConfigManagement;

public class Global
{
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    private ClientInfo? _clientInfo;
    private MCSServiceClientProxy? _serviceClientProxy;
    private JobClientProxy? _jobClientProxy;

    public string ModelName = string.Empty;

    public static Global Instance => _instance.Value;

    private Global()
    {       
    }

    public void Subscribe()
    {
        _clientInfo = new ClientInfo { Id = $"[InterventionScan]_{IdGenerator.Next(0)}" };
        _serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);

        _jobClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<JobClientProxy>();
        _jobClientProxy?.Subscribe(_clientInfo);
    }

    public void Unsubscribe()
    {
        if (_clientInfo != null)
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
            _jobClientProxy?.Unsubscribe(_clientInfo);
        }
    }  

    public static List<T> FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
    {
        try
        {
            var TList = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    TList.Add((T)child);
                    List<T> childOfChildren = FindVisualChild<T>(child);
                    if (childOfChildren != null)
                    {
                        TList.AddRange(childOfChildren);
                    }
                }
                else
                {
                    if (child != null)
                    {
                        var childOfChildren = FindVisualChild<T>(child);
                        if (childOfChildren != null)
                        {
                            TList.AddRange(childOfChildren);
                        }
                    }
                }
            }
            return TList;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            return new List<T>();
        }
    }
}
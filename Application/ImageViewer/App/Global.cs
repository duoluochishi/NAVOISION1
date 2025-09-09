//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Threading.Tasks;

namespace NV.CT.ImageViewer;

public class Global
{
    private static readonly Lazy<Global> _instance = new(() => new Global());

    private ClientInfo? _clientInfo;
    private MCSServiceClientProxy? _serviceClientProxy;
    private JobClientProxy? _jobClientProxy;
    private PrintConfigClientProxy? _printConfigClientProxy;

    public static Global Instance => _instance.Value;

    public string StudyId { get; set; } = string.Empty;

    public string SeriesId { get; set; } = string.Empty;

    private Global()
    {
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

    public void Subscribe()
    {
        _clientInfo = new ClientInfo { Id = $"[Viewer]_{IdGenerator.Next()}" };
        _serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);

        _jobClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<JobClientProxy>();
        _jobClientProxy?.Subscribe(_clientInfo);

        _printConfigClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<PrintConfigClientProxy>();
        //由于此时Print进程可能尚未启动，所以此处采用异步尝试的方式避免堵塞进程ImageViewer的启动
        Task.Run( () => { this.SubscribePrintConfigService(); });        
    }

    public void SubscribePrintConfigService()
    {
        if (this.CheckConnectivityOfPrintConfig())
        {
            _printConfigClientProxy?.Subscribe(_clientInfo);
        }
    }

    public void Unsubscribe()
    {
        if (_clientInfo != null)
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
            _jobClientProxy?.Unsubscribe(_clientInfo);
            if (this.CheckConnectivityOfPrintConfig())
            {
                _printConfigClientProxy?.Unsubscribe(_clientInfo);
            }                
        }
    }

    public bool CheckConnectivityOfPrintConfig()
    {
        return _printConfigClientProxy.IsConnected;
    }
    public string ParseParameters(string parameters)
    {
        string splitParameter = string.Empty;
        if (parameters.Contains(","))
        {
            splitParameter = parameters.Split(",")[0];
        }
        else
        {
            splitParameter = parameters;
        }
        return splitParameter;
    }
    public string GetSeriesId(string parameters)
    {
        string splitParameter = string.Empty;
        if (parameters.Contains(","))
        {
            splitParameter = parameters.Split(",")[1];
        }
        else
        {
            splitParameter = "";
        }
        return splitParameter;
    }
}
namespace NV.CT.RGT;

public class Global
{
    private static readonly Lazy<Global> _instance = new(() => new Global());
    private ClientInfo? _clientInfo;
    private SyncServiceClientProxy? _syncClientProxy;
    public static Global Instance => _instance.Value;
    private Global()
    {
    }

    public void Initialize()
    {
        try
        {
        }
        catch (Exception ex)
        {
            CTS.Global.Logger?.LogError("RGT Global Error {0}", ex);
        }
    }

    public void Subscribe()
    {
        _clientInfo = new ClientInfo { Id = $"[RGT]_{IdGenerator.Next()}" };

        _syncClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<SyncServiceClientProxy>();
        _syncClientProxy?.Subscribe(_clientInfo);
    }

    public void Unsubscribe()
    {
        if (_clientInfo != null)
        {
            _syncClientProxy?.Unsubscribe(_clientInfo);
        }
    }
}
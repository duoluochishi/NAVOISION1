namespace NV.CT.ServiceFramework;

public class Global
{
    private static Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    private Global()
    {
    }

    public static Global Instance => _instance.Value;

    public IServiceProvider ServiceProvider { get; set; }

    public IntPtr MainWindowHwnd { get; set; }
}

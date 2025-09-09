//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.Communication;

namespace NV.CT.PatientManagement;

public class Global
{
    public IServiceProvider ServiceProvider { get; set; }
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());
    public IServiceCollection? Services;
    public bool IsAddEmergencyPatient = false;
    private static ClientInfo? clientInfo;
    private static JobClientProxy? _jobClientProxy;
    private static MCSServiceClientProxy? _serviceClientProxy;

    public static Global Instance => _instance.Value;
#pragma warning disable CS8618
    private Global()
#pragma warning restore CS8618
    {
    }
    public void Subscribe()
    {
        var tag = $"[PatientManagement]-{DateTime.Now:yyyyMMddHHmmss}";
        clientInfo = new() { Id = tag };

        _serviceClientProxy = Program.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(clientInfo);

        _jobClientProxy = Program.ServiceProvider?.GetRequiredService<JobClientProxy>();
        _jobClientProxy?.Subscribe(clientInfo);
    }

    public void Unsubscribe()
    {
        if (clientInfo != null)
        {
            _serviceClientProxy?.Unsubscribe(clientInfo);
            _jobClientProxy?.Unsubscribe(clientInfo);
        }
    }

}
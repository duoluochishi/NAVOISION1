
namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension
{
    public class ProtocolAdjustmentOnFORChange : IHostedService
    {
        private IProtocolHostService _protocolHostService;

        public ProtocolAdjustmentOnFORChange(IProtocolHostService protocolHostService) {
            _protocolHostService = protocolHostService;
            _protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
        }

        private void ProtocolHostService_StructureChanged(object? sender, CTS.EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
        {        
            if (e.Data.ChangeType is StructureChangeType.Delete)        //删除导致的结构变化
            {
                return;
            }
            if(e.Data.ChangeType is StructureChangeType.Add && e.Data.Current is not ProtocolModel) //添加扫描、recon、Measurement导致的结构变化。
            {
                return;
            }

            //此时处理的可能情况： Add、Replace Protocol, ChangeFOR
            //TODO：逻辑还是有点问题，比如扫描一半时添加新的扫描协议，此时会导致已plan的协议重置。逻辑待完善。
            var changingList = ScanReconDirectionHelper.AdjustAllScanReconDirections(_protocolHostService);                 
            _protocolHostService.SetParameters(changingList);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

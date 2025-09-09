using NV.CT.Service.AutoCali.Model;
using System.Collections.ObjectModel;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    /// <summary>
    /// 校准协议任务视图模型，按照多层次组织：校准场景-校准项目-校准协议-扫描重建参数
    /// </summary>
    public class CaliProtocolTaskViewModel : CaliTaskViewModel<CalibrationProtocol>
    {
        public CaliProtocolTaskViewModel(CalibrationProtocol caliProtocol) : base(caliProtocol)
        {
            SubTaskViewModels = new ObservableCollection<ICaliTaskViewModel>();
            foreach (var item in caliProtocol?.HandlerGroup)
            {
                var subViewModel = new CaliScanTaskViewModel(item);
                subViewModel.Parent = this;

                SubTaskViewModels.Add(subViewModel);
            }
        }
    }
}

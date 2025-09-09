using NV.CT.Service.AutoCali.Model;
using System.Collections.ObjectModel;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    public class CaliItemTaskViewModel : CaliTaskViewModel<CalibrationItem>
    {
        public CaliItemTaskViewModel(CalibrationItem caliItem) : base(caliItem)
        {
            SubTaskViewModels = new ObservableCollection<ICaliTaskViewModel>();
            foreach (var item in caliItem.CalibrationProtocolGroup)
            {
                var subViewModel = new CaliProtocolTaskViewModel(item);
                subViewModel.Parent = this;

                SubTaskViewModels.Add(subViewModel);
            }
        }
    }
}

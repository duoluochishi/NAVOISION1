using NV.CT.UI.ViewModel;

namespace NV.CT.LogManagement.Models
{
    public class ModuleModel : BaseViewModel
    {
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private string _displayText;
        public string DisplayText
        {
            get => _displayText ?? string.Empty;
            set => SetProperty(ref _displayText, value);
        }

        private string _valueText;
        public string ValueText
        {
            get => _valueText ?? string.Empty;
            set => SetProperty(ref _valueText, value);
        }

    }
}

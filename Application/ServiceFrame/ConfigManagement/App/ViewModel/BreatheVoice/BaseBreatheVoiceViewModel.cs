
namespace NV.CT.ConfigManagement.ViewModel
{
    public class BaseBreatheVoiceViewModel : BaseViewModel
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private bool _isDefault;
        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        private bool _isFactory;
        public bool IsFactory
        {
            get => _isFactory;
            set => SetProperty(ref _isFactory, value);
        }

        private double _time;
        public double Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }

        private string _language;
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        private ObservableCollection<BaseBreatheVoiceViewModel> _breatheVoices = new ObservableCollection<BaseBreatheVoiceViewModel>();
        public ObservableCollection<BaseBreatheVoiceViewModel> BreatheVoices
        {
            get => _breatheVoices;
            set => SetProperty(ref _breatheVoices, value);
        }

        private int _Count;
        public int Count
        {
            get => _Count;
            set => SetProperty(ref _Count, value);
        }

        public int Id { get; set; }
        public string FilePath { get; set; }
    }
}

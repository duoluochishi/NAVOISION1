using CommunityToolkit.Mvvm.ComponentModel;

namespace NV.CT.Service.HardwareTest.Models.Integrations.ImageChain
{
    public partial class ImageChainTestingScenario : ObservableObject
    {
        public ImageChainTestingScenario(string name)
        {
            this.Name = name;        
        }

        [ObservableProperty]
        private string name = string.Empty;
    }
}

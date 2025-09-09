using NV.CT.UI.ViewModel;

namespace NV.CT.UI.Controls.Export
{
    public class ComboItemModel : BaseViewModel
    {
        private string text;
        public string Text
        {
            get => text;
            set => SetProperty(ref text, value);
        }

        private string value;
        public string Value
        {
            get => value;
            set => SetProperty(ref this.value, value);
        }

        public ComboItemModel(string text, string value)
        {
            this.text = text;
            this.value = value;
        }
    }
}

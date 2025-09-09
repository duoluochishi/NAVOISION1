namespace NV.CT.Service.QualityTest.Models
{
    public class ParamDisplayItemModel
    {
        public string Header { get; set; } = string.Empty;
        public string BindingPath { get; set; } = string.Empty;
        public double MinWidth { get; set; } = 50;
        public string Width { get; set; } = "auto";
    }
}
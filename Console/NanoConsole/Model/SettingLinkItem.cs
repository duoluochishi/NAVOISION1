//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.Model;

public class SettingLinkItem : BaseViewModel
{
    public string ItemIndex { get; set; } = string.Empty;
    public string AppCode { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ToolTip { get; set; } = string.Empty;

    private bool _isEnabled = false;
    public bool IsEnabled { get { return _isEnabled; } set { SetProperty(ref _isEnabled, value); } }
    public string ItemType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string StartMode { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string CommandParameters { get; set; } = string.Empty;
    public string IconName { get; set; } = string.Empty;

    public System.Windows.Media.Geometry? IconGeometry { get; set; }

    public void SetIcon(System.Windows.ResourceDictionary ownerResourceDictionary)
    {
        IconGeometry = ownerResourceDictionary[IconName] as System.Windows.Media.Geometry;
    }
}
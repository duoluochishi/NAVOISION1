using System.Windows.Media;

namespace NV.CT.AuxConsole.View.English;

/// <summary>
/// not used anymore
/// </summary>
public partial class ReconCardControl : UserControl
{
	public ReconCardControl()
	{
		InitializeComponent();
	}

	public string StudyId { get; set; } = string.Empty;

	public int ProcessId { get; set; } = 0;

	public StreamGeometry CardGeometry { get; set; }
}

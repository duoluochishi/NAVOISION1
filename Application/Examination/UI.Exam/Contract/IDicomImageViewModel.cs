using NV.CT.DicomImageViewer;

namespace NV.CT.UI.Exam.Contract;

public interface IDicomImageViewModel
{
	public TomoImageViewer TomoImageViewer { get; set; }
}
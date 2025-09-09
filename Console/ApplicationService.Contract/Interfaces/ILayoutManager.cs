using NV.CT.Console.ApplicationService.Contract.Models;

namespace NV.CT.Console.ApplicationService.Contract.Interfaces;

public interface ILayoutManager
{
	void Goto(Screens view);

	void Back();

	event EventHandler<Screens>? LayoutChanged;
}
namespace NV.CT.Console.ApplicationService.Impl;

public class LayoutManager : ILayoutManager
{
	public event EventHandler<Screens>? LayoutChanged;
	private Screens? _currentScreen;
	private Screens? _prevScreen;
	public void Back()
	{
		if (_prevScreen != null)
		{
			LayoutChanged?.Invoke(this, (Screens)_prevScreen);
			_currentScreen = _prevScreen;
			_prevScreen = null;
		}
	}

	public void Goto(Screens screen)
	{
		_prevScreen = _currentScreen;
		_currentScreen = screen;
		LayoutChanged?.Invoke(this, screen);
	}
}


namespace NV.CT.RGT.View;

public partial class MainWindow
{
    private readonly IStateService _stateService;
    public MainWindow(IStateService stateService)
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        _stateService = stateService;

        _stateService.AnimationFinished += StateService_AnimationFinished;
    }

    [UIRoute]
    private void StateService_AnimationFinished(object? sender, EventArgs e)
    {
        WelcomeControl.Visibility = Visibility.Collapsed;
        MainControl.Visibility = Visibility.Visible;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Environment.Exit(0);
    }
}
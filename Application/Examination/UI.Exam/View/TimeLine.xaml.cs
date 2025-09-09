namespace NV.CT.UI.Exam.View;

public partial class Timeline
{
    public Timeline()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<TimelineViewModel>();

        this.Loaded += Timeline_Loaded;
    }

    private void Timeline_Loaded(object sender, RoutedEventArgs e)
    {
        Global.ServiceProvider.GetRequiredService<TimelineViewModel>()._listView = this.lvTimeline;
        Global.ServiceProvider.GetRequiredService<TimelineViewModel>().IsArrowEnable = Global.ServiceProvider.GetRequiredService<TimelineViewModel>().GetTotalWidth();
    }
}
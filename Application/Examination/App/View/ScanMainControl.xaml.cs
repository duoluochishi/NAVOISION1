//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------


using NV.CT.Examination.Layout;
using NV.CT.Examination.ViewModel;

namespace NV.CT.Examination.View;

public partial class ScanMainControl
{
    private readonly ILogger<ScanMainControl>? _logger;
    private readonly ILayoutManager? _layoutManager;
    private readonly IScreenSync? _screenSync;

    public ScanMainControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<ScanMainViewModel>();

        foreach (ResourceDictionary resourceDictionary in LoadingResource.LoadingInControl())
        {
            Resources.MergedDictionaries.Add(resourceDictionary);
        }

        _logger = CTS.Global.ServiceProvider?.GetRequiredService<ILogger<ScanMainControl>>();
        _layoutManager = CTS.Global.ServiceProvider?.GetRequiredService<ILayoutManager>();
        if (_layoutManager is not null)
            _layoutManager.LayoutChanged += LayoutChanged;

        _screenSync = CTS.Global.ServiceProvider?.GetRequiredService<IScreenSync>();
        if (_screenSync is not null)
        {
            _screenSync.ScreenChanged += SyncScreenChanged;
        }

        InitContentView();
    }

    //Exam进程同步 平板的页面跳转事件
    private void SyncScreenChanged(object? sender, string e)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            var screen = Enum.Parse<SyncScreens>(e);

            _logger?.LogInformation($"sync screen changed to {screen}");

            //screen in examination

            if (screen == SyncScreens.ProtocolSelection && _layoutManager?.CurrentLayout != ScanTaskAvailableLayout.ProtocolSelection)
            {
                //平板的协议选择页面，对应Exam里面的 协议选择页面
                _layoutManager?.SwitchToView(ScanTaskAvailableLayout.ProtocolSelection);
            }
            else if (screen == SyncScreens.ScanDefault && _layoutManager?.CurrentLayout != ScanTaskAvailableLayout.ScanDefault)
            {
                //平板的协议选择页面，对应Exam里面的 协议选择页面
                _layoutManager?.SwitchToView(ScanTaskAvailableLayout.ScanDefault);
            }
            //screen in NanoConsole
        });
    }

    private void InitContentView()
    {
        var studyService = CTS.Global.ServiceProvider?.GetService<IStudyService>();
        var studyHostService = CTS.Global.ServiceProvider?.GetService<IStudyHostService>();

        if (studyService is not null && studyHostService is not null)
        {
            var (studyModel, _) = studyService.Get(studyHostService.StudyId);
            if (studyModel is not null && studyModel.PatientType == (int)PatientType.Emergency)
            {
                //急诊
                LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<ScanDefaultControl>();

                //急诊默认需要触发Scan
                CTS.Global.ServiceProvider?.GetService<ISelectionManager>()?.SelectScan();
            }
            else if (studyModel is not null && !string.IsNullOrEmpty(studyModel.Protocol))
            {     
                //恢复检查
                LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<ScanDefaultControl>();
                //恢复检查
                CTS.Global.ServiceProvider?.GetService<ISelectionManager>()?.SelectScan();
            }
            else
            {
                LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<ProtocolSelectionControl>();
            }
        }
        else
        {
            //默认加载 ProtocolSelection 布局
            LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<ProtocolSelectionControl>();
        }
    }

    private void LayoutChanged(object? sender, CTS.EventArgs<ScanTaskAvailableLayout> e)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            UserControl? uc = null;
            switch (e.Data)
            {
                case ScanTaskAvailableLayout.ScanDefault:
                    uc = CTS.Global.ServiceProvider?.GetRequiredService<ScanDefaultControl>();
                    break;
                case ScanTaskAvailableLayout.ProtocolSelection:
                    uc = CTS.Global.ServiceProvider?.GetRequiredService<ProtocolSelectionControl>();
                    break;
                case ScanTaskAvailableLayout.Recon:
                    uc = CTS.Global.ServiceProvider?.GetRequiredService<ReconControl>();
                    break;
            }

            if (uc is null)
                return;

            LayoutContainer.Content = uc;
        });
        //var controlType = Type.GetType($"{nameof(NV)}.{nameof(CT)}.{nameof(Examination)}.{nameof(App)}.{nameof(View)}.{nameof(Layout)}.{page}Control");
        //if (controlType is null)
        //{
        //    _logger?.LogError($"ScanMainControl controlType resolve failed");
        //    return;
        //}
        //var viewControl = _serviceProvider?.GetRequiredService(controlType);
        //LayoutContainer.Content = viewControl;
    }

}